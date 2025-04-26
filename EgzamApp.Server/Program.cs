using EgzamApp.Server.Data;
using EgzamApp.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=egzamapp.db"));

// Register our custom services
builder.Services.AddScoped<ExamService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to handle file uploads properly
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EgzamApp API", Version = "v1" });
    
    // Add support for file uploads in Swagger
    c.OperationFilter<Swashbuckle.AspNetCore.SwaggerGen.FileUploadOperationFilter>();
    
    // Define security scheme for file uploads
    c.AddSecurityDefinition("multipart/form-data", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "multipart/form-data"
    });
});

var app = builder.Build();

// Apply database migrations with improved error handling
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Initializing database...");
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // This will create the database if it doesn't exist
        context.Database.EnsureCreated();
        
        // Check if tables exist and create schema
        if (!context.Database.CanConnect())
        {
            logger.LogWarning("Cannot connect to database. Creating one...");
            context.Database.EnsureCreated();
        }
        
        // Manually ensure tables are created based on our models
        if (!context.Exams.Any() && !TableExists(context, "Exams"))
        {
            logger.LogInformation("Creating database schema...");
            context.Database.EnsureDeleted(); // Optional: remove any old DB
            context.Database.EnsureCreated(); // Create fresh schema
        }
        
        logger.LogInformation("Database initialization complete.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EgzamApp API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

// Helper method to check if a table exists
bool TableExists(ApplicationDbContext context, string tableName)
{
    try
    {
        // Try to query the table
        var conn = context.Database.GetDbConnection();
        using var command = conn.CreateCommand();
        command.CommandText = $"SELECT 1 FROM sqlite_master WHERE type='table' AND name='{tableName}';";
        
        if (conn.State != System.Data.ConnectionState.Open)
            conn.Open();
            
        using var reader = command.ExecuteReader();
        return reader.HasRows;
    }
    catch
    {
        return false;
    }
}

// File upload operation filter for Swagger
namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFileCollection))
                .ToList();

            if (fileParameters.Any())
            {
                // If the operation has file parameters, set the proper content type
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = fileParameters.ToDictionary(
                                    param => param.Name ?? "file",
                                    param => new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                )
                            }
                        }
                    }
                };
            }
        }
    }
}
