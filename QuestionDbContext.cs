using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace QuestionApi;

public class QuestionDbContext : IdentityDbContext {
    public DbSet<Question> Questions { get; set; }
    public string DbPath { get; }

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

/// <summary>
/// 题目表
/// </summary>
public class Question {
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public List<Option> Options { get; } = [];
    public List<Exam> Exams { get; } = [];
    public List<ExamQuestion> ExamQuestions { get; } = [];
    public List<StudentAnswer> StudentAnswers { get; } = [];
}

/// <summary>
/// 题型枚举
/// </summary>
public enum QuestionType {
    SingleChoice,
    MultipleChoice,
    TrueFalse,
    FillInTheBlank
}

/// <summary>
/// 选项表
/// </summary>
public class Option {
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

/// <summary>
/// 试卷表
/// </summary>
public class Exam {
    public int ExamId { get; set; }
    [MaxLength(500)]
    public string ExamName { get; set; } = string.Empty;
    public List<Question> Questions { get; } = [];
    public List<ExamQuestion> ExamQuestions { get; } = [];
    public List<AnswerHistory> AnswerHistories { get; } = [];
}

/// <summary>
/// 试卷题目关联表
/// </summary>
public class ExamQuestion {
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
}

/// <summary>
/// 学生表
/// </summary>
public class Student {
    public int StudentId { get; set; }
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public IdentityUser? User { get; set; }
    public List<StudentAnswer> StudentAnswers { get; } = [];
    public List<AnswerHistory> AnswerHistories { get; } = [];
}

/// <summary>
/// 学生答题表
/// </summary>
public class StudentAnswer {
    [Key]
    public int AnswerId { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public int AnswerHistoryId { get; set; }
    public AnswerHistory AnswerHistory { get; set; } = null!;
    /// <summary>
    /// 答案选项（单选题和多选题。用英文逗号","隔开）
    /// </summary>
    public string ChosenOptions { get; set; } = string.Empty;
    /// <summary>
    /// 答案文本（填空题）
    /// </summary>
    public string AnswerText { get; set; } = string.Empty;
}

/// <summary>
/// 答题历史表
/// </summary>
public class AnswerHistory {
    public int AnswerHistoryId { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    /// <summary>
    /// 交卷时间
    /// </summary>
    public DateTime SubmissionTime { get; set; }
    public List<StudentAnswer> StudentAnswers { get; } = [];
}