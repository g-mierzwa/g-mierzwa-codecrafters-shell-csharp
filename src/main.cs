using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

var availableCommands = new string[] {"exit", "echo", "type"};

while(true)
{
    Console.Write("$ ");
    var command = Console.ReadLine();
    var tokens = command.Split(' ');

    var envPaths = Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator);
    bool found;

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
                    found = false;
                    foreach (var path in envPaths)
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
            found = false;
            foreach (var path in envPaths)
            {
                if (File.Exists(Path.Combine(path, tokens[0])))
                {
                    var arguments = new string[tokens.Length - 1];
                    Array.Copy(tokens, 1, arguments, 0, tokens.Length - 1);
                    
                    using (var process = new Process())
                    {
                        process.StartInfo.FileName = tokens[0];
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        for (int i = 1; i < tokens.Length; i++)
                        {
                            process.StartInfo.ArgumentList.Add(tokens[i]);
                        }
                        process.Start();

                        var reader = process.StandardOutput;
                        string output = reader.ReadToEnd();
                        Console.WriteLine(output);

                        process.WaitForExit();
                    }
                    
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Console.WriteLine($"{tokens[0]}: command not found");
            }
            break;
    }
}
