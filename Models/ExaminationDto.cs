using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class ExaminationFilter {
    [MaxLength(50)]
    public string? ExaminationName { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }
    [EnumDataType(typeof(ExaminationType), ErrorMessage = "无效的枚举值")]
    public ExaminationType? ExaminationType { get; set; }

    public IQueryable<Examination> Build(IQueryable<Examination> queryable) {
        if (!string.IsNullOrWhiteSpace(ExaminationName)) {
            queryable = queryable.Where(v => v.ExaminationName.Contains(ExaminationName));
        }
        if (DifficultyLevel is not null and > 0) {
            queryable = queryable.Where(v => v.DifficultyLevel == DifficultyLevel);
        }
        if (ExaminationType is not null and > 0) {
            queryable = queryable.Where(v => v.ExaminationType == ExaminationType);
        }
        return queryable;
    }
}

public class ExaminationDto {
    public int ExaminationId { get; set; }
    public string ExaminationName { get; set; } = string.Empty;
    public ExaminationType ExaminationType { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public int ExamPaperId { get; set; }
    public int DurationSeconds { get; set; }
}


public class ExaminationInput {
    [MaxLength(256)]
    public required string ExaminationName { get; set; } = string.Empty;
    [EnumDataType(typeof(ExaminationType), ErrorMessage = "无效的枚举值")]
    public ExaminationType ExaminationType { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel DifficultyLevel { get; set; }
    public int ExamPaperId { get; set; }
    public int DurationSeconds { get; set; }
}

public class ExaminationUpdate {
    [MaxLength(256)]
    public string? ExaminationName { get; set; }
    [EnumDataType(typeof(ExaminationType), ErrorMessage = "无效的枚举值")]
    public ExaminationType? ExaminationType { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }
    public int? ExamPaperId { get; set; }
    public int? DurationSeconds { get; set; }
}