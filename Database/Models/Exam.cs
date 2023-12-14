using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Database;

/// <summary>
/// 试卷表
/// </summary>
public class Exam {
    public int ExamId { get; set; }
    [MaxLength(500)]
    public string ExamName { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
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