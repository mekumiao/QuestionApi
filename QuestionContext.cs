using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;

namespace QuestionApi;

public class QuestionContext : DbContext {
    public DbSet<Question> Questions { get; set; }
    public DbSet<User> Users { get; set; }
    public string DbPath { get; }

    public QuestionContext() {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "blogging.db");
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
    public string Type { get; set; } = string.Empty;
}

public class User {
    public int UserId { get; set; }
}