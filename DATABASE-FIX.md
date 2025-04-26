# Database Fix for EgzamApp

This document explains the fixes made to resolve the database error: `SQLite Error 1: 'no such table: Exams'` that occurred when uploading exam files.

## Issue Summary

The error occurred because the database tables weren't being properly created. This happens when Entity Framework Core's database migration system isn't properly set up or when the migrations aren't being applied correctly.

## Fixes Applied

### 1. Improved Database Initialization

Updated the Program.cs file to include more robust database initialization:

```csharp
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
```

### 2. Enhanced Entity Configuration

Improved the ApplicationDbContext to provide better entity configuration:

- Added explicit entity configurations for all models
- Added proper relationship definitions
- Added improved error handling and logging
- Configured proper handling for the Options collection in Question entity

### A Special Note About Options Collection

The Question entity has an Options collection (List<string>) which SQLite doesn't natively support. We added a special JSON converter to serialize this collection:

```csharp
// Configure Question entity
modelBuilder.Entity<Question>(entity =>
{
    // ...other configuration...
    
    // Use the extension method to configure options as JSON
    entity.ConfigureQuestion();
});
```

### 3. Added Diagnostics Controller

Added a new API controller to help diagnose database issues:

- `GET /api/diagnostics/db-status` - Shows database connection status and table information
- `POST /api/diagnostics/initialize` - Forces database re-initialization

## How to Verify the Fix

1. Restart the backend application
2. Check the logs for database initialization messages
3. Access the diagnostics endpoint to verify the database is properly set up:
   - `GET http://localhost:5080/api/diagnostics/db-status`
4. If tables aren't created, you can force initialization:
   - `POST http://localhost:5080/api/diagnostics/initialize`
5. Try uploading a JSON exam file again

## Common Issues

If you still encounter problems:

1. **Database File Location**: Make sure the application has write access to the directory where the SQLite database file is created.

2. **Connection String**: Verify the connection string in appsettings.json is correct.

3. **File Permissions**: Check that the user running the application has sufficient permissions to create and write to the database file.

4. **Manual Reset**: If all else fails, you can manually delete the database file and let the application recreate it.

## Going Forward

For production use, consider:

1. Using a more robust database like SQL Server instead of SQLite
2. Setting up proper migrations instead of using EnsureCreated
3. Adding database backup and recovery procedures
