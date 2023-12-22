using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class ExamPaperFilter {
    [MaxLength(50)]
    public string? ExamPaperName { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }

    public IQueryable<ExamPaper> Build(IQueryable<ExamPaper> queryable) {
        if (!string.IsNullOrWhiteSpace(ExamPaperName)) {
            queryable = queryable.Where(v => v.ExamPaperName.Contains(ExamPaperName));
        }
        if (DifficultyLevel is not null and > 0) {
            queryable = queryable.Where(v => v.DifficultyLevel == DifficultyLevel);
        }
        return queryable;
    }
}

public class ExamPaperDto {
    public int ExamPaperId { get; set; }
    public string ExamPaperName { get; set; } = string.Empty;
    public ExamPaperType ExamPaperType { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public List<ExamPaperQuestionDto> Questions { get; set; } = [];
}

public class ExamPaperInput {
    [MaxLength(500)]
    public string ExamPaperName { get; set; } = string.Empty;
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel DifficultyLevel { get; set; }
    [MaxLength(100), MinLength(1)]
    public List<ExamPaperQuestionInput> Questions { get; set; } = [];
}

public class ExamPaperQuestionInput {
    public int QuestionId { get; set; }
    public int Order { get; set; }
}

public class ExamPaperUpdate {
    [MaxLength(500)]
    public string? ExamPaperName { get; set; } = string.Empty;
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }
    [MaxLength(100), MinLength(1)]
    public List<ExamPaperQuestionInput> Questions { get; set; } = [];
}

public class ExamPaperQuestionDto {
    public int ExamPaperId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public ICollection<OptionDto> Options { get; set; } = [];
    public int Order { get; set; }
}

public class RandomGenerationInput {
    public DifficultyLevel? DifficultyLevel { get; set; }
}