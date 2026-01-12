using AutoMapper;
using CoreLedger.Application.DTOs;
using CoreLedger.Application.Interfaces;
using CoreLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoreLedger.Infrastructure.Services;

/// <summary>
///     Service for managing user lifecycle and synchronization with Auth0.
/// </summary>
public class UserService : IUserService
{
    private readonly IAuth0Service _auth0Service;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(
        IApplicationDbContext context,
        IAuth0Service auth0Service,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _context = context;
        _auth0Service = auth0Service;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto> EnsureUserExistsAsync(
        string authProviderId,
        string provider,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Ensuring user exists: AuthProviderId={AuthProviderId}, Provider={Provider}",
            authProviderId,
            provider);

        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.AuthProviderId == authProviderId && u.Provider == provider, cancellationToken);

        if (existingUser != null)
        {
            _logger.LogInformation(
                "User found in database: Id={UserId}, Email={Email}",
                existingUser.Id,
                existingUser.Email);

            // User exists - fetch fresh profile from Auth0 to keep in sync
            var auth0Profile = await _auth0Service.GetUserProfileAsync(
                accessToken,
                cancellationToken);

            existingUser.UpdateLoginInfo(auth0Profile.Email, auth0Profile.Name);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Updated existing user from Auth0: Id={UserId}, Email={Email}",
                existingUser.Id,
                existingUser.Email);

            return _mapper.Map<UserDto>(existingUser);
        }

        // User doesn't exist - create new user from Auth0 profile
        _logger.LogInformation(
            "User not found in database, creating from Auth0: {AuthProviderId}",
            authProviderId);

        var profile = await _auth0Service.GetUserProfileAsync(accessToken, cancellationToken);

        var newUser = User.Create(
            profile.Sub,
            provider,
            profile.Sub, // User creates themselves on first login
            profile.Email,
            profile.Name);

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created new user from Auth0: Id={UserId}, Email={Email}",
            newUser.Id,
            newUser.Email);

        return _mapper.Map<UserDto>(newUser);
    }

    public async Task<UserDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync([id], cancellationToken);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetByAuthProviderIdAsync(
        string authProviderId,
        string provider,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.AuthProviderId == authProviderId && u.Provider == provider, cancellationToken);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }
}