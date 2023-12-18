using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Database;

public class Examination {
    public int ExaminationId { get; set; }
    [MaxLength(256)]
    public string ExaminationName { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public ExaminationType ExaminationType { get; set; }
    public int Order { get; set; }
    public int ExamPaperId { get; set; }
    public ExamPaper ExamPaper { get; set; } = null!;
    /// <summary>
    /// 持续时间
    /// </summary>
    public int DurationSeconds { get; set; }
}

public enum ExaminationType {
    None = 0,
    /// <summary>
    /// 正常考试
    /// </summary>
    Exam,
    /// <summary>
    /// 模拟考试
    /// </summary>
    Mock,
    /// <summary>
    /// 练习
    /// </summary>
    Practice
}