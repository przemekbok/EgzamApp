using Microsoft.AspNetCore.Mvc;

namespace EgzamApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Test endpoint was called successfully");
            return Ok(new { 
                Message = "API is working correctly!", 
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }

        [HttpGet("error")]
        public IActionResult GetError()
        {
            try
            {
                throw new Exception("This is a test exception");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test exception was thrown");
                return StatusCode(500, new {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("database")]
        public IActionResult GetDatabaseInformation([FromServices] EgzamApp.Server.Data.ApplicationDbContext context)
        {
            try
            {
                var canConnect = context.Database.CanConnect();
                var provider = context.Database.ProviderName;
                
                return Ok(new {
                    CanConnect = canConnect,
                    Provider = provider,
                    ExamsCount = context.Exams.Count(),
                    QuestionsCount = context.Questions.Count()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database information");
                return StatusCode(500, new {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}
