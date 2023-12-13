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
    /// <summary>
    /// 选项编号。示例：A、B、C、D
    /// </summary>
    public char OptionCode { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
