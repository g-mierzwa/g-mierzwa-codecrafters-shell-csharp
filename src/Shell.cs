namespace Src
{
    public class Shell
    {
        //private static readonly string[] AvailableCommands = ["exit", "echo", "type", "pwd", "cd"];
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
                var arguments = Console.ReadLine().Split(' ');

                if (Shell.AvailableCommands.TryGetValue(arguments[0], out ICommand? value))
                {
                    value.Execute(arguments);
                }
                else
                {
                    new ExternalCommand().Execute(arguments);
                }
            }
        }
    }
}