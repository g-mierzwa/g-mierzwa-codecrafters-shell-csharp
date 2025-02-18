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

    public void Run()
    {
        while (true)
        {
            Console.Write("$ ");
            string input = Console.ReadLine();
            var arguments = ParseInput(input);

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
                    isInsideSingleQuotes = !isInsideSingleQuotes;
                    if (isInsideDoubleQuotes)
                    {
                        currentToken += currentCharacter;
                    }
                    break;
                case '"':
                    isInsideDoubleQuotes = !isInsideDoubleQuotes;
                    if (isInsideSingleQuotes)
                    {
                        currentToken += currentCharacter;
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
}
