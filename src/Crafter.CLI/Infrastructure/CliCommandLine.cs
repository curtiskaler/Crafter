using System.CommandLine;

namespace Auturge.Cli.Infrastructure;

internal static class CliCommandLine
{
    internal static RootCommand GetRootCommand()
    {
        Option<FileInfo> fileOption = new("--file")
        {
            Description = "The file to read and display on the console"
        };

        RootCommand rootCommand = new("Sample app for System.CommandLine");
        rootCommand.Options.Add(fileOption);
        
        return rootCommand;
    }
}
