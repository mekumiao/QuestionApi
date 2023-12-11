using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionApi;

public class QuestionDbContext : IdentityDbContext {
    public DbSet<Question> Questions { get; set; }
    public string DbPath { get; }

    public QuestionDbContext() {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "question.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

/// <summary>
/// 题目表
/// </summary>
public class Question {
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public ICollection<Option> Options { get; set; } = [];
}

/// <summary>
/// 题型枚举
/// </summary>
public enum QuestionType {
    SingleChoice,
    MultipleChoice,
    TrueFalse,
    FillInTheBlank
}

/// <summary>
/// 选项表
/// </summary>
public class Option {
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
