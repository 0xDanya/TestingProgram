using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;

namespace TestingProgram
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int AttemptsLimit { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool IsAdmin { get; set; }
        public List<TestSession> TestSessions { get; set; }
    }

    public class Question
    {
        [Key]
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public List<int> CorrectAnswers { get; set; }
        public int Weight { get; set; }
        public QuestionType Type { get; set; }
        public int TestId { get; set; }
        public Test Test { get; set; }
    }

    public enum QuestionType
    {
        SingleChoice,
        MultipleChoice
    }

    public class Test
    {
        [Key]
        public int TestId { get; set; }
        public string Name { get; set; }
        public List<Question> Questions { get; set; }
        public bool IsPublished { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class TestSession
    {
        [Key]
        public int TestSessionId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TestId { get; set; }
        public Test Test { get; set; }
        public Dictionary<int, List<int>> Answers { get; set; }
        public int? Score { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsCompleted => EndTime.HasValue;
    }

    public class TestingDbContext : DbContext
    {
        public TestingDbContext(DbContextOptions<TestingDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestSession> TestSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.TestSessions)
                .WithOne(ts => ts.User)
                .HasForeignKey(ts => ts.UserId);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Test)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TestId);

            modelBuilder.Entity<Test>()
                .HasOne(t => t.Author)
                .WithMany()
                .HasForeignKey(t => t.AuthorId);

            modelBuilder.Entity<TestSession>()
                .HasOne(ts => ts.User)
                .WithMany(u => u.TestSessions)
                .HasForeignKey(ts => ts.UserId);

            modelBuilder.Entity<TestSession>()
                .HasOne(ts => ts.Test)
                .WithMany()
                .HasForeignKey(ts => ts.TestId);
        }
    }
}
