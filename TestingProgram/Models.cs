using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations.Schema;

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
        public List<Answer> Options { get; set; }
        public int Weight { get; set; }
        public QuestionType Type { get; set; }
        public int TestId { get; set; }
        public Test Test { get; set; }
    }

    public class Answer
    {
        [Key]
        public int AnswerId { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
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
        public List<TestSession> TestSessions { get; set; } 
    }

    public class TestSession
    {
        [Key]
        public int TestSessionId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int TestId { get; set; }
        public Test Test { get; set; }
        public List<Question> Questions { get; set; }
        public int? Score { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsCompleted => EndTime.HasValue;
    }

    public class TestingDbContext : DbContext
    {
        public TestingDbContext() : base() { try { Database.EnsureCreated(); } catch (Exception ex) { Console.WriteLine(ex); } }
        public TestingDbContext(DbContextOptions<TestingDbContext> options)
        : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer
        ("data source=(localdb)\\MSSQLLocalDB;initial catalog=TestingDB;integrated security=True;MultipleActiveResultSets=true");

        public DbSet<User> Users { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<TestSession> TestSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
            .HasMany(u => u.TestSessions)
            .WithOne(ts => ts.User)
            .HasForeignKey(ts => ts.UserId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Test>()
                .HasMany(t => t.Questions).WithOne(q => q.Test);
            modelBuilder.Entity<Test>()
                .HasMany(t => t.TestSessions).WithOne(ts => ts.Test);
            modelBuilder.Entity<TestSession>()
                .HasMany(ts => ts.Questions);
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Options).WithOne(a => a.Question);
            modelBuilder.Entity<Question>()
                .Property(q => q.Type).HasConversion(
                v => v.ToString(),
                v => (QuestionType)Enum.Parse(typeof(QuestionType), v));
        }
    }
}