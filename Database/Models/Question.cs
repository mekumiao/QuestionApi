using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Database;

/// <summary>
/// 题目表
/// </summary>
public class Question {
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public List<Option> Options { get; } = [];
    public List<ExamPaper> Exams { get; } = [];
    public List<ExamPaperQuestion> ExamPaperQuestions { get; } = [];
    public List<StudentAnswer> StudentAnswers { get; } = [];
}

/// <summary>
/// 题型枚举
/// </summary>
public enum QuestionType {
    None,
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
    /// <summary>
    /// 选项编号。示例：A、B、C、D
    /// </summary>
    public char OptionCode { get; set; }
    public string OptionText { get; set; } = string.Empty;
    [Obsolete("弃用这个字段，统一由 Question.CorrectAnswer 决定")]
    public bool IsCorrect { get; set; }
}

/// <summary>
/// 难度等级
/// </summary>
public enum DifficultyLevel {
    None,
    Easy,
    Medium,
    Hard
}