using System.CommandLine;
using Auturge.Cli.Infrastructure;
using Auturge.Common.Infrastructure;
using Auturge.Common.Processing;
using Crafter.Model.Configuration;

namespace Crafter.Cli.Infrastructure;

public class CliApp(CrafterConfigResolver? configResolver = null) : IApplicationLifecycle<CrafterConfig>
{
    public CrafterConfig Config { get; private set; } = CrafterConfig.Empty;

    private readonly CrafterConfigResolver _configResolver = configResolver ?? new CrafterConfigResolver();

    private static readonly RootCommand _rootCommand = CliCommandLine.GetRootCommand();
    private static readonly ProcessStatus _status = new();

    public CrafterConfig Initialize(string[] args)
    {
        _status.MarkInitializing();

        Config = _configResolver.Resolve(_rootCommand, args);

        if (Config.InitializationStatus.IsSuccess)
        {
            _status.MarkInitialized();
        }
        else
        {
            _status.MarkFailed(Config.InitializationStatus);
        }

        return Config;
    }

    public ProcessStatus Start(CrafterConfig config)
    {
        _status.MarkStarted();
        Console.WriteLine("fuck yeah, get some");
        _status.MarkCompleted();
        return _status;
    }
}
