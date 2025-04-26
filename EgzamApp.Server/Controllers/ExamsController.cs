using System.Security.Claims;
using EgzamApp.Server.Models;
using EgzamApp.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EgzamApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamsController : ControllerBase
    {
        private readonly ExamService _examService;
        private readonly ILogger<ExamsController> _logger;

        public ExamsController(ExamService examService, ILogger<ExamsController> logger)
        {
            _examService = examService;
            _logger = logger;
        }

        // Temporary method to get user ID - will be replaced with proper auth
        private string GetUserId()
        {
            // In a real application, this would come from authentication
            // For now, we'll use a placeholder
            return "demo-user";
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExam(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            if (!file.ContentType.Equals("application/json"))
            {
                return BadRequest("Only JSON files are supported");
            }

            string userId = GetUserId();

            using var stream = file.OpenReadStream();
            var result = await _examService.ProcessExamFileAsync(stream, userId);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetExams()
        {
            string userId = GetUserId();
            var exams = await _examService.GetUserExamsAsync(userId);
            return Ok(exams);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExam(int id)
        {
            string userId = GetUserId();
            var exam = await _examService.GetExamAsync(id, userId);

            if (exam == null)
            {
                return NotFound();
            }

            return Ok(exam);
        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartExam(int id)
        {
            try
            {
                string userId = GetUserId();
                var userExam = await _examService.StartExamAsync(id, userId);
                return Ok(userExam);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting exam");
                return StatusCode(500, "Error starting exam");
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitExam([FromBody] ExamSubmission submission)
        {
            try
            {
                string userId = GetUserId();
                var userExam = await _examService.SubmitExamAnswersAsync(
                    submission.UserExamId, 
                    submission.Answers, 
                    userId);
                
                return Ok(userExam);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting exam");
                return StatusCode(500, "Error submitting exam");
            }
        }
    }

    public class ExamSubmission
    {
        public int UserExamId { get; set; }
        public List<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
    }
}