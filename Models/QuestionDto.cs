using System.ComponentModel.DataAnnotations;

namespace QuestionApi.Modules;

public class QuestionDto {
    public int QuestionId { get; set; }
    public string Remark { get; set; } = string.Empty;
    [EnumDataType(typeof(QuestionType), ErrorMessage = "无效的枚举值")]
    public QuestionType Type { get; set; }
}

public class QuestionCreateDto {
    public string Remark { get; set; } = string.Empty;
    [EnumDataType(typeof(QuestionType), ErrorMessage = "无效的枚举值")]
    public QuestionType Type { get; set; }
}

public class QuestionUpdateDto {
    public string Remark { get; set; } = string.Empty;
    [EnumDataType(typeof(QuestionType), ErrorMessage = "无效的枚举值")]
    public QuestionType Type { get; set; }
}