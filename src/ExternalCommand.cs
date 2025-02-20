using System.Diagnostics;

namespace Src;

public class ExternalCommand : ICommand
{
    public void Execute(string[] args)
    {
        bool found = false;

        foreach (var path in Shell.EnvPaths)
        {
            if (File.Exists(Path.Combine(path, args[0])))
            {
                var arguments = args.Skip(1).ToArray();

                using (var process = new Process())
                {
                    process.StartInfo.FileName = args[0];
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    foreach (var argument in arguments)
                    {
                        process.StartInfo.ArgumentList.Add(argument);
                    }
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    Console.Write(output);
                    Console.Write(error);

                    process.WaitForExit();
                }

                found = true;
                break;
            }
        }

        if (!found)
        {
            Console.WriteLine($"{args[0]}: command not found");
        }
    }
}
