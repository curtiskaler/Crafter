using System.CommandLine;
using Auturge.Common.Infrastructure;
using Crafter.Model.Configuration;

namespace Crafter.Cli.Infrastructure;

public interface ICliAppConfig : IAppConfig
{
    static ICliAppConfig Default => CliAppConfig.Default;

    /// <summary> The configuration loaded from the config files. </summary>
    CrafterConfig? RuntimeConfig { get; internal set; }

    /// <summary> The list of DSL files to execute </summary>
    List<FileInfo> ScriptFiles { get; }
}

internal class CliAppConfig : ICliAppConfig
{
    public static CliAppConfig Default => new DefaultCliAppConfig();
    
    public ParseResult? CommandLine { get; set; } = null;
    public InitializationStatus InitializationStatus { get; } = new();
    public CrafterConfig? RuntimeConfig { get; set; } = null;
    public List<FileInfo> ConfigFiles { get; } = [];
    public List<FileInfo> ScriptFiles { get; } = [];
}

internal class DefaultCliAppConfig : CliAppConfig
{
}
