using EntityFramework.Exceptions.PostgreSQL;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuestionApi.Database;

public class QuestionDbContext(DbContextOptions options) : IdentityDbContext<AppUser, AppRole, int>(options) {
    public DbSet<Question> Questions { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<ExamPaper> ExamPapers { get; set; }
    public DbSet<ExamPaperQuestion> ExamPaperQuestions { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<StudentAnswer> StudentAnswers { get; set; }
    public DbSet<AnswerHistory> AnswerHistories { get; set; }
    public DbSet<Examination> Examinations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.UseExceptionProcessor();
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);

        builder.Entity<Question>()
            .HasMany(v => v.Options)
            .WithOne(v => v.Question)
            .HasForeignKey(v => v.QuestionId)
            .IsRequired();

        builder.Entity<Option>()
            .Property(v => v.OptionCode)
            .HasDefaultValue('A');

        builder.Entity<ExamPaper>()
            .HasMany(e => e.Questions)
            .WithMany(e => e.Exams)
            .UsingEntity<ExamPaperQuestion>(
                l => l.HasOne(v => v.Question).WithMany(v => v.ExamPaperQuestions).HasForeignKey(e => e.QuestionId).IsRequired(),
                r => r.HasOne(v => v.ExamPaper).WithMany(v => v.ExamPaperQuestions).HasForeignKey(e => e.ExamPaperId).IsRequired());

        builder.Entity<Student>()
            .HasOne(v => v.User)
            .WithOne(v => v.Student)
            .HasForeignKey<Student>(v => v.UserId)
            .IsRequired(false);

        builder.Entity<StudentAnswer>()
            .HasOne(v => v.Student)
            .WithMany(v => v.StudentAnswers)
            .HasForeignKey(v => v.StudentId)
            .IsRequired();

        builder.Entity<StudentAnswer>()
            .HasOne(v => v.Question)
            .WithMany(v => v.StudentAnswers)
            .HasForeignKey(v => v.QuestionId)
            .IsRequired();

        builder.Entity<AnswerHistory>()
            .HasOne(v => v.Student)
            .WithMany(v => v.AnswerHistories)
            .HasForeignKey(v => v.StudentId)
            .IsRequired();

        builder.Entity<AnswerHistory>()
            .HasOne(v => v.ExamPaper)
            .WithMany(v => v.AnswerHistories)
            .HasForeignKey(v => v.ExamPaperId)
            .IsRequired();

        builder.Entity<AnswerHistory>()
            .HasMany(v => v.StudentAnswers)
            .WithOne(v => v.AnswerHistory)
            .HasForeignKey(v => v.AnswerHistoryId)
            .IsRequired();

        builder.Entity<AnswerHistory>()
            .HasOne(v => v.Examination)
            .WithMany(v => v.AnswerHistories)
            .HasForeignKey(v => v.ExaminationId)
            .IsRequired(false);

        builder.Entity<Examination>()
            .HasOne(v => v.ExamPaper)
            .WithMany(v => v.Examinations)
            .HasForeignKey(v => v.ExamPaperId)
            .IsRequired();
    }
}