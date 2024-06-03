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
                        Register(context);
                        break;
                    case "2":
                        Login(context);
                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        WriteLine("Invalid choice. Try again.");
                        break;
                }
            }
        }
    }

    static void Register(TestingDbContext context)
    {
        Clear();
        WriteLine("Register a new account");
        Write("Enter username: ");
        string username = ReadLine();
        Write("Enter password: ");
        string password = ReadLine();

        var user = new User
        {
            Username = username,
            Password = password,
            RegistrationDate = DateTime.Now,
            AttemptsLimit = 3,
            IsAdmin = false
        };

        context.Users.Add(user);
        context.SaveChanges();

        WriteLine("Registration successful!");
        ReadLine();
    }

    static void Login(TestingDbContext context)
    {
        Clear();
        WriteLine("Login to your account");
        Write("Enter username: ");
        string username = ReadLine();
        Write("Enter password: ");
        string password = ReadLine();

        var user = context.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
        if (user != null)
        {
            WriteLine("Login successful!");
            if (user.IsAdmin)
            {
                AdminMenu(context, user);
            }
            else
            {
                UserMenu(context, user);
            }
        }
        else
        {
            WriteLine("Invalid username or password.");
            ReadLine();
        }
    }

    static void UserMenu(TestingDbContext context, User user)
    {
        bool logout = false;

        while (!logout)
        {
            Clear();
            WriteLine($"Welcome, {user.Username}");
            WriteLine("1. Take a Test");
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
                    logout = true;
                    break;
                default:
                    WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
    }

    static void AdminMenu(TestingDbContext context, User admin)
    {
        bool logout = false;

        while (!logout)
        {
            Clear();
            WriteLine($"Welcome, Admin {admin.Username}");
            WriteLine("1. Create Test");
            WriteLine("2. Edit Test");
            WriteLine("3. Delete Test");
            WriteLine("4. Logout");
            Write("Choose an option: ");
            string choice = ReadLine();

            switch (choice)
            {
                case "1":
                    CreateTest(context, admin);
                    break;
                case "2":
                    EditTest(context);
                    break;
                case "3":
                    DeleteTest(context);
                    break;
                case "4":
                    logout = true;
                    break;
                default:
                    WriteLine("Invalid choice. Try again.");
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
            Author = admin,
            CreationDate = DateTime.Now,
            IsPublished = false,
            Questions = new List<Question>()
        };


        while (true)
        {
            AddQuestion(context, ref test);
            WriteLine("Press [C] To continue adding questions" +
                          "\nPress any other key to stop adding answers");
            if (ReadKey().Key != ConsoleKey.C)
            {
                break;
            }
        }

        test.IsPublished = true;
        context.Tests.Add(test);
        context.SaveChanges();

        WriteLine("Test created successfully!");
        EditTest(context, test.TestId);
    }

    static void EditTest(TestingDbContext context, int? testId = null)
    {
        if (testId == null)
        {
            Clear();
            var tests = context.Tests.ToList();
            if (!tests.Any())
            {
                WriteLine("No tests available to edit.");
                ReadLine();
                return;
            }

            WriteLine("Available Tests:");
            foreach (var test in tests)
            {
                WriteLine($"{test.TestId}. {test.Name}");
            }

            Write("Enter test ID to edit: ");
            testId = int.Parse(ReadLine());
        }

        var selectedTest = context.Tests.Include(t => t.Questions).SingleOrDefault(t => t.TestId == testId);

        if (selectedTest == null)
        {
            WriteLine("Test not found.");
            ReadLine();
            return;
        }

        bool done = false;
        while (!done)
        {
            Clear();
            WriteLine($"Editing Test: {selectedTest.Name}");
            WriteLine("1. Add Question");
            WriteLine("2. Edit Question");
            WriteLine("3. Delete Question");
            WriteLine("4. Done");
            Write("Choose an option: ");
            string choice = ReadLine();

            switch (choice)
            {
                case "1":
                    AddQuestion(context, ref selectedTest);
                    break;
                case "2":
                    EditQuestion(context, ref selectedTest);
                    break;
                case "3":
                    DeleteQuestion(context, ref selectedTest);
                    break;
                case "4":
                    done = true;
                    break;
                default:
                    WriteLine("Invalid choice. Try again.");
                    break;
            }
        }
    }

    static void AddQuestion(TestingDbContext context, ref Test test)
    {
        Clear();
        WriteLine("Enter your question description: ");
        string description = ReadLine();
        Question question = new Question();
        question.Options = new List<Answer>();
        question.Text = description;
        while (true)
        {
            Answer the_answer = new Answer();
            WriteLine("Enter description of answer for the question: ");
            string op = ReadLine();
            the_answer.Question = question;
            the_answer.Text = op;
            WriteLine("Is that correct answer?" +
                      "\n[Y] Yes " +
                      "\n[Any other key] No");
            char.TryParse(Console.ReadLine(), out char isCorr);

            if (isCorr == 'Y' || isCorr == 'y')
            {
                the_answer.IsCorrect = true;
                question.Options.Add(the_answer);
            }

            else
            {
                the_answer.IsCorrect = false;
                question.Options.Add(the_answer);
            }
            if (question.Options.Count > 1)
                question.Type = QuestionType.MultipleChoice;
            else
                question.Type = QuestionType.SingleChoice;
            WriteLine("Press [C] To continue adding options" +
                      "\nPress any other key to stop adding answers");
            if (ReadKey().Key != ConsoleKey.C)
            {
                break;
            }
        }
        WriteLine("Enter weight (count of marks) of your question: ");
        int.TryParse(ReadLine(), out int mark);
        question.Weight = mark;
    }

    static void EditQuestion(TestingDbContext context, ref Test test)
    {
        Clear();
        if (!test.Questions.Any())
        {
            WriteLine("No questions available to edit.");
            ReadLine();
            return;
        }

        WriteLine("Available Questions:");
        foreach (var question in test.Questions)
        {
            WriteLine($"{question.QuestionId}. {question.Text}");
        }

        Write("Enter question ID to edit: ");
        int questionId = int.Parse(ReadLine());

        var selectedQuestion = test.Questions.SingleOrDefault(q => q.QuestionId == questionId);

        if (selectedQuestion == null)
        {
            WriteLine("Question not found.");
            ReadLine();
            return;
        }

        Write("Enter new question text: ");
        selectedQuestion.Text = ReadLine();
        while (true)
        {
            Answer the_answer = new Answer();
            WriteLine("Enter description of answer for the question: ");
            string op = ReadLine();
            the_answer.Question = selectedQuestion;
            the_answer.Text = op;
            WriteLine("Is that correct answer?" +
                      "\n[Y] Yes " +
                      "\n[Any other key] No");
            char.TryParse(Console.ReadLine(), out char isCorr);

            if (isCorr == 'Y' || isCorr == 'y')
            {
                the_answer.IsCorrect = true;
                selectedQuestion.Options.Add(the_answer);
            }

            else
            {
                the_answer.IsCorrect = false;
                selectedQuestion.Options.Add(the_answer);
            }
            WriteLine("Press [C] To continue adding options" +
                      "\nPress any other key to stop adding answers");
            if (ReadKey().Key != ConsoleKey.C)
            {
                break;
            }
        }
        WriteLine("Enter new question weight: ");
        selectedQuestion.Weight = int.Parse(ReadLine());
        WriteLine("Enter new question type (0 for SingleChoice, 1 for MultipleChoice): ");
        selectedQuestion.Type = (QuestionType)int.Parse(ReadLine());

        context.Questions.Update(selectedQuestion);
        context.SaveChanges();

        WriteLine("Question edited successfully!");
    }

    static void DeleteQuestion(TestingDbContext context, ref Test test)
    {
        Clear();
        if (!test.Questions.Any())
        {
            WriteLine("No questions available to delete.");
            ReadLine();
            return;
        }

        WriteLine("Available Questions:");
        foreach (var question in test.Questions)
        {
            WriteLine($"{question.QuestionId}. {question.Text}");
        }

        Write("Enter question ID to delete: ");
        int questionId = int.Parse(ReadLine());

        var selectedQuestion = test.Questions.SingleOrDefault(q => q.QuestionId == questionId);

        if (selectedQuestion == null)
        {
            WriteLine("Question not found.");
            ReadLine();
            return;
        }

        context.Questions.Remove(selectedQuestion);
        context.SaveChanges();

        WriteLine("Question deleted successfully!");
    }

    static void DeleteTest(TestingDbContext context)
    {
        Clear();
        var tests = context.Tests.ToList();
        if (!tests.Any())
        {
            WriteLine("No tests available to delete.");
            ReadLine();
            return;
        }

        WriteLine("Available Tests:");
        foreach (var test in tests)
        {
            WriteLine($"{test.TestId}. {test.Name}");
        }

        Write("Enter test ID to delete: ");
        int testId = int.Parse(ReadLine());

        var selectedTest = context.Tests.Include(t => t.Questions).SingleOrDefault(t => t.TestId == testId);

        if (selectedTest == null)
        {
            WriteLine("Test not found.");
            ReadLine();
            return;
        }

        context.Tests.Remove(selectedTest);
        context.SaveChanges();

        WriteLine("Test deleted successfully!");
    }

    static void TakeTest(TestingDbContext context, User user)
    {
        Clear();
        var tests = context.Tests.Where(t => t.IsPublished).ToList();
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

        var selectedTest = context.Tests.Include(t => t.Questions).SingleOrDefault(t => t.TestId == testId);

        if (selectedTest == null)
        {
            WriteLine("Test not found.");
            ReadLine();
            return;
        }

        var testSession = new TestSession
        {
            UserId = user.UserId,
            TestId = selectedTest.TestId,
            StartTime = DateTime.Now,
            Questions = selectedTest.Questions,
            Score = 0
        };

        foreach (var question in selectedTest.Questions)
        {
            Clear();
            WriteLine($"Question: {question.Text}");
            for (int i = 0; i < question.Options.Count; i++)
            {
                WriteLine($"{i}. {question.Options[i]}");
            }

            WriteLine("Enter your answers (comma separated): ");
            var answers = ReadLine().Split(',').Select(int.Parse).ToList();
            bool isCorrectAnswer = CheckAnswers(question, answers);
            if (isCorrectAnswer)
            {
                testSession.Score += question.Weight;
            }
        }

        testSession.EndTime = DateTime.Now;

        context.TestSessions.Add(testSession);
        context.SaveChanges();

        WriteLine($"Test completed! Your score: {testSession.Score}");
        ReadLine();
    }

    static bool CheckAnswers(Question question, List<int> answersNums)
    {
        foreach (var num in answersNums)
        {
            if (question.Options[num].IsCorrect == false)
                return false;
            
        }
        return true;
    }

    static void ViewResults(TestingDbContext context, User user)
    {
        Clear();
        var sessions = context.TestSessions.Include(ts => ts.Test).Where(ts => ts.UserId == user.UserId).ToList();

        if (!sessions.Any())
        {
            WriteLine("No test sessions found.");
            ReadLine();
            return;
        }

        WriteLine("Your Test Sessions:");
        foreach (var session in sessions)
        {
            WriteLine($"Test: {session.Test.Name}, Score: {session.Score}, Completed: {session.EndTime}");
        }

        ReadLine();
    }
}