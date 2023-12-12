using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace QuestionApi.Database;

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