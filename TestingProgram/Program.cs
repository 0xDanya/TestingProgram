using Microsoft.EntityFrameworkCore;
using TestingProgram;
using static System.Console;

internal class Program
{
    private static void Main(string[] args)
    {

        using (var context = new TestingDbContext())
        {
            bool exit = false;

            while (!exit)
            {
                Clear();
                WriteLine("Welcome to the Test System");
                WriteLine("1. Register");
                WriteLine("2. Login");
                WriteLine("3. Exit");
                Write("Choose an option: ");
                string choice = ReadLine();

                switch (choice)
                {
                    case "1":
                        Register(context, false);
                        break;
                    case "2":
                        Login(context);
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        WriteLine("Invalid choice. Try again.");
                        ReadLine();
                        break;
                }
            }
        }
    }

    private static void Register(TestingDbContext context, bool isAdmin)
    {
        Clear();
        WriteLine("Register a new user");
        Write("Username: ");
        string username = ReadLine();
        Write("Password: ");
        string password = ReadLine();


        User existingUser = context.Users.SingleOrDefault(u => u.Username == username);
        if (existingUser != null)
        {
            WriteLine("A user with this username already exists. Please choose a different username.");
            ReadLine();
            return;
        }

        var user = new User
        {
            Username = username,
            Password = password,
            RegistrationDate = DateTime.Now,
            IsAdmin = isAdmin,
            AttemptsLimit = 3,
            TestSessions = new List<TestSession>()
        };

        context.Users.Add(user);
        context.SaveChanges();

        WriteLine("User registered successfully!");
        ReadLine();
    }

    private static void Login(TestingDbContext context)
    {
        Clear();

        //WriteLine("Login");
        //Write("Username: ");
        //string username = ReadLine();
        //Write("Password: ");
        //string password = ReadLine();

        //var user = context.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
        //if (user == null)
        //{
        //    WriteLine("Invalid username or password.");
        //    ReadLine();
        //    return;
        //}

        //if (user.IsAdmin)
        //{
        //    AdminMenu(context, user);
        //}
        //else
        //{
        //    UserMenu(context, user);
        //}
        var user = context.Users.SingleOrDefault(u => u.Username == "Junior" && u.Password == "12345");
        UserMenu(context, user);
    }

    private static void AdminMenu(TestingDbContext context, User admin)
    {
        bool exit = false;

        while (!exit)
        {
            Clear();
            WriteLine($"Welcome, {admin.Username} (Admin)");
            WriteLine("1. Create Test");
            WriteLine("2. Edit Test");
            WriteLine("3. Delete Test");
            WriteLine("4. View Results");
            WriteLine("5. Create admin account");
            WriteLine("6. Logout");
            Write("Choose an option: ");
            string choice = ReadLine();

            switch (choice)
            {
                case "1":
                    CreateTest(context, admin);
                    break;
                case "2":
                    Write("Enter test ID to edit: ");
                    int editTestId = int.Parse(ReadLine());
                    EditTest(context, editTestId);
                    break;
                case "3":
                    Write("Enter test ID to delete: ");
                    int deleteTestId = int.Parse(ReadLine());
                    DeleteTest(context, deleteTestId);
                    break;
                case "4":
                    ViewResults(context, admin);
                    break;
                case "5":
                    Register(context, true);
                    break;
                case "6":
                    exit = true;
                    break;
                default:
                    WriteLine("Invalid choice. Try again.");
                    ReadLine();
                    break;
            }
        }
    }

    private static void UserMenu(TestingDbContext context, User user)
    {
        bool exit = false;

        while (!exit)
        {
            Clear();
            WriteLine($"Welcome, {user.Username}");
            WriteLine("1. Take Test");
            WriteLine("2. View Results");
            WriteLine("3. Logout");
            Write("Choose an option: ");
            string choice = ReadLine();

            switch (choice)
            {
                case "1":
                    TakeTest(context, user);
                    break;
                case "2":
                    ViewResults(context, user);
                    break;
                case "3":
                    exit = true;
                    break;
                default:
                    WriteLine("Invalid choice. Try again.");
                    ReadLine();
                    break;
            }
        }
    }

    static void CreateTest(TestingDbContext context, User admin)
    {
        Clear();
        Write("Enter test name: ");
        string testName = ReadLine();

        var test = new Test
        {
            Name = testName,
            AuthorId = admin.UserId,
            CreationDate = DateTime.Now,
            IsPublished = false,
            Questions = new List<Question>()
        };

        context.Tests.Add(test);
        context.SaveChanges();

        WriteLine("Test created successfully!");
        EditTest(context, test.TestId);
    }

    static void EditTest(TestingDbContext context, int testId)
    {
        var test = context.Tests.Include(t => t.Questions)
                                .ThenInclude(q => q.Options)
                                .SingleOrDefault(t => t.TestId == testId);

        if (test == null)
        {
            WriteLine("Test not found.");
            ReadLine();
            return;
        }

        while (true)
        {
            Clear();
            WriteLine($"Editing Test: {test.Name}");
            WriteLine("1. Add Question");
            WriteLine("2. Remove Question");
            WriteLine("3. Publish Test");
            WriteLine("4. Save and Exit");
            Write("Choose an option: ");
            int choice = int.Parse(ReadLine());

            switch (choice)
            {
                case 1:
                    AddQuestion(context, test);
                    break;
                case 2:
                    RemoveQuestion(context, test);
                    break;
                case 3:
                    test.IsPublished = true;
                    context.SaveChanges();
                    WriteLine("Test published successfully!");
                    ReadLine();
                    return;
                case 4:
                    context.SaveChanges();
                    return;
                default:
                    WriteLine("Invalid choice.");
                    ReadLine();
                    break;
            }
        }
    }

    static void AddQuestion(TestingDbContext context, Test test)
    {
        Clear();
        Write("Enter question text: ");
        string questionText = ReadLine();

        Write("Enter question weight: ");
        int weight = int.Parse(ReadLine());

        Write("Enter question type (1: SingleChoice, 2: MultipleChoice): ");
        int questionTypeInt = int.Parse(ReadLine());
        QuestionType questionType = questionTypeInt == 1 ? QuestionType.SingleChoice : QuestionType.MultipleChoice;

        var question = new Question
        {
            Text = questionText,
            Weight = weight,
            Type = questionType,
            TestId = test.TestId,
            Options = new List<Answer>()
        };

        Write("Enter number of options: ");
        int optionCount = int.Parse(ReadLine());

        for (int i = 0; i < optionCount; i++)
        {
            Write($"Enter option {i + 1} text: ");
            string optionText = ReadLine();

            Write($"Is option {i + 1} correct? (y/n): ");
            bool isCorrect = ReadLine().ToLower() == "y";

            question.Options.Add(new Answer
            {
                Text = optionText,
                IsCorrect = isCorrect,
                Question = question
            });
        }

        test.Questions.Add(question);
        context.SaveChanges();
    }

    static void RemoveQuestion(TestingDbContext context, Test test)
    {
        Clear();
        WriteLine("Questions:");
        foreach (var question in test.Questions)
        {
            WriteLine($"{question.QuestionId}. {question.Text}");
        }

        Write("Enter question ID to remove: ");
        int questionId = int.Parse(ReadLine());
        var questionToRemove = context.Questions.Include(q => q.Options).SingleOrDefault(q => q.QuestionId == questionId);

        if (questionToRemove != null)
        {
            context.Questions.Remove(questionToRemove);
            context.SaveChanges();
            WriteLine("Question removed successfully!");
        }
        else
        {
            WriteLine("Question not found.");
        }

        ReadLine();
    }

    static void DeleteTest(TestingDbContext context, int testId)
    {
        var test = context.Tests.Include(t => t.Questions).ThenInclude(q => q.Options).SingleOrDefault(t => t.TestId == testId);

        if (test == null)
        {
            WriteLine("Test not found.");
            ReadLine();
            return;
        }

        context.Tests.Remove(test);
        context.SaveChanges();

        WriteLine("Test deleted successfully!");
        ReadLine();
    }

    static void TakeTest(TestingDbContext context, User user)
    {
        Clear();
        var tests = context.Tests.Include(t => t.Questions).ThenInclude(q => q.Options).Where(t => t.IsPublished).ToList();
        if (!tests.Any())
        {
            WriteLine("No tests available.");
            ReadLine();
            return;
        }

        WriteLine("Available Tests:");
        foreach (var test in tests)
        {
            WriteLine($"{test.TestId}. {test.Name}");
        }

        Write("Enter test ID to take: ");
        int testId = int.Parse(ReadLine());
        var selectedTest = context.Tests.Include(t => t.Questions).ThenInclude(q => q.Options).SingleOrDefault(t => t.TestId == testId);

        if (selectedTest == null)
        {
            WriteLine("Test not found.");
            ReadLine();
            return;
        }

        if (context.TestSessions.Count(ts => ts.UserId == user.UserId && ts.TestId == testId) >= user.AttemptsLimit)
        {
            WriteLine("You have reached the attempt limit for this test.");
            ReadLine();
            return;
        }

        var testSession = new TestSession
        {
            UserId = user.UserId,
            TestId = testId,
            StartTime = DateTime.Now,
            Questions = selectedTest.Questions.ToList()
        };

        context.TestSessions.Add(testSession);
        context.SaveChanges();

        var userAnswers = new Dictionary<int, List<int>>();

        foreach (var question in selectedTest.Questions)
        {
            Clear();
            WriteLine(question.Text);
            for (int i = 0; i < question.Options.Count; i++)
            {
                WriteLine($"{i + 1}. {question.Options[i].Text}");
            }

            var answers = new List<int>();
            if (question.Type == QuestionType.SingleChoice)
            {
                Write("Enter the number of the correct option: ");
                answers.Add(int.Parse(ReadLine()));
            }
            else if (question.Type == QuestionType.MultipleChoice)
            {
                Write("Enter the numbers of the correct options separated by commas: ");
                answers = ReadLine().Split(',').Select(s => int.Parse(s.Trim())).ToList();
            }

            userAnswers.Add(question.QuestionId, answers);
        }

        List<Answer> userAnswList = new();

        foreach (var answer in userAnswers)
        {
            foreach (var optionId in answer.Value)
            {
                var answerRecord = new Answer
                {
                    QuestionId = answer.Key,
                    AnswerId = optionId
                };
                userAnswList.Add(answerRecord);
            }
        }

        testSession.EndTime = DateTime.Now;
        context.SaveChanges();

        CalculateScore(context, /*ref*/ testSession, userAnswList);

        WriteLine("Test completed!");
        ReadLine();
    }

    static void CalculateScore(TestingDbContext context, /*ref*/ TestSession testSession, List<Answer> userAnswers)
    {
        int totalScore = 0;
        foreach (var question in testSession.Questions)
        {
            var correctOptions = question.Options.Where(o => o.IsCorrect).Select(o => o.AnswerId).ToList();
            var userOptions = userAnswers.Select(o => o.AnswerId).ToList();
            if (correctOptions.Count == userOptions.Count && !correctOptions.Except(userOptions).Any())
            {
                totalScore += question.Weight;
            }
        }

        testSession.Score = totalScore;
        context.SaveChanges();
    }

    static void ViewResults(TestingDbContext context, User user)
    {
        Clear();
        //var sessions = context.TestSessions.Include(ts => ts.Test).Where(ts => ts.UserId == user.UserId && ts.IsCompleted).ToList();
        List<TestSession> sessions = context.TestSessions
                        .Include(ts => ts.Test)
                        .Where(ts => ts.UserId == user.UserId)
                        //.Where(ts => Convert.ToBoolean(ts.IsCompleted) == true)
                        .ToList();
        if (!sessions.Any())
        {
            WriteLine("No completed test sessions found.");
            ReadLine();
            return;
        }

        //WriteLine("Completed Test Sessions:");
        WriteLine("Test Sessions:");
        foreach (var session in sessions)
        {
            WriteLine($"Test: {session.Test.Name}, Score: {session.Score}, Completed: {session.EndTime}");
        }

        ReadLine();
    }
}