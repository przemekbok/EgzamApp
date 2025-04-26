using EgzamApp.Server.Data;
using EgzamApp.Server.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=egzamapp.db"));

// Register our custom services
builder.Services.AddScoped<ExamService>();

// Improve JSON serialization with Enum conversion
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add Swagger with more robust error handling
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "EgzamApp API", 
        Version = "v1",
        Description = "API for the EgzamApp exam application"
    });
    
    // Explicitly exclude schema for Options property which is a List<string>
    // This can sometimes cause issues with Swagger
    c.SchemaFilter<OptionSchemaFilter>();
    
    // Add better error handling
    c.CustomSchemaIds(type => type.FullName);
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
    try 
    {
        app.UseSwagger(c =>
        {
            c.SerializeAsV2 = false;
        });
        
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EgzamApp API v1");
            c.RoutePrefix = "swagger";
        });
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error configuring Swagger");
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();

// Schema filter to handle Options property
public class OptionSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (context.Type == typeof(List<string>))
        {
            schema.Type = "array";
            schema.Items = new OpenApiSchema { Type = "string" };
        }
    }
}
