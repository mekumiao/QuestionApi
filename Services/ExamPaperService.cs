using System.Diagnostics;

using EntityFramework.Exceptions.Common;

using MapsterMapper;
using Microsoft.EntityFrameworkCore;

using QuestionApi.Database;

namespace QuestionApi.Services;

public class ExamPaperService(ILogger<ExamPaperService> logger, QuestionDbContext dbContext, IMapper mapper) {
    private readonly ILogger<ExamPaperService> _logger = logger;
    private readonly QuestionDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// 随机生成试卷
    /// </summary>
    /// <returns></returns>
    public async Task<(ExamPaper? examPaper, string? message)> RandomGenerationAsync(string userName, DifficultyLevel? difficultyLevel) {
        var queryable = _dbContext.Questions.AsNoTracking();
        queryable = _dbContext.Database.ProviderName?.Contains("sqlserver", StringComparison.CurrentCultureIgnoreCase) is true
            ? queryable.OrderBy(x => Guid.NewGuid())
            : queryable.OrderBy(x => EF.Functions.Random());

        if (difficultyLevel > DifficultyLevel.None) {
            queryable = queryable.Where(v => v.DifficultyLevel <= difficultyLevel);
        }

        var singleQuestions = await queryable.Where(v => v.QuestionType == QuestionType.SingleChoice).Select(v => v.QuestionId).Take(10).ToListAsync();
        var multipleQuestions = await queryable.Where(v => v.QuestionType == QuestionType.MultipleChoice).Select(v => v.QuestionId).Take(5).ToArrayAsync();
        var truefalseQuestions = await queryable.Where(v => v.QuestionType == QuestionType.TrueFalse).Select(v => v.QuestionId).Take(5).ToArrayAsync();
        var fillblankQuestions = await queryable.Where(v => v.QuestionType == QuestionType.FillInTheBlank).Select(v => v.QuestionId).Take(5).ToArrayAsync();

        singleQuestions.AddRange(multipleQuestions);
        singleQuestions.AddRange(truefalseQuestions);
        singleQuestions.AddRange(fillblankQuestions);

        var examPaper = new ExamPaper {
            ExamPaperType = ExamPaperType.Random,
            ExamPaperName = $"随机生成-{userName}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            DifficultyLevel = difficultyLevel ?? DifficultyLevel.None,
        };

        var questions = singleQuestions.Select(v => new ExamPaperQuestion { QuestionId = v });
        examPaper.ExamPaperQuestions.AddRange(questions);

        try {
            await _dbContext.ExamPapers.AddAsync(examPaper);
            await _dbContext.SaveChangesAsync();
        }
        catch (ReferenceConstraintException ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "保存随机生成的试卷时失败");
            return (null, "随机生成失败，请尝试重新生成");
        }
        catch (Exception ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "保存随机生成的试卷时失败");
            throw;
        }

        return (examPaper, null);
    }
}