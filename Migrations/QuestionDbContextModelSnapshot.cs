﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using QuestionApi.Database;

#nullable disable

namespace QuestionApi.Migrations
{
    [DbContext(typeof(QuestionDbContext))]
    partial class QuestionDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("QuestionApi.Database.AnswerHistory", b =>
                {
                    b.Property<int>("AnswerHistoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("AnswerHistoryId"));

                    b.Property<int>("ExamPaperId")
                        .HasColumnType("integer");

                    b.Property<int?>("ExaminationId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsSubmission")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("StudentId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("SubmissionTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TotalIncorrectAnswers")
                        .HasColumnType("integer");

                    b.HasKey("AnswerHistoryId");

                    b.HasIndex("ExamPaperId");

                    b.HasIndex("ExaminationId");

                    b.HasIndex("StudentId");

                    b.ToTable("AnswerHistories");
                });

            modelBuilder.Entity("QuestionApi.Database.AppUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NickName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("QuestionApi.Database.ExamPaper", b =>
                {
                    b.Property<int>("ExamPaperId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ExamPaperId"));

                    b.Property<int>("DifficultyLevel")
                        .HasColumnType("integer");

                    b.Property<string>("ExamPaperName")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.HasKey("ExamPaperId");

                    b.ToTable("ExamPapers");
                });

            modelBuilder.Entity("QuestionApi.Database.ExamPaperQuestion", b =>
                {
                    b.Property<int>("ExamPaperId")
                        .HasColumnType("integer");

                    b.Property<int>("QuestionId")
                        .HasColumnType("integer");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.HasKey("ExamPaperId", "QuestionId");

                    b.HasIndex("QuestionId");

                    b.ToTable("ExamPaperQuestions");
                });

            modelBuilder.Entity("QuestionApi.Database.Examination", b =>
                {
                    b.Property<int>("ExaminationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ExaminationId"));

                    b.Property<int>("DifficultyLevel")
                        .HasColumnType("integer");

                    b.Property<int>("DurationSeconds")
                        .HasColumnType("integer");

                    b.Property<int>("ExamPaperId")
                        .HasColumnType("integer");

                    b.Property<string>("ExaminationName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<int>("ExaminationType")
                        .HasColumnType("integer");

                    b.Property<int>("Order")
                        .HasColumnType("integer");

                    b.HasKey("ExaminationId");

                    b.HasIndex("ExamPaperId");

                    b.ToTable("Examinations");
                });

            modelBuilder.Entity("QuestionApi.Database.Option", b =>
                {
                    b.Property<int>("OptionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("OptionId"));

                    b.Property<bool>("IsCorrect")
                        .HasColumnType("boolean");

                    b.Property<char>("OptionCode")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("character(1)")
                        .HasDefaultValue('A');

                    b.Property<string>("OptionText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("QuestionId")
                        .HasColumnType("integer");

                    b.HasKey("OptionId");

                    b.HasIndex("QuestionId");

                    b.ToTable("Options");
                });

            modelBuilder.Entity("QuestionApi.Database.Question", b =>
                {
                    b.Property<int>("QuestionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("QuestionId"));

                    b.Property<string>("CorrectAnswer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DifficultyLevel")
                        .HasColumnType("integer");

                    b.Property<string>("QuestionText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("QuestionType")
                        .HasColumnType("integer");

                    b.HasKey("QuestionId");

                    b.ToTable("Questions");
                });

            modelBuilder.Entity("QuestionApi.Database.Student", b =>
                {
                    b.Property<int>("StudentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("StudentId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.HasKey("StudentId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Students");
                });

            modelBuilder.Entity("QuestionApi.Database.StudentAnswer", b =>
                {
                    b.Property<int>("StudentAnswerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("StudentAnswerId"));

                    b.Property<int>("AnswerHistoryId")
                        .HasColumnType("integer");

                    b.Property<string>("AnswerText")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsCorrect")
                        .HasColumnType("boolean");

                    b.Property<int>("QuestionId")
                        .HasColumnType("integer");

                    b.Property<int>("QuestionType")
                        .HasColumnType("integer");

                    b.Property<int>("StudentId")
                        .HasColumnType("integer");

                    b.HasKey("StudentAnswerId");

                    b.HasIndex("AnswerHistoryId");

                    b.HasIndex("QuestionId");

                    b.HasIndex("StudentId");

                    b.ToTable("StudentAnswers");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("QuestionApi.Database.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("QuestionApi.Database.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuestionApi.Database.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("QuestionApi.Database.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("QuestionApi.Database.AnswerHistory", b =>
                {
                    b.HasOne("QuestionApi.Database.ExamPaper", "ExamPaper")
                        .WithMany("AnswerHistories")
                        .HasForeignKey("ExamPaperId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuestionApi.Database.Examination", "Examination")
                        .WithMany("AnswerHistories")
                        .HasForeignKey("ExaminationId");

                    b.HasOne("QuestionApi.Database.Student", "Student")
                        .WithMany("AnswerHistories")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExamPaper");

                    b.Navigation("Examination");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("QuestionApi.Database.ExamPaperQuestion", b =>
                {
                    b.HasOne("QuestionApi.Database.ExamPaper", "ExamPaper")
                        .WithMany("ExamPaperQuestions")
                        .HasForeignKey("ExamPaperId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuestionApi.Database.Question", "Question")
                        .WithMany("ExamPaperQuestions")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExamPaper");

                    b.Navigation("Question");
                });

            modelBuilder.Entity("QuestionApi.Database.Examination", b =>
                {
                    b.HasOne("QuestionApi.Database.ExamPaper", "ExamPaper")
                        .WithMany("Examinations")
                        .HasForeignKey("ExamPaperId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ExamPaper");
                });

            modelBuilder.Entity("QuestionApi.Database.Option", b =>
                {
                    b.HasOne("QuestionApi.Database.Question", "Question")
                        .WithMany("Options")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Question");
                });

            modelBuilder.Entity("QuestionApi.Database.Student", b =>
                {
                    b.HasOne("QuestionApi.Database.AppUser", "User")
                        .WithOne("Student")
                        .HasForeignKey("QuestionApi.Database.Student", "UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("QuestionApi.Database.StudentAnswer", b =>
                {
                    b.HasOne("QuestionApi.Database.AnswerHistory", "AnswerHistory")
                        .WithMany("StudentAnswers")
                        .HasForeignKey("AnswerHistoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuestionApi.Database.Question", "Question")
                        .WithMany("StudentAnswers")
                        .HasForeignKey("QuestionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("QuestionApi.Database.Student", "Student")
                        .WithMany("StudentAnswers")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AnswerHistory");

                    b.Navigation("Question");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("QuestionApi.Database.AnswerHistory", b =>
                {
                    b.Navigation("StudentAnswers");
                });

            modelBuilder.Entity("QuestionApi.Database.AppUser", b =>
                {
                    b.Navigation("Student");
                });

            modelBuilder.Entity("QuestionApi.Database.ExamPaper", b =>
                {
                    b.Navigation("AnswerHistories");

                    b.Navigation("ExamPaperQuestions");

                    b.Navigation("Examinations");
                });

            modelBuilder.Entity("QuestionApi.Database.Examination", b =>
                {
                    b.Navigation("AnswerHistories");
                });

            modelBuilder.Entity("QuestionApi.Database.Question", b =>
                {
                    b.Navigation("ExamPaperQuestions");

                    b.Navigation("Options");

                    b.Navigation("StudentAnswers");
                });

            modelBuilder.Entity("QuestionApi.Database.Student", b =>
                {
                    b.Navigation("AnswerHistories");

                    b.Navigation("StudentAnswers");
                });
#pragma warning restore 612, 618
        }
    }
}
