using Auturge.Cli.Infrastructure;
using Crafter.Cli.Infrastructure;
using Crafter.Model.Configuration;

namespace Crafter.Cli;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things
    // aren't initialized yet and stuff might break.
    [STAThread]
    public static int Main(string[] args)
    {
        CliApp app = new();
        CrafterConfig config = app.Initialize(args);
        
        return !config.InitializationStatus.IsSuccess
            ? config.InitializationStatus.StateCode
            : app.Start(config).StateCode;
    }
}
