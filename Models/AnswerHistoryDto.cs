using System.ComponentModel.DataAnnotations;

using QuestionApi.Database;

namespace QuestionApi.Models;

public class AnswerHistoryFilter {
    public int? StudentId { get; set; }
    public int? ExamPaperId { get; set; }

    public IQueryable<AnswerHistory> Build(IQueryable<AnswerHistory> queryable) {
        if (StudentId.HasValue) {
            queryable = queryable.Where(v => v.StudentId == StudentId);
        }
        if (ExamPaperId.HasValue) {
            queryable = queryable.Where(v => v.ExamPaperId == ExamPaperId);
        }
        return queryable;
    }
}

public class AnswerInput {
    public int QuestionId { get; set; }
    [MaxLength(500)]
    public string AnswerText { get; set; } = string.Empty;
}

public class AnswerHistoryDto {
    public int AnswerHistoryId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int ExamPaperId { get; set; }
    public string ExamPaperName { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }
    /// <summary>
    /// 交卷时间
    /// </summary>
    public DateTime SubmissionTime { get; set; }
    public bool IsSubmission { get; set; }
    public int TotalIncorrectAnswers { get; set; }
    public List<StudentAnswerDto> StudentAnswers { get; } = [];
}

public class StudentAnswerDto {
    public int StudentAnswerId { get; set; }
    public int StudentId { get; set; }
    public int QuestionId { get; set; }
    public QuestionType QuestionType { get; set; }
    public int AnswerHistoryId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
