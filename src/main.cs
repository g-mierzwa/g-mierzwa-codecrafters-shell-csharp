using System.Net;
using System.Net.Sockets;

var availableCommands = new string[] {"exit", "echo", "type"};

while(true)
{
    Console.Write("$ ");
    var command = Console.ReadLine();
    var tokens = command.Split(' ');

    switch (tokens[0])
    {
        case "exit":
            if (tokens.Length == 2 && int.TryParse(tokens[1], out int exitCode))
            {
                Environment.Exit(exitCode);
            }
            else
            {
                Console.WriteLine("Wrong argument for exit command");
            }
            break;
        case "echo":
            Console.WriteLine(command.Length > 4 ? command.Substring(5) : "");
            break;
        case "type":
            if (tokens.Length == 2)
            {
                if (availableCommands.Contains(tokens[1]))
                {
                    Console.WriteLine($"{tokens[1]} is a shell builtin");
                }
                else
                {
                    var paths = Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator);
                    bool found = false;
                    foreach (var path in paths)
                    {
                        if (File.Exists(Path.Combine(path, tokens[1])))
                        {
                            Console.WriteLine($"{tokens[1]} is {Path.Combine(path, tokens[1])}");
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        Console.WriteLine($"{tokens[1]}: not found");
                    }
                }
            }
            else
            {
                Console.WriteLine("Wrong argument for type command");
            }
            break;
        default:
            Console.WriteLine($"{tokens[0]}: command not found");
            break; 
    }
}
