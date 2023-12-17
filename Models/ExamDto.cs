using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class ExamFilter {
    [MaxLength(50)]
    public string? ExamName { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }

    public IQueryable<Exam> Build(IQueryable<Exam> queryable) {
        if (!string.IsNullOrWhiteSpace(ExamName)) {
            queryable = queryable.Where(v => v.ExamName.Contains(ExamName));
        }
        if (DifficultyLevel is not null and > 0) {
            queryable = queryable.Where(v => v.DifficultyLevel == DifficultyLevel);
        }
        return queryable;
    }
}

public class ExamDto {
    public int ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public List<ExamQuestionDto> ExamQuestions { get; set; } = [];
}

public class ExamInput {
    public string ExamName { get; set; } = string.Empty;
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel DifficultyLevel { get; set; }
    public List<ExamQuestionInput> ExamQuestions { get; set; } = [];
}

public class ExamQuestionInput {
    public int QuestionId { get; set; }
    public int Order { get; set; }
}

public class ExamUpdate {
    public string ExamName { get; set; } = string.Empty;
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }
    public List<ExamQuestionInput>? ExamQuestions { get; set; }
}

public class ExamQuestionDto {
    public int ExamId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public ICollection<OptionDto> Options { get; set; } = [];
    public int Order { get; set; }
}