using QuestionApi.Database;

namespace QuestionApi.Models;

public class ExamDto {
    public int ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public List<QuestionDto> Questions { get; } = [];
    public List<ExamQuestionDto> ExamQuestions { get; } = [];
    public List<AnswerHistoryDto> AnswerHistories { get; } = [];
}

public class ExamInput {
    public int ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DifficultyLevel DifficultyLevel { get; set; }
    public List<QuestionDto> Questions { get; } = [];
    public List<ExamQuestionDto> ExamQuestions { get; } = [];
    public List<AnswerHistoryDto> AnswerHistories { get; } = [];
}

public class ExamQuestionDto {
    public int ExamId { get; set; }
    public ExamDto Exam { get; set; } = null!;
    public int QuestionId { get; set; }
    public QuestionDto Question { get; set; } = null!;
}