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

/// <summary>
/// Operation filter to configure file download responses with proper binary schema.
/// This enables NSwag to generate FileResponse types for file download endpoints.
/// </summary>
public class FileDownloadOperationFilter : IOperationFilter
{
    private static readonly HashSet<string> FileContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "text/csv",
        "application/octet-stream",
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/zip"
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Responses == null) return;

        foreach (var response in operation.Responses.Values)
        {
            if (response?.Content == null) continue;

            foreach (var content in response.Content)
            {
                if (content.Value != null && FileContentTypes.Contains(content.Key))
                {
                    // Set binary schema for file responses
                    content.Value.Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "binary"
                    };
                }
            }
        }
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

            // Configure file download responses with binary schema
            options.OperationFilter<FileDownloadOperationFilter>();

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