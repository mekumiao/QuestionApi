using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class QuestionFilter {
    [MaxLength(50)]
    public string? QuestionText { get; set; }
    [EnumDataType(typeof(QuestionType), ErrorMessage = "无效的枚举值")]
    public QuestionType? QuestionType { get; set; }

    public IQueryable<Question> Build(IQueryable<Question> queryable) {
        if (!string.IsNullOrWhiteSpace(QuestionText)) {
            queryable = queryable.Where(v => v.QuestionText.Contains(QuestionText));
        }
        if (QuestionType is not null) {
            queryable = queryable.Where(v => v.QuestionType == QuestionType);
        }
        return queryable;
    }
}

public class QuestionDto {
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    [EnumDataType(typeof(QuestionType), ErrorMessage = "无效的枚举值")]
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public ICollection<OptionDto> Options { get; set; } = [];
}

public class QuestionInput {
    public required string QuestionText { get; set; } = string.Empty;
    [EnumDataType(typeof(QuestionType), ErrorMessage = "无效的枚举值")]
    public required QuestionType QuestionType { get; set; }
    public required string CorrectAnswer { get; set; } = string.Empty;
    public List<OptionInput>? Options { get; set; }
}

public class QuestionUpdate {
    public string? QuestionText { get; set; }
    public string? CorrectAnswer { get; set; }
    public List<OptionUpdate>? Options { get; set; }
}

public class OptionDto {
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public char OptionCode { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class OptionInput {
    public string OptionText { get; set; } = string.Empty;
    public char OptionCode { get; set; }
    public bool IsCorrect { get; set; }
}

public class OptionUpdate {

    public int OptionId { get; set; }
    public char OptionCode { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}