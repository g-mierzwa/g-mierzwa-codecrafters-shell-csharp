using System.Text;
using System.Text.RegularExpressions;

namespace Src;

public class Shell
{
    public static readonly Dictionary<string, ICommand> AvailableCommands = new Dictionary<string, ICommand>
    {
        {"exit", new ExitCommand()},
        {"echo", new EchoCommand()},
        {"type", new TypeCommand()},
        {"pwd", new PwdCommand()},
        {"cd", new CdCommand()},
    };
    public static readonly string[] EnvPaths = Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator);
    private static StreamWriter? writer = null;
    private static bool isOutputRedirected = false;
    public void Run()
    {
        while (true)
        {
            Console.Write("$ ");
            string input = ReadUserInput();
            var arguments = ParseInput(input);
            SetOutput(ref arguments, ref writer);

            if (arguments.Length == 0)
            {
                continue;
            }
            else if (Shell.AvailableCommands.TryGetValue(arguments[0], out ICommand? value))
            {
                value.Execute(arguments);
            }
            else
            {
                new ExternalCommand().Execute(arguments);
            }

            RestoreDefaultOutput(ref writer);
        }
    }

    private static string[] ParseInput(string input)
    {
        var output = new List<string>();
        string currentToken = "";
        bool isInsideSingleQuotes = false;
        bool isInsideDoubleQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char currentCharacter = input[i];
            char[] specialCharacters = ['\\', '$', '"', '\n'];

            switch (currentCharacter)
            {
                case '\'':
                    if (isInsideDoubleQuotes)
                    {
                        currentToken += currentCharacter;
                    }
                    else
                    {
                        isInsideSingleQuotes = !isInsideSingleQuotes;
                    }
                    break;
                case '"':
                    if (isInsideSingleQuotes)
                    {
                        currentToken += currentCharacter;
                    }
                    else
                    {
                        isInsideDoubleQuotes = !isInsideDoubleQuotes;
                    }
                    break;
                case '\\':
                    if (i + 1 < input.Length)
                    {
                        char nextCharacter = input[i + 1];
                        if ((!isInsideSingleQuotes && !isInsideDoubleQuotes) ||
                            (isInsideDoubleQuotes && specialCharacters.Contains(nextCharacter)))
                        {
                            currentToken += nextCharacter;
                            i++;
                        }
                        else
                        {
                            currentToken += currentCharacter;
                        }
                    }
                    break;
                case ' ':
                    if (!isInsideSingleQuotes && !isInsideDoubleQuotes && currentToken.Length > 0)
                    {
                        if (i + 1 >= input.Length || input[i + 1] != ' ')
                        {
                            output.Add(currentToken);
                            currentToken = "";
                        }
                    }
                    else
                    {
                        currentToken += currentCharacter;
                    }
                    break;
                default:
                    currentToken += currentCharacter;
                    break;
            }
        }
        if (currentToken.Length > 0)
        {
            output.Add(currentToken);
        }
        return output.ToArray();
    }

    public static void SetOutput(ref string[] args, ref StreamWriter? writer)
    {
        int redirectOperatorIndex = -1;
        bool errorRedirection = false;
        bool append = false;
        
        if (args.Contains(">") || args.Contains("1>"))
        {
            redirectOperatorIndex = Array.IndexOf(args, ">") >= 0 ? Array.IndexOf(args, ">") : Array.IndexOf(args, "1>");
            errorRedirection = false;
            append = false;
        }
        else if (args.Contains("2>"))
        {
            redirectOperatorIndex = Array.IndexOf(args, "2>");
            errorRedirection = true;
            append = false;
        }
        else if (args.Contains(">>") || args.Contains("1>>"))
        {
            redirectOperatorIndex = Array.IndexOf(args, ">>") >= 0 ? Array.IndexOf(args, ">>") : Array.IndexOf(args, "1>>");
            errorRedirection = false;
            append = true;
        }
        else if (args.Contains("2>>"))
        {
            redirectOperatorIndex = Array.IndexOf(args, "2>>");
            errorRedirection = true;
            append = true;
        }

        if (redirectOperatorIndex > 0 && args.Length > redirectOperatorIndex + 1)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), args[redirectOperatorIndex + 1]);
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            writer = append ? new StreamWriter(path, true) : new StreamWriter(path);
            writer.AutoFlush = true;

            if (errorRedirection)
            {
                Console.SetError(writer);
            }
            else
            {
                Console.SetOut(writer);
            }
            isOutputRedirected = true;
            args = args.Skip(0).Take(redirectOperatorIndex).ToArray();
        }
    }

    public static void RestoreDefaultOutput(ref StreamWriter? writer)
    {
        if (isOutputRedirected)
        {
            writer.Close();
            var stdOut = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(stdOut);
            var stdError = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
            Console.SetError(stdError);
            isOutputRedirected = false;
        }
    }

    public static string ReadUserInput()
    {
        var input = new StringBuilder();
        var userInputThread = new Thread(() =>
        {
            while (true)
            {
                var nextKey = Console.ReadKey(true);

                if (nextKey.Key == ConsoleKey.Tab)
                {
                    string? autocompleted = AutocompleteBultin(input.ToString());
                    if (!string.IsNullOrEmpty(autocompleted))
                    {
                        Console.Clear();                                            //TODO Clear single line instead
                        Console.Write($"$ {autocompleted} ");
                        input.Clear();
                        input.Append($"{autocompleted} ");
                    }
                }
                else if (nextKey.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else if (nextKey.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.Clear();                                            //TODO Clear single line instead
                    Console.Write($"$ {input.ToString()}");
                }
                else
                {
                    input.Append(nextKey.KeyChar);
                    Console.Write(nextKey.KeyChar);
                }
            }
        });

        userInputThread.Start();
        userInputThread.Join();

        return input.ToString();
    }

    public static string? AutocompleteBultin(string input)
    {
        int count = 0;
        string? foundBuiltin = null;

        foreach (string builtin in AvailableCommands.Keys)
        {
            if (builtin.StartsWith(input))
            {
                foundBuiltin = builtin;
                count++;
            }
        }

        if (count == 1)
        {
            return foundBuiltin;
        }
        else
        {
            Console.Write("\a");
            return null;
        }
    }
}
