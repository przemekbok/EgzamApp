using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EgzamApp.Server
{
    /// <summary>
    /// Extension methods for configuring Swagger
    /// </summary>
    public static class SwaggerConfiguration
    {
        /// <summary>
        /// Configures Swagger for the application
        /// </summary>
        public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EgzamApp API",
                    Version = "v1",
                    Description = "API for the EgzamApp application to manage exams and user progress",
                    Contact = new OpenApiContact
                    {
                        Name = "Support",
                        Email = "support@example.com"
                    }
                });

                // Use Swagger annotations
                c.EnableAnnotations();

                // Configure operation IDs
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null;
                });

                // Configure file upload (for IFormFile parameters)
                c.OperationFilter<FileUploadOperationFilter>();

                // Properly document enum values
                c.SchemaFilter<EnumSchemaFilter>();
            });

            return services;
        }
    }

    /// <summary>
    /// Filter to correctly handle file uploads in Swagger
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) || 
                           (p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(List<>) && p.ParameterType.GetGenericArguments()[0] == typeof(IFormFile)));

            foreach (var fileParameter in fileParameters)
            {
                var parameter = operation.Parameters.FirstOrDefault(p => p.Name == fileParameter.Name);
                if (parameter != null)
                {
                    operation.Parameters.Remove(parameter);
                }

                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    [fileParameter.Name] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                },
                                Required = new HashSet<string> { fileParameter.Name }
                            }
                        }
                    }
                };
            }
        }
    }

    /// <summary>
    /// Filter to correctly document enum values in Swagger
    /// </summary>
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum.Clear();
                schema.Type = "string";
                
                foreach (var name in Enum.GetNames(context.Type))
                {
                    schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(name));
                }
            }
        }
    }
}
