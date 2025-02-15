using System.Net;
using System.Net.Sockets;

while(true)
{
    Console.Write("$ ");
    var command = Console.ReadLine();
    var tokens = command.Split(' ');

    if (tokens[0] == "exit")
    {
        if (int.TryParse(tokens[1], out int exitCode))
        {
            Environment.Exit(exitCode);
        }
        else
        {
            Console.WriteLine("Wrong argument for exit command");
        }
    }
    else
    {
        Console.WriteLine($"{tokens[0]}: command not found");
    }
}
