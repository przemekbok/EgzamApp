using System.Text.Json;
using EgzamApp.Server.Data;
using EgzamApp.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EgzamApp.Server.Services
{
    public class ExamService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExamService> _logger;

        public ExamService(ApplicationDbContext context, ILogger<ExamService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ExamUploadResult> ProcessExamFileAsync(Stream fileStream, string userId)
        {
            try
            {
                // Parse the JSON file
                var exam = await JsonSerializer.DeserializeAsync<Exam>(fileStream);
                
                if (exam == null)
                {
                    return new ExamUploadResult
                    {
                        Success = false,
                        Message = "Invalid exam file format"
                    };
                }

                // Set the user ID
                exam.UserId = userId;
                exam.UploadDate = DateTime.UtcNow;

                // Add the exam to the database
                _context.Exams.Add(exam);
                await _context.SaveChangesAsync();

                return new ExamUploadResult
                {
                    Success = true,
                    Message = "Exam uploaded successfully",
                    Exam = exam
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing exam JSON file");
                return new ExamUploadResult
                {
                    Success = false,
                    Message = "Invalid JSON format: " + ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing exam file");
                return new ExamUploadResult
                {
                    Success = false,
                    Message = "Error processing file: " + ex.Message
                };
            }
        }

        public async Task<IEnumerable<Exam>> GetUserExamsAsync(string userId)
        {
            return await _context.Exams
                .Where(e => e.UserId == userId)
                .Include(e => e.Questions)
                .OrderByDescending(e => e.UploadDate)
                .ToListAsync();
        }

        public async Task<Exam?> GetExamAsync(int examId, string userId)
        {
            return await _context.Exams
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == examId && e.UserId == userId);
        }

        public async Task<UserExam> StartExamAsync(int examId, string userId)
        {
            var exam = await _context.Exams
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null)
            {
                throw new ArgumentException("Exam not found");
            }

            var userExam = new UserExam
            {
                ExamId = examId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                Completed = false
            };

            _context.UserExams.Add(userExam);
            await _context.SaveChangesAsync();

            return userExam;
        }

        public async Task<UserExam> SubmitExamAnswersAsync(int userExamId, List<UserAnswer> answers, string userId)
        {
            var userExam = await _context.UserExams
                .Include(ue => ue.Exam)
                .ThenInclude(e => e!.Questions)
                .FirstOrDefaultAsync(ue => ue.Id == userExamId && ue.UserId == userId);

            if (userExam == null)
            {
                throw new ArgumentException("User exam not found");
            }

            if (userExam.Completed)
            {
                throw new InvalidOperationException("Exam already completed");
            }

            // Calculate score
            int correctAnswers = 0;
            foreach (var answer in answers)
            {
                var question = userExam.Exam!.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question != null)
                {
                    answer.IsCorrect = answer.SelectedAnswer == question.CorrectAnswer;
                    if (answer.IsCorrect)
                    {
                        correctAnswers++;
                    }
                }
            }

            // Update user exam
            userExam.Answers = answers;
            userExam.EndTime = DateTime.UtcNow;
            userExam.Completed = true;
            userExam.Score = (int)Math.Round((double)correctAnswers / userExam.Exam!.Questions.Count * 100);

            await _context.SaveChangesAsync();

            return userExam;
        }
    }
}