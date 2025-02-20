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
            string input = Console.ReadLine();
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
        int i = Array.IndexOf(args, "1>");
        bool errorRedirection = false;
        if (i < 0)
        {
            i = Array.IndexOf(args, ">");
        }
        if (i < 0)
        {
            i = Array.IndexOf(args, "2>");
            if (i > 0)
            {
                errorRedirection = true;
            }
        }
        if (i > 0 && args.Length > i + 1)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), args[i + 1]);
            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            writer = new StreamWriter(path) { AutoFlush = true };
            if (errorRedirection)
            {
                Console.SetError(writer);
            }
            else
            {
                Console.SetOut(writer);
            }
            isOutputRedirected = true;
            args = args.Skip(0).Take(i).ToArray();
        }
    }

    public static void RestoreDefaultOutput(ref StreamWriter? writer)
    {
        if (isOutputRedirected)
        {
            writer.Close();
            var stdOut = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(stdOut);
            isOutputRedirected = false;
        }
    }
}
