using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

/// <summary>
/// 答题板
/// </summary>
public class AnswerBoard {
    public int AnswerBoardId { get; set; }
    public string ExamPaperName { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public int TotalIncorrectAnswers { get; set; }
    public int DurationSeconds { get; set; }
    public int TimeTakenSeconds { get; set; }
    public bool IsTimeout { get; set; }
    public bool IsSubmission { get; set; }
    public ICollection<AnswerBoardQuestion> Questions { get; set; } = [];
}

public class AnswerBoardQuestion {
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    /// <summary>
    /// 是否答对
    /// </summary>
    public bool? IsCorrect { get; set; }
    public ICollection<AnswerBoardQuestionOption> Options { get; set; } = [];
}

public class AnswerBoardQuestionOption {
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public char OptionCode { get; set; }
    public string OptionText { get; set; } = string.Empty;
}

public class AnswerBoardInput {
    public int ExamPaperId { get; set; }
    public int? ExaminationId { get; set; }
}

public class AnswerInput {
    public int QuestionId { get; set; }
    [MaxLength(500)]
    public string AnswerText { get; set; } = string.Empty;
}
