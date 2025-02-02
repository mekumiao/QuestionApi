using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class ExamPaperFilter {
    [MaxLength(50)]
    public string? ExamPaperName { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }
    [EnumDataType(typeof(ExamPaperType), ErrorMessage = "无效的枚举值")]
    public ExamPaperType? ExamPaperType { get; set; }

    public IQueryable<ExamPaper> Build(IQueryable<ExamPaper> queryable) {
        if (!string.IsNullOrWhiteSpace(ExamPaperName)) {
            queryable = queryable.Where(v => v.ExamPaperName.Contains(ExamPaperName));
        }
        if (DifficultyLevel is not null and > 0) {
            queryable = queryable.Where(v => v.DifficultyLevel == DifficultyLevel);
        }
        if (ExamPaperType is not null and > 0) {
            queryable = queryable.Where(v => v.ExamPaperType == ExamPaperType);
        }
        return queryable;
    }
}

public class ExamPaperDto {
    public int ExamPaperId { get; set; }
    public string ExamPaperName { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public ExamPaperType ExamPaperType { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public List<ExamPaperQuestionDto> Questions { get; set; } = [];
}

public class ExamPaperInput {
    [MaxLength(256)]
    public required string ExamPaperName { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel DifficultyLevel { get; set; }
    /// <summary>
    /// 题目列表(允许传入空数组)
    /// </summary>
    [MaxLength(100)]
    public List<ExamPaperQuestionInput>? Questions { get; set; }
}

public class ExamPaperQuestionInput {
    public int QuestionId { get; set; }
    public int Order { get; set; }
}

public class ExamPaperQuestionUpdate {
    public int QuestionId { get; set; }
    [MaxLength(256)]
    public string? CorrectAnswer { get; set; }
    public int Order { get; set; }
}

public class ExamPaperUpdate {
    [MaxLength(256)]
    public string? ExamPaperName { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }
    [MaxLength(100)]
    public List<ExamPaperQuestionUpdate>? Questions { get; set; }
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
    [MaxLength(256)]
    public string? ExamPaperName { get; set; }
    /// <summary>
    /// 允许出现低于或等于此难度级别的题目
    /// </summary>
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }
}

public class ImportExamPaperFromExcelInput {
    [MaxLength(256)]
    public string? ExamPaperName { get; set; }
    public required IFormFile File { get; set; }
}