using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CoreLedger.API.Extensions;

/// <summary>
/// Document filter to add server URLs to the OpenAPI document
/// </summary>
public class AddServersDocumentFilter : IDocumentFilter
{
    public void Apply(Microsoft.OpenApi.OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Servers = new List<Microsoft.OpenApi.OpenApiServer>
        {
            new Microsoft.OpenApi.OpenApiServer
            {
                Url = "https://localhost:7109",
                Description = "Local development (HTTPS)"
            },
            new Microsoft.OpenApi.OpenApiServer
            {
                Url = "http://localhost:5071",
                Description = "Local development (HTTP)"
            }
        };
    }
}

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Core Ledger API",
                Version = "v1",
                Description = "Financial Ledger REST API for institutional financial clients",
                Contact = new OpenApiContact
                {
                    Name = "Core Ledger Team",
                    Email = "support@coreledger.com"
                }
            });

            // Add server URLs for API client generation
            options.DocumentFilter<AddServersDocumentFilter>();

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
            });

            options.AddSecurityRequirement(doc =>
            {
                var scheme = new OpenApiSecuritySchemeReference("Bearer", doc);
                return new OpenApiSecurityRequirement
                {
                    { scheme, new List<string>() }
                };
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);

            options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Core Ledger API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Core Ledger API Documentation";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
        });

        return app;
    }
}