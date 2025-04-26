using EgzamApp.Server.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EgzamApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(ApplicationDbContext context, ILogger<DiagnosticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("db-status")]
        public IActionResult GetDatabaseStatus()
        {
            try
            {
                var result = new
                {
                    CanConnect = _context.Database.CanConnect(),
                    Provider = _context.Database.ProviderName,
                    ConnectionString = "Hidden for security",
                    TablesInfo = GetTableInfo()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database status");
                return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        [HttpPost("initialize")]
        public async Task<IActionResult> InitializeDatabase()
        {
            try
            {
                // Force recreation of the database
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();
                
                return Ok(new { Success = true, Message = "Database initialized successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
                return StatusCode(500, new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        private object GetTableInfo()
        {
            var tables = new List<string>();
            var conn = _context.Database.GetDbConnection();
            
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                using var command = conn.CreateCommand();
                command.CommandText = "SELECT name FROM sqlite_master WHERE type='table';";
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    tables.Add(reader.GetString(0));
                }
                
                return new
                {
                    TableCount = tables.Count,
                    Tables = tables
                };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
        }
    }
}
