using System.Net.NetworkInformation;
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
            bool pressedTab = false;
            while (true)
            {
                var nextKey = Console.ReadKey(true);

                if (nextKey.Key == ConsoleKey.Tab && !pressedTab)
                {
                    string? autocompleted = AutocompleteCommand(input.ToString(), out bool full);
                    if (!string.IsNullOrEmpty(autocompleted))
                    {
                        Console.Clear();                                            //TODO Clear single line instead
                        input.Clear();
                        Console.Write($"$ {autocompleted}");
                        input.Append($"{autocompleted}");
                        if (full)
                        {
                            Console.Write(' ');
                            input.Append(' ');
                        }
                    }
                    pressedTab = true;
                }
                else if(nextKey.Key == ConsoleKey.Tab && pressedTab)
                {
                    string? autocompleted = AutocompleteCommand(input.ToString(), out bool full, true);
                    pressedTab = false;
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
                    pressedTab = false;
                }
                else
                {
                    input.Append(nextKey.KeyChar);
                    Console.Write(nextKey.KeyChar);
                    pressedTab = false;
                }
            }
        });

        userInputThread.Start();
        userInputThread.Join();

        return input.ToString();
    }

    public static string? AutocompleteCommand(string input, out bool full, bool printList = false)
    {
        List<string> autocompletableCommands = new();
        List<string> foundCommands = new();
        
        foreach (var builtin in AvailableCommands.Keys)
        {
            autocompletableCommands.Add(builtin);
        }

        foreach (var path in EnvPaths)
        {
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    autocompletableCommands.Add(Path.GetFileName(file));
                }
            }
        }
        autocompletableCommands = autocompletableCommands.Distinct().ToList();
        autocompletableCommands.Sort();

        foreach (var command in autocompletableCommands)
        {
            if (command.StartsWith(input))
            {
                foundCommands.Add(command);
            }
        }

        if (foundCommands.Count == 1)
        {
            full = true;
            return foundCommands[0];
        }
        else if (!printList)
        {
            string commonPrefix = LongestCommonPrefix(foundCommands.ToArray());
            if (string.IsNullOrEmpty(commonPrefix))
            {
                Console.Write("\a");
                full = false;
                return null;
            }
            else
            {
                full = false;
                Console.Write("\a");
                return commonPrefix;
            }
        }
        else
        {
            Console.WriteLine();
            foreach (var command in foundCommands)
            {
                Console.Write($"{command}  ");
            }
            Console.WriteLine();
            Console.Write($"$ {input}");
            full = false;
            return null;
        }
    }

    public static string LongestCommonPrefix(string[] input)
    {
        if (input.Length == 0)
        {
            return "";
        }
        string prefix = "";
        for (int i = 0; i < input[0].Length; i++)
        {
            char c = input[0][i];
            for (int j = 1; j < input.Length; j++)
            {
                if (i >= input[j].Length || input[j][i] != c)
                {
                    return prefix;
                }
            }
            prefix += c;
        }
        return prefix;
    }
}
