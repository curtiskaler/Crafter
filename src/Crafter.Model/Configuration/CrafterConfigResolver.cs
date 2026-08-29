using System.CommandLine;
using Auturge.Common.CommandLine;

namespace Crafter.Model.Configuration;

public class CrafterConfigResolver(CrafterConfigFileLoader? configLoader = null)
{
    private CrafterConfigFileLoader ConfigFileLoader { get; } = configLoader ?? new CrafterConfigFileLoader();

    public CrafterConfig Resolve(RootCommand rootCommand, string[]? args)
    {
        CrafterConfig config = CrafterConfig.Empty;

        config.InitializationStatus.MarkStarted();
        args ??= Environment.GetCommandLineArgs();

        config.CommandLine = rootCommand.Parse(args);
        if (config.CommandLine.GetException() is { } exception)
        {
            // command-line parse exception :(
            config.InitializationStatus.MarkFailed(exception);
            return config;
        }

        // parse the config and script files for configuration bits
        ConfigFileLoader.Load(config);
        if (ConfigFileLoader.Exceptions.Count != 0)
        {
            config.InitializationStatus.MarkFailed(ConfigFileLoader.Exceptions);
        }
        else
        {
            config.InitializationStatus.MarkCompleted();
        }
        
        
        return config;
    }
}
