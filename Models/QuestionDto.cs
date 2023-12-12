using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class QuestionDto {
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    [EnumDataType(typeof(QuestionType), ErrorMessage = "无效的枚举值")]
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public ICollection<Option> Options { get; set; } = [];
}

public class QuestionCreateDto {
    public string QuestionText { get; set; } = string.Empty;
    [EnumDataType(typeof(QuestionType), ErrorMessage = "无效的枚举值")]
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
}

public class QuestionUpdateDto {
    public string QuestionText { get; set; } = string.Empty;
    [EnumDataType(typeof(QuestionType), ErrorMessage = "无效的枚举值")]
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
}

public class OptionDto {
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}