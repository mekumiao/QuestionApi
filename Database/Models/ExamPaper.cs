using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionApi.Database;

/// <summary>
/// 试卷表
/// </summary>
public class ExamPaper {
    [Key]
    public int ExamPaperId { get; set; }
    [MaxLength(500)]
    public string ExamPaperName { get; set; } = string.Empty;
    public ExamPaperType ExamPaperType { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public List<Question> Questions { get; } = [];
    public List<ExamPaperQuestion> ExamPaperQuestions { get; } = [];
    public List<AnswerHistory> AnswerHistories { get; } = [];
    public List<Examination> Examinations { get; } = [];
}

/// <summary>
/// 试卷题目关联表
/// </summary>
public class ExamPaperQuestion {
    public int ExamPaperId { get; set; }
    public ExamPaper ExamPaper { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    /// <summary>
    /// 题目排序
    /// </summary>
    public int Order { get; set; }
}

public enum ExamPaperType {
    None = 0,
    Random,
    Import,
    Create,
    RedoIncorrect,
    RandomPractice,
}