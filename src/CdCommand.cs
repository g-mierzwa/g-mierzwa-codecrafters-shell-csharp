using System.Runtime.InteropServices;

namespace Src;

public class CdCommand : ICommand
{
    public void Execute(string[] args)
    {
        if (args.Length > 1)
        {
            try
            {
                string? homeDir = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                                Environment.GetEnvironmentVariable("USERPROFILE") :
                                Environment.GetEnvironmentVariable("HOME");
                string path = args[1].Replace("~", homeDir);
                Directory.SetCurrentDirectory(path);
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"cd: {args[1]}: No such file or directory");
            }
        }
    }
}
