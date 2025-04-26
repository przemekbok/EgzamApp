using EgzamApp.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EgzamApp.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<UserExam> UserExams { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Exam>()
                .HasMany(e => e.Questions);

            modelBuilder.Entity<UserExam>()
                .HasOne(ue => ue.Exam)
                .WithMany()
                .HasForeignKey(ue => ue.ExamId);

            modelBuilder.Entity<UserExam>()
                .HasMany(ue => ue.Answers);
        }
    }
}