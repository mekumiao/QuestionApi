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
    private readonly static ExamPaperExcelGenerator ExamPaperExcelGenerator = new();
    private readonly static ExamPaperExcelParser ExamPaperExcelParser = new();

    /// <summary>
    /// 随机生成试卷
    /// </summary>
    /// <returns></returns>
    public async Task<(ExamPaper examPaper, string? message)> RandomGenerationAsync(ExamPaper initExamPaper) {
        var queryable = _dbContext.Questions.AsNoTracking();
        queryable = _dbContext.Database.ProviderName?.Contains("sqlserver", StringComparison.CurrentCultureIgnoreCase) is true
            ? queryable.OrderBy(x => Guid.NewGuid())
            : queryable.OrderBy(x => EF.Functions.Random());

        if (initExamPaper.DifficultyLevel > DifficultyLevel.None) {
            queryable = queryable.Where(v => v.DifficultyLevel <= initExamPaper.DifficultyLevel);
        }

        var singleQuestions = await queryable.Where(v => v.QuestionType == QuestionType.SingleChoice).Select(v => v.QuestionId).Take(5).ToListAsync();
        var multipleQuestions = await queryable.Where(v => v.QuestionType == QuestionType.MultipleChoice).Select(v => v.QuestionId).Take(5).ToArrayAsync();
        var truefalseQuestions = await queryable.Where(v => v.QuestionType == QuestionType.TrueFalse).Select(v => v.QuestionId).Take(5).ToArrayAsync();
        var fillblankQuestions = await queryable.Where(v => v.QuestionType == QuestionType.FillInTheBlank).Select(v => v.QuestionId).Take(5).ToArrayAsync();

        singleQuestions.AddRange(multipleQuestions);
        singleQuestions.AddRange(truefalseQuestions);
        singleQuestions.AddRange(fillblankQuestions);

        if (singleQuestions.Count == 0) {
            return (initExamPaper, "没有找到任何可用的题目");
        }

        int order = 1;
        var questions = singleQuestions.Select(v => new ExamPaperQuestion { QuestionId = v, Order = order++ });
        initExamPaper.ExamPaperQuestions.AddRange(questions);
        initExamPaper.TotalQuestions = initExamPaper.ExamPaperQuestions.Count;

        try {
            await _dbContext.ExamPapers.AddAsync(initExamPaper);
            await _dbContext.SaveChangesAsync();
        }
        catch (ReferenceConstraintException ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "保存随机生成的试卷时失败");
            return (initExamPaper, "随机生成失败，请尝试重新生成");
        }
        catch (Exception ex) {
            Debug.Assert(false);
            _logger.LogError(ex, "保存随机生成的试卷时失败");
            throw;
        }

        return (initExamPaper, null);
    }

    public async Task<(List<ExamPaper> examPapers, Dictionary<string, string[]> errors)> ImportFromExcelAsync(string examPaperName, Stream stream) {
        var result = ExamPaperExcelParser.Parse(stream);
        if (result.errors.Count > 0) {
            return result;
        }
        try {
            if (string.IsNullOrWhiteSpace(examPaperName) is false) {
                foreach (var item in result.examPapers) {
                    item.ExamPaperName = examPaperName;
                }
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

    public async Task<(string? firstName, string? error)> ExportToExcelAsync(Stream stream, int[] examPaperIds) {
        var examPapers = await _dbContext.ExamPapers
            .AsNoTracking()
            .Include(v => v.ExamPaperQuestions)
            .ThenInclude(v => v.Question)
            .ThenInclude(v => v.Options)
            .Where(v => examPaperIds.Contains(v.ExamPaperId))
            .OrderByDescending(v => v.ExamPaperId)
            .ToArrayAsync();
        if (examPapers.Length == 0) {
            return (null, "没有找到任何的试卷");
        }
        ExamPaperExcelGenerator.Generate(stream, examPapers);
        return (examPapers[0].ExamPaperName, null);
    }

    public (string? firstName, string? error) ExportToExcelTemplate(Stream stream) {
        var examPaper = new ExamPaper {
            ExamPaperName = "试卷导入模板",
        };

        examPaper.ExamPaperQuestions.AddRange([
            new ExamPaperQuestion {
                Order = 1,
                Question = new Question {
                    QuestionText = "题目1描述？",
                    QuestionType = QuestionType.SingleChoice,
                    DifficultyLevel = DifficultyLevel.Easy,
                    CorrectAnswer = "A",
                }
            },
            new ExamPaperQuestion {
                Order = 2,
                Question = new Question {
                    QuestionText = "题目2描述？",
                    QuestionType = QuestionType.MultipleChoice,
                    DifficultyLevel = DifficultyLevel.Medium,
                    CorrectAnswer = "ABC",
                }
            },
            new ExamPaperQuestion {
                Order = 3,
                Question = new Question {
                    QuestionText = "题目3描述？",
                    QuestionType = QuestionType.TrueFalse,
                    DifficultyLevel = DifficultyLevel.Medium,
                    CorrectAnswer = "1",
                }
            },
            new ExamPaperQuestion {
                Order = 4,
                Question = new Question {
                    QuestionText = "题目4描述？",
                    QuestionType = QuestionType.TrueFalse,
                    DifficultyLevel = DifficultyLevel.Medium,
                    CorrectAnswer = "0",
                }
            },
            new ExamPaperQuestion {
                Order = 5,
                Question = new Question {
                    QuestionText = "题目5描述？",
                    QuestionType = QuestionType.FillInTheBlank,
                    DifficultyLevel = DifficultyLevel.Hard,
                    CorrectAnswer = "填空题答案",
                }
            },
        ]);
        examPaper.ExamPaperQuestions[0].Question.Options.AddRange([
            new Option {
                OptionCode = 'A',
                OptionText = "选项1",
            },
            new Option {
                OptionCode = 'B',
                OptionText = "选项2",
            },
            new Option {
                OptionCode = 'C',
                OptionText = "选项3",
            },
            new Option {
                OptionCode = 'D',
                OptionText = "选项4",
            },
        ]);
        examPaper.ExamPaperQuestions[1].Question.Options.AddRange([
             new Option {
                OptionCode = 'A',
                OptionText = "选项1",
            },
            new Option {
                OptionCode = 'B',
                OptionText = "选项2",
            },
            new Option {
                OptionCode = 'C',
                OptionText = "选项3",
            },
            new Option {
                OptionCode = 'D',
                OptionText = "选项4",
            },
        ]);
        examPaper.TotalQuestions = examPaper.ExamPaperQuestions.Count;

        ExamPaperExcelGenerator.Generate(stream, [examPaper]);
        return (examPaper.ExamPaperName, null);
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
            examPaper.TotalQuestions = examPaper.ExamPaperQuestions.Count;
        }
        examPaper.DifficultyLevel = (DifficultyLevel)examPaper.ExamPaperQuestions.Average(v => (int)v.Question.DifficultyLevel);
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
            // return $"第{row}行{col}列的值{cellValue}不能为空白";
            // 允许不设置答案（不设置答案的题目，提交任何答案都将判对）
            return null;
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
            var c = col - 5;// 减去前面的5列
            if (c > 26) {
                return null;
            }
            var option = new Option {
                OptionText = cellValue,
                OptionCode = Convert.ToChar(c + 64),// A字母的ASCII是65，列索引从1开始
            };
            question.Question.Options.Add(option);
        }
        return null;
    }
}

public class ExamPaperExcelGenerator {

    public void Generate(Stream stream, ExamPaper[] examPapers) {
        int sheetIndex = 0;
        string name = string.Empty;
        using var package = new ExcelPackage();
        foreach (var examPaper in examPapers) {
            name = examPaper.ExamPaperName;
        isExists:
            if (package.Workbook.Worksheets.Any(v => v.Name == name)) {
                name = $"{name}-{sheetIndex++}";
                goto isExists;
            }
            if (string.IsNullOrWhiteSpace(name)) {
                name = "Sheet1";
            }
            var worksheet = package.Workbook.Worksheets.Add(name);
            SetTitle(worksheet);
            for (int row = 2; row < examPaper.ExamPaperQuestions.Count + 2; row++) {
                var item = examPaper.ExamPaperQuestions.OrderBy(v => v.Order).ToArray()[row - 2];
                worksheet.Cells[row, 1].Value = item.Order;
                worksheet.Cells[row, 2].Value = item.Question.QuestionText;
                worksheet.Cells[row, 3].Value = GetQuestionTypeString(item.Question.QuestionType);
                worksheet.Cells[row, 4].Value = (int)item.Question.DifficultyLevel;
                worksheet.Cells[row, 5].Value = item.Question.CorrectAnswer;
                int i = 1;
                foreach (var option in item.Question.Options.OrderBy(v => v.OptionCode)) {
                    worksheet.Cells[row, 5 + i].Value = option.OptionText;
                    i++;
                }
            }
        }

        stream.Position = 0;
        package.SaveAs(stream);
        stream.Position = 0;
    }

    private static void SetTitle(ExcelWorksheet worksheet) {
        worksheet.Cells[1, 1].Value = "排序";
        worksheet.Cells[1, 2].Value = "题目";
        worksheet.Cells[1, 3].Value = "类型";
        worksheet.Cells[1, 4].Value = "难度";
        worksheet.Cells[1, 5].Value = "答案";
        worksheet.Cells[1, 6].Value = "选项A";
        worksheet.Cells[1, 7].Value = "选项B";
        worksheet.Cells[1, 8].Value = "选项C";
        worksheet.Cells[1, 9].Value = "选项D";

        using var range = worksheet.Cells["A1:I1"];
        range.Style.Font.Bold = true;
    }

    private static string? GetQuestionTypeString(QuestionType questionType) {
        return questionType switch {
            QuestionType.SingleChoice => "单选题",
            QuestionType.MultipleChoice => "多选题",
            QuestionType.TrueFalse => "判断题",
            QuestionType.FillInTheBlank => "填空题",
            QuestionType.None => null,
            _ => throw new NotImplementedException(),
        };
    }
}