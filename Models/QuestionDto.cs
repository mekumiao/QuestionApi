namespace QuestionApi.Modules;

public class QuestionDto {
    public int QuestionId { get; set; }
    public string Remark { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
}