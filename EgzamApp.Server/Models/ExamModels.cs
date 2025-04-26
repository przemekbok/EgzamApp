using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EgzamApp.Server.Models
{
    public class Exam
    {
        public int Id { get; set; }
        
        [JsonPropertyName("examTitle")]
        public string ExamTitle { get; set; } = string.Empty;
        
        [JsonPropertyName("examDescription")]
        public string ExamDescription { get; set; } = string.Empty;
        
        [JsonPropertyName("passingScore")]
        public int PassingScore { get; set; }
        
        [JsonPropertyName("timeLimit")]
        public string TimeLimit { get; set; } = string.Empty;
        
        [JsonPropertyName("questions")]
        public List<Question> Questions { get; set; } = new List<Question>();
        
        public string UserId { get; set; } = string.Empty;
        
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
    }

    public class Question
    {
        // This is the database Id (not serialized from JSON)
        [JsonIgnore]
        public int Id { get; set; }
        
        [JsonPropertyName("question")]
        public string QuestionText { get; set; } = string.Empty;
        
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("options")]
        public List<string> Options { get; set; } = new List<string>();
        
        [JsonPropertyName("correctAnswer")]
        public int CorrectAnswer { get; set; }
        
        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; } = string.Empty;
        
        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;
        
        // Map to the existing ExternalId column in the database
        [JsonPropertyName("id")]
        [Column("ExternalId")]
        public int QuestionId { get; set; }
        
        [JsonIgnore]
        public int? ExamId { get; set; }
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