using System.Diagnostics;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;

namespace QuestionApi.Services;

public class QuestionService(ILogger<QuestionService> logger, QuestionDbContext dbContext, IMapper mapper) {
    private readonly ILogger<QuestionService> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;
    private readonly static ExamPaperExcelGenerator ExamPaperExcelGenerator = new();
    private readonly static ExamPaperExcelParser ExamPaperExcelParser = new();

    public async Task<(Question[] questions, Dictionary<string, string[]> errors)> ImportFromExcelAsync(Stream stream) {
        var (examPapers, errors) = ExamPaperExcelParser.Parse(stream);
        if (errors.Count > 0) {
            return ([], errors);
        }
        var questions = examPapers.SelectMany(v => v.ExamPaperQuestions).Select(v => v.Question).ToArray();
        try {
            await _dbContext.Questions.AddRangeAsync(questions);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "导入题目：解析成功，保存到数据库失败");
            throw;
        }
        return (questions, errors);
    }

    public async Task<string?> ExportToExcelAsync(Stream stream, int[] questionIds) {
        var questions = await _dbContext.Questions
            .AsNoTracking()
            .Include(v => v.Options)
            .Where(v => questionIds.Contains(v.QuestionId))
            .OrderByDescending(v => v.QuestionId)
            .ToArrayAsync();
        if (questions.Length == 0) {
            return "没有找到任何的题目";
        }
        var examPaper = new ExamPaper();
        examPaper.ExamPaperQuestions.AddRange(questions.Select(v => new ExamPaperQuestion { Question = v }));
        ExamPaperExcelGenerator.Generate(stream, [examPaper]);
        return null;
    }
}
