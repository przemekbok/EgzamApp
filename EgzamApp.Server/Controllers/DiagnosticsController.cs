using System.Text;
using System.Text.Json;
using EgzamApp.Server.Data;
using EgzamApp.Server.Models;
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

        [HttpPost("test-json")]
        public IActionResult TestJsonDeserialization([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                // Try to deserialize with different options
                var results = new List<object>();

                // Test case-insensitive deserialization
                try
                {
                    var options1 = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var exam1 = JsonSerializer.Deserialize<Exam>(json, options1);
                    results.Add(new { Method = "Case-insensitive", Success = true, Title = exam1?.ExamTitle, QuestionCount = exam1?.Questions?.Count });
                }
                catch (Exception ex)
                {
                    results.Add(new { Method = "Case-insensitive", Success = false, Error = ex.Message });
                }

                // Test with JsonPropertyName attributes
                try
                {
                    var options2 = new JsonSerializerOptions();
                    var exam2 = JsonSerializer.Deserialize<Exam>(json, options2);
                    results.Add(new { Method = "With JsonPropertyName", Success = true, Title = exam2?.ExamTitle, QuestionCount = exam2?.Questions?.Count });
                }
                catch (Exception ex)
                {
                    results.Add(new { Method = "With JsonPropertyName", Success = false, Error = ex.Message });
                }

                // Test with manual dynamic parsing
                try
                {
                    var dynamicJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                    var hasTitleProperty = dynamicJson?.ContainsKey("examTitle") ?? false;
                    var hasQuestionsProperty = dynamicJson?.ContainsKey("questions") ?? false;
                    
                    results.Add(new
                    {
                        Method = "Dynamic parsing",
                        Success = true,
                        Properties = dynamicJson?.Keys.ToList(),
                        HasExamTitle = hasTitleProperty,
                        HasQuestions = hasQuestionsProperty
                    });
                }
                catch (Exception ex)
                {
                    results.Add(new { Method = "Dynamic parsing", Success = false, Error = ex.Message });
                }

                return Ok(new
                {
                    FileInfo = new
                    {
                        Name = file.FileName,
                        Size = file.Length,
                        ContentType = file.ContentType
                    },
                    JsonPreview = json.Length > 500 ? json.Substring(0, 500) + "..." : json,
                    DeserializationResults = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing JSON deserialization");
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
