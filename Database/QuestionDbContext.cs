using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuestionApi.Database;

public class QuestionDbContext : IdentityDbContext {
    public string DbPath { get; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamQuestion> ExamQuestions { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<StudentAnswer> StudentAnswers { get; set; }
    public DbSet<AnswerHistory> AnswerHistories { get; set; }

    public QuestionDbContext() {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "question.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);

        builder.Entity<Question>()
            .HasMany(v => v.Options)
            .WithOne(v => v.Question)
            .HasForeignKey(v => v.QuestionId)
            .IsRequired();

        builder.Entity<Exam>()
            .HasMany(e => e.Questions)
            .WithMany(e => e.Exams)
            .UsingEntity<ExamQuestion>(
                l => l.HasOne(v => v.Question).WithMany(v => v.ExamQuestions).HasForeignKey(e => e.QuestionId).IsRequired(),
                r => r.HasOne(v => v.Exam).WithMany(v => v.ExamQuestions).HasForeignKey(e => e.ExamId).IsRequired());

        builder.Entity<Student>()
            .HasOne(v => v.User)
            .WithOne()
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
            .HasOne(v => v.Exam)
            .WithMany(v => v.AnswerHistories)
            .HasForeignKey(v => v.ExamId)
            .IsRequired();

        builder.Entity<AnswerHistory>()
            .HasMany(v => v.StudentAnswers)
            .WithOne(v => v.AnswerHistory)
            .HasForeignKey(v => v.AnswerHistoryId)
            .IsRequired();
    }
}
