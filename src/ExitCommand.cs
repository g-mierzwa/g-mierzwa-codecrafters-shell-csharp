namespace Src;

public class ExitCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (args.Length > 1 && !string.IsNullOrEmpty(args[1]) && int.TryParse(args[1], out int exitCode))
        {
            Environment.Exit(exitCode);
        }
        else
        {
            Environment.Exit(0);
        }
    }
}
