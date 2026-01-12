using CoreLedger.Domain.Cadastros.Entities;
using CoreLedger.Domain.Entities;
using CoreLedger.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS0618 // Type or member is obsolete - Fund is deprecated but still used for legacy support

namespace CoreLedger.Infrastructure.Persistence;

/// <summary>
///     Contexto de banco de dados da aplicação com configuração apropriada para PostgreSQL.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AccountType> AccountTypes => Set<AccountType>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CoreJob> CoreJobs => Set<CoreJob>();
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<Fundo> Fundos => Set<Fundo>();
    public DbSet<FundoClasse> FundoClasses => Set<FundoClasse>();
    public DbSet<FundoSubclasse> FundoSubclasses => Set<FundoSubclasse>();
    public DbSet<FundoTaxa> FundoTaxas => Set<FundoTaxa>();
    public DbSet<FundoTaxaPerformance> FundoTaxasPerformance => Set<FundoTaxaPerformance>();
    public DbSet<FundoPrazo> FundoPrazos => Set<FundoPrazo>();
    public DbSet<FundoPrazoExcecao> FundoPrazoExcecoes => Set<FundoPrazoExcecao>();
    public DbSet<Instituicao> Instituicoes => Set<Instituicao>();
    public DbSet<ClassificacaoAnbima> ClassificacoesAnbima => Set<ClassificacaoAnbima>();
    public DbSet<FundoVinculo> FundoVinculos => Set<FundoVinculo>();
    public DbSet<FundoParametrosFIDC> FundoParametrosFIDC => Set<FundoParametrosFIDC>();
    public DbSet<FundoParametrosCota> FundoParametrosCota => Set<FundoParametrosCota>();
    public DbSet<Security> Securities => Set<Security>();
    public DbSet<TransactionStatus> TransactionStatuses => Set<TransactionStatus>();
    public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
    public DbSet<TransactionSubType> TransactionSubTypes => Set<TransactionSubType>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<TransactionIdempotency> TransactionIdempotencies => Set<TransactionIdempotency>();
    public DbSet<TransactionCreatedOutboxMessage> TransactionCreatedOutboxMessages => Set<TransactionCreatedOutboxMessage>();
    public DbSet<Calendario> Calendarios => Set<Calendario>();
    public DbSet<Indexador> Indexadores => Set<Indexador>();
    public DbSet<HistoricoIndexador> HistoricosIndexadores => Set<HistoricoIndexador>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}