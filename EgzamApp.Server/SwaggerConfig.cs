using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace EgzamApp.Server
{
    /// <summary>
    /// Configuration for Swagger
    /// </summary>
    public static class SwaggerConfig
    {
        /// <summary>
        /// Configures Swagger for the application
        /// </summary>
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "EgzamApp API",
                    Version = "v1",
                    Description = "API for the EgzamApp training exam application",
                    Contact = new OpenApiContact
                    {
                        Name = "EgzamApp Support",
                        Email = "support@egzamapp.example.com"
                    }
                });

                // Use custom schema IDs to avoid conflicts
                c.CustomSchemaIds(type => type.FullName);

                // Handle circular references
                c.DocumentFilter<SwaggerDocumentFilter>();

                // Handle collections
                c.SchemaFilter<EnumerableSchemaFilter>();
                
                // Add operation descriptions
                c.OperationFilter<SwaggerDefaultValues>();
            });
        }
    }

    /// <summary>
    /// Document filter to handle circular references
    /// </summary>
    public class SwaggerDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Process all schemas to remove circular references
            var schemas = swaggerDoc.Components.Schemas.ToList();
            foreach (var item in schemas)
            {
                var schema = item.Value;
                ProcessSchema(schema, new HashSet<string>());
            }
        }

        private void ProcessSchema(OpenApiSchema schema, HashSet<string> visited)
        {
            if (schema == null) return;

            // If we've seen this schema before or it has no properties, return
            if (schema.Properties == null || !schema.Properties.Any()) return;

            // Process each property
            var properties = schema.Properties.ToList();
            foreach (var prop in properties)
            {
                var propSchema = prop.Value;
                if (propSchema.Reference != null && visited.Contains(propSchema.Reference.Id))
                {
                    // Circular reference detected, remove reference and replace with simple type
                    propSchema.Reference = null;
                    propSchema.Type = "object";
                    propSchema.Properties = new Dictionary<string, OpenApiSchema>();
                }
                else if (propSchema.Reference != null)
                {
                    // Track this reference
                    visited.Add(propSchema.Reference.Id);
                }

                // Process arrays
                if (propSchema.Type == "array" && propSchema.Items != null)
                {
                    ProcessSchema(propSchema.Items, visited);
                }

                // Process nested properties
                ProcessSchema(propSchema, visited);
            }
        }
    }

    /// <summary>
    /// Schema filter to handle enumerable types
    /// </summary>
    public class EnumerableSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(List<string>))
            {
                schema.Type = "array";
                schema.Items = new OpenApiSchema { Type = "string" };
            }
        }
    }

    /// <summary>
    /// Operation filter to set default values
    /// </summary>
    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Add response descriptions
            foreach (var response in operation.Responses)
            {
                response.Value.Description ??= response.Key switch
                {
                    "200" => "Success",
                    "201" => "Created",
                    "400" => "Bad Request",
                    "401" => "Unauthorized",
                    "403" => "Forbidden",
                    "404" => "Not Found",
                    "500" => "Server Error",
                    _ => response.Value.Description
                };
            }
        }
    }
}
