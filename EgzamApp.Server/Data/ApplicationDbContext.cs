using EgzamApp.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EgzamApp.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ILogger<ApplicationDbContext> logger = null)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<UserExam> UserExams { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            _logger?.LogInformation("Configuring database model...");

            // Configure Exam entity
            modelBuilder.Entity<Exam>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ExamTitle).IsRequired();
                entity.Property(e => e.ExamDescription).IsRequired(false);
                entity.Property(e => e.PassingScore).IsRequired();
                entity.Property(e => e.TimeLimit).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.UploadDate).IsRequired();
                
                // Configure relationship with Questions
                entity.HasMany(e => e.Questions)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Question entity
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(q => q.Id);
                entity.Property(q => q.QuestionText).IsRequired();
                entity.Property(q => q.Type).IsRequired();
                entity.Property(q => q.Difficulty).IsRequired(false);
                entity.Property(q => q.Explanation).IsRequired(false);
                
                // Handle the Options collection - requires special configuration
                entity.Ignore(q => q.Options);
            });

            // Configure UserExam entity
            modelBuilder.Entity<UserExam>(entity =>
            {
                entity.HasKey(ue => ue.Id);
                entity.Property(ue => ue.UserId).IsRequired();
                entity.Property(ue => ue.StartTime).IsRequired();
                entity.Property(ue => ue.EndTime).IsRequired(false);
                entity.Property(ue => ue.Score).IsRequired();
                entity.Property(ue => ue.Completed).IsRequired();
                
                // Configure relationship with Exam
                entity.HasOne(ue => ue.Exam)
                      .WithMany()
                      .HasForeignKey(ue => ue.ExamId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Configure relationship with UserAnswers
                entity.HasMany(ue => ue.Answers)
                      .WithOne()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserAnswer entity
            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.HasKey(ua => ua.Id);
                entity.Property(ua => ua.QuestionId).IsRequired();
                entity.Property(ua => ua.SelectedAnswer).IsRequired();
                entity.Property(ua => ua.IsCorrect).IsRequired();
            });
            
            _logger?.LogInformation("Database model configuration complete");
        }
        
        // Override SaveChanges to add logging
        public override int SaveChanges()
        {
            try
            {
                _logger?.LogInformation("Saving changes to database...");
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving changes to database");
                throw;
            }
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Saving changes to database asynchronously...");
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving changes to database asynchronously");
                throw;
            }
        }
    }
}
