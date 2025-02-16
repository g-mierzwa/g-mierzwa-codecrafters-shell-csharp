using System.Text.RegularExpressions;

namespace Src
{
    public class Shell
    {
        public static readonly Dictionary<string, ICommand> AvailableCommands =new Dictionary<string, ICommand>
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
                //var arguments = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var arguments = SeperateInput(input);

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

        private static string[] SeperateInput(string input)
        {
            //string pattern = @"[^\s""']+|""([^""]*)""|'([^']*)'";
            string pattern = @"'(.*?)'|(\S+)";
            var output = new List<string>();

            foreach (Match m in Regex.Matches(input, pattern))
            {
                if (m.Groups[1].Success)
                {
                    output.Add(m.Groups[1].Value);
                    Console.WriteLine(m.Groups[1].Value);
                }
                else
                {
                    output.Add(m.Groups[2].Value);
                    Console.WriteLine(m.Groups[2].Value);
                }
            }

            return output.ToArray();
        }
    }
}