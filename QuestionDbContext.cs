using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;

namespace QuestionApi;

public class QuestionDbContext : DbContext {
    public DbSet<Question> Questions { get; set; }
    public DbSet<User> Users { get; set; }
    public string DbPath { get; }

    public QuestionDbContext() {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "question.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

/// <summary>
/// 题库表
/// </summary>
public class Question {
    public int QuestionId { get; set; }
    public string Remark { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
}

/// <summary>
/// 题型枚举
/// </summary>
public enum QuestionType {
    single,
    multiple,
    panduan,
    tiankong
}

public class User {
    public int UserId { get; set; }
}