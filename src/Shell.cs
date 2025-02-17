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
        int currentPosition = 0;

        while (currentPosition >= 0)
        {
            currentPosition = input.IndexOfAny([' ', '"', '\'']);

            if (currentPosition < 0)
            {
                continue;
            }

            switch (input[currentPosition])
            {
                case ' ':
                    string textToAdd = input.Substring(0, currentPosition);
                    textToAdd = textToAdd.Replace("\\", "");
                    currentToken += textToAdd;
                    input = input.Substring(currentPosition + 1).TrimStart();
                    output.Add(currentToken);
                    currentToken = "";
                    break;
                case '\'':
                    currentToken += input.Substring(0, currentPosition);
                    input = input.Substring(currentPosition + 1);
                    currentPosition = input.IndexOf('\'');
                    currentToken += input.Substring(0, currentPosition);
                    input = input.Substring(currentPosition + 1);
                    break;
                case '"':
                    currentToken += input.Substring(0, currentPosition);
                    input = input.Substring(currentPosition + 1);
                    currentPosition = input.IndexOf('"');
                    currentToken += input.Substring(0, currentPosition);
                    input = input.Substring(currentPosition + 1);
                    break;
            }
        }
        currentToken += input;
        output.Add(currentToken);
        return output.ToArray();
    }
}
