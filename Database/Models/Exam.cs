using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionApi.Database;

/// <summary>
/// 试卷表
/// </summary>
public class ExamPaper {
    [Key]
    public int ExamId { get; set; }
    [MaxLength(500)]
    public string ExamName { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public List<Question> Questions { get; } = [];
    public List<ExamPaperQuestion> ExamQuestions { get; } = [];
    public List<AnswerHistory> AnswerHistories { get; } = [];
}

/// <summary>
/// 试卷题目关联表
/// </summary>
public class ExamPaperQuestion {
    public int ExamId { get; set; }
    public ExamPaper ExamPaper { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    /// <summary>
    /// 题目排序
    /// </summary>
    public int Order { get; set; }
}