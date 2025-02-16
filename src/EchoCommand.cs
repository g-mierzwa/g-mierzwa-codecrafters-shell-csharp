namespace Src
{
    public class EchoCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine(string.Join(' ', args.Skip(1).ToArray()));
            }
            else
            {
                Console.WriteLine("");
            }
        }
    }
}