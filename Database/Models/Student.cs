using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Database;

/// <summary>
/// 学生表
/// </summary>
public class Student {
    public int StudentId { get; set; }
    [MaxLength(256)]
    public string StudentName { get; set; } = string.Empty;
    /// <summary>
    /// 总题目数
    /// </summary>
    public int TotalQuestions { get; set; }
    /// <summary>
    /// 作答总数
    /// </summary>
    public int TotalNumberAnswers { get; set; }
    /// <summary>
    /// 错题总数
    /// </summary>
    public int TotalIncorrectAnswers { get; set; }
    /// <summary>
    /// 参加考试次数
    /// </summary>
    public int TotalExamParticipations { get; set; }
    /// <summary>
    /// 练习次数
    /// </summary>
    public int TotalPracticeSessions { get; set; }
    public int? UserId { get; set; }
    public AppUser? User { get; set; }
    public List<StudentAnswer> StudentAnswers { get; } = [];
    public List<AnswerHistory> AnswerHistories { get; } = [];
}