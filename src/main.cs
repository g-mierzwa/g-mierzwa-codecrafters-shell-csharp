using System.Net;
using System.Net.Sockets;

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
        default:
            Console.WriteLine($"{tokens[0]}: command not found");
            break; 
    }
}
