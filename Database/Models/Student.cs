using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Database;

/// <summary>
/// 学生表
/// </summary>
public class Student {
    public int StudentId { get; set; }
    [MaxLength(256)]
    public string StudentName { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public AppUser? User { get; set; }
    public List<StudentAnswer> StudentAnswers { get; } = [];
    public List<AnswerHistory> AnswerHistories { get; } = [];
}