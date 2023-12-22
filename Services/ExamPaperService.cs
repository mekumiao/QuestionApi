using System.Diagnostics;
using System.Text.RegularExpressions;

using EntityFramework.Exceptions.Common;

using MapsterMapper;

using Microsoft.EntityFrameworkCore;

using OfficeOpenXml;

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

    public async Task<(List<ExamPaper> examPapers, Dictionary<string, string[]> errors)> ImportFromExcelAsync(string examPaperName, Stream stream) {
        var parser = new ExamPaperExcelParser();
        var result = parser.Parse(stream);
        if (result.errors.Count > 0) {
            return result;
        }
        try {
            foreach (var item in result.examPapers) {
                item.ExamPaperName = examPaperName;
            }
            await _dbContext.ExamPapers.AddRangeAsync(result.examPapers);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "导入试卷：解析成功，保存到数据库失败");
            throw;
        }
        return result;
    }
}

public partial class ExamPaperExcelParser {

    [GeneratedRegex(@"^[a-zA-Z]+$")]
    private static partial Regex IsLetter();

    public (List<ExamPaper> examPapers, Dictionary<string, string[]> errors) Parse(Stream stream) {
        var errors = new Dictionary<string, string[]>();
        var examPapers = new List<ExamPaper>();
        using var package = new ExcelPackage(stream);
        for (int i = 0; i < package.Workbook.Worksheets.Count; i++) {
            var worksheet = package.Workbook.Worksheets[0];
            var (examPaper, subErrors) = ParseWorksheet(worksheet);
            if (subErrors.Count > 0) {
                errors.Add(worksheet.Name, [.. subErrors]);
                continue;
            }
            examPapers.Add(examPaper);
        }
        return (examPapers, errors);
    }

    private static (ExamPaper examPaper, List<string> errors) ParseWorksheet(ExcelWorksheet worksheet) {
        string? message;
        var errors = new List<string>(1);

        var examPaper = new ExamPaper { ExamPaperName = worksheet.Name, ExamPaperType = ExamPaperType.Import };
        for (int row = 2; row <= worksheet.Dimension.Rows; row++) {
            var question = new ExamPaperQuestion { Question = new Question() };
            for (int col = 1; col <= worksheet.Dimension.Columns; col++) {
                var cellValue = worksheet.Cells[row, col].Text.Trim();
                if (cellValue.Length > 256) {
                    errors.Add($"第{row}行{col}列的值长度不能大于256");
                    continue;
                }
                switch (col) {
                    case 1:
                        message = SetOrder(question, cellValue, row, col);
                        if (message is not null) {
                            errors.Add(message);
                        }
                        break;
                    case 2:
                        message = SetQuestionText(question, cellValue, row, col);
                        if (message is not null) {
                            errors.Add(message);
                        }
                        break;
                    case 3:
                        message = SetQuestionType(question, cellValue, row, col);
                        if (message is not null) {
                            errors.Add(message);
                        }
                        break;
                    case 4:
                        message = SetDifficultyLevel(question, cellValue, row, col);
                        if (message is not null) {
                            errors.Add(message);
                        }
                        break;
                    case 5:
                        message = SetCorrectAnswer(question, cellValue, row, col);
                        if (message is not null) {
                            errors.Add(message);
                        }
                        break;
                    default:
                        // 后面的都归为选项
                        message = AddQuestionOption(question, cellValue, row, col);
                        if (message is not null) {
                            errors.Add(message);
                        }
                        break;
                }
            }
            examPaper.ExamPaperQuestions.Add(question);
        }
        return (examPaper, errors);
    }

    private static string? SetOrder(ExamPaperQuestion question, string cellValue, int row, int col) {
        if (string.IsNullOrWhiteSpace(cellValue)) {
            return $"第{row}行{col}列的值不能为空白";
        }
        if (!int.TryParse(cellValue, out var order)) {
            return $"第{row}行{col}列的值必须时数字";
        }
        question.Order = order;
        return null;
    }

    private static string? SetQuestionText(ExamPaperQuestion question, string cellValue, int row, int col) {
        if (string.IsNullOrWhiteSpace(cellValue)) {
            return $"第{row}行{col}列的值不能为空白";
        }
        question.Question.QuestionText = cellValue;
        return null;
    }

    private static string? SetQuestionType(ExamPaperQuestion question, string cellValue, int row, int col) {
        if (string.IsNullOrWhiteSpace(cellValue)) {
            return $"第{row}行{col}列的值{cellValue}不能为空白";
        }
        var questionType = cellValue switch {
            "单选题" => QuestionType.SingleChoice,
            "多选题" => QuestionType.MultipleChoice,
            "判断题" => QuestionType.TrueFalse,
            "填空题" => QuestionType.FillInTheBlank,
            _ => QuestionType.None,
        };
        if (questionType == QuestionType.None) {
            return $"第{row}行{col}列的值{cellValue}格式错误。正确值分别为：单选题、多选题、判断题、填空题";
        }
        question.Question.QuestionType = questionType;
        return null;
    }

    private static string? SetDifficultyLevel(ExamPaperQuestion question, string cellValue, int row, int col) {
        if (string.IsNullOrWhiteSpace(cellValue)) {
            return $"第{row}行{col}列的值{cellValue}不能为空白";
        }
        var difficultyLevel = cellValue switch {
            "1" => DifficultyLevel.Easy,
            "2" => DifficultyLevel.Medium,
            "3" => DifficultyLevel.Hard,
            _ => DifficultyLevel.None,
        };
        if (difficultyLevel == DifficultyLevel.None) {
            return $"第{row}行{col}列的值{cellValue}格式错误。正确值分别为：1、2、3";
        }
        question.Question.DifficultyLevel = difficultyLevel;
        return null;
    }

    private static string? SetCorrectAnswer(ExamPaperQuestion question, string cellValue, int row, int col) {
        if (string.IsNullOrWhiteSpace(cellValue)) {
            return $"第{row}行{col}列的值{cellValue}不能为空白";
        }
        switch (question.Question.QuestionType) {
            case QuestionType.SingleChoice:
                if (!char.IsLetter(cellValue, 0)) {
                    return $"第{row}行{col}列的值{cellValue}必须是字母";
                }
                question.Question.CorrectAnswer = cellValue.ToUpper();
                break;
            case QuestionType.MultipleChoice:
                if (!IsLetter().IsMatch(cellValue)) {
                    return $"第{row}行{col}列的值{cellValue}必须是字母";
                }
                question.Question.CorrectAnswer = cellValue.ToUpper();
                break;
            case QuestionType.TrueFalse:
                if (cellValue != "0" && cellValue != "1") {
                    return $"第{row}行{col}列的值{cellValue}必须是0或1";
                }
                question.Question.CorrectAnswer = cellValue;
                break;
            case QuestionType.FillInTheBlank:
                question.Question.CorrectAnswer = cellValue;
                break;
            default:
                break;
        }
        return null;
    }

    private static string? AddQuestionOption(ExamPaperQuestion question, string cellValue, int row, int col) {
        if (question.Question.QuestionType == QuestionType.SingleChoice || question.Question.QuestionType == QuestionType.MultipleChoice) {
            if (string.IsNullOrWhiteSpace(cellValue)) {
                return $"第{row}行{col}列的值{cellValue}不能为空白";
            }
            var c = col - 4;
            if (c > 26) {
                return null;
            }
            var option = new Option {
                OptionText = cellValue,
                OptionCode = Convert.ToChar(c + 64),//减去前面的4列，A字母的ASCII是65，列索引从1开始
            };
            question.Question.Options.Add(option);
        }
        return null;
    }
}