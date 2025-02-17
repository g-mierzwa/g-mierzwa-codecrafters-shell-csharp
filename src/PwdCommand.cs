namespace Src;

public class PwdCommand : ICommand
{
    public void Execute(string[] args)
    {
        Console.WriteLine(Directory.GetCurrentDirectory());
    }
}
