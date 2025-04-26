using System.ComponentModel.DataAnnotations;

namespace EgzamApp.Server.Models
{
    public class Exam
    {
        public int Id { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string ExamDescription { get; set; } = string.Empty;
        public int PassingScore { get; set; }
        public string TimeLimit { get; set; } = string.Empty;
        public List<Question> Questions { get; set; } = new List<Question>();
        public string UserId { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    }

    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectAnswer { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }

    public class UserExam
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ExamId { get; set; }
        public Exam? Exam { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Score { get; set; }
        public bool Completed { get; set; }
        public List<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
    }

    public class UserAnswer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class ExamUploadResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Exam? Exam { get; set; }
    }
}