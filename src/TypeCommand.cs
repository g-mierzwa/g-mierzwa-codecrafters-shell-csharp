namespace Src
{
    public class TypeCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length > 1)
            {
                if (Shell.AvailableCommands.ContainsKey(args[1]))
                {
                    Console.WriteLine($"{args[1]} is a shell builtin");
                }
                else
                {
                    bool found = false;
                    foreach (var path in Shell.EnvPaths)
                    {
                        if (File.Exists(Path.Combine(path, args[1])))
                        {
                            Console.WriteLine($"{args[1]} is {Path.Combine(path, args[1])}");
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Console.WriteLine($"{args[1]}: not found");
                    }
                }
            }
            else
            {
                Console.WriteLine("");
            }
        }
    }
}