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
    public string ExamPaperName { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int ExamParticipantCount { get; set; }
    public int Order { get; set; }
    public bool IsPublish { get; set; }
}

public class ExaminationPublishDto {
    public int ExaminationId { get; set; }
    public string ExaminationName { get; set; } = string.Empty;
    public ExaminationType ExaminationType { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public int ExamPaperId { get; set; }
    public string ExamPaperName { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    public int ExamParticipantCount { get; set; }
    public int Order { get; set; }
    public bool IsSubmission { get; set; }
    /// <summary>
    /// 剩余时间
    /// </summary>
    public int RemainingSeconds { get; set; }
    /// <summary>
    /// 消耗时间
    /// </summary>
    public int TimeTakenSeconds { get; set; }
    public AnswerState AnswerState { get; set; }
}

public enum AnswerState {
    None,
    Unanswered,
    Answering,
    Finished,
    Timeout,
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
    public int Order { get; set; }
    public bool IsPublish { get; set; }
}

public class ExaminationUpdate {
    [MaxLength(256)]
    public string? ExaminationName { get; set; }
    [EnumDataType(typeof(ExaminationType), ErrorMessage = "无效的枚举值")]
    public ExaminationType? ExaminationType { get; set; }
    [EnumDataType(typeof(DifficultyLevel), ErrorMessage = "无效的枚举值")]
    public DifficultyLevel? DifficultyLevel { get; set; }
    public int? DurationSeconds { get; set; }
    public int? Order { get; set; }
    public bool? IsPublish { get; set; }
}

public class CertificateDto {
    public int UserId { get; set; }
    public string? Avatar { get; set; }
    public int? AvatarFileId { get; set; }
    public required string UserName { get; set; }
    public required string NickName { get; set; }
    public int StudentId { get; set; }
    public int ExaminationId { get; set; }
    public required string ExaminationName { get; set; }
    public int TimeTakenSeconds { get; set; }
    public int Score { get; set; }
    public bool IsSuccess { get; set; }
}