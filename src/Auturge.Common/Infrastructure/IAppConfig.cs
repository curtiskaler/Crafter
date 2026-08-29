using System.CommandLine;

namespace Auturge.Common.Infrastructure;

public interface IAppConfig
{
    InitializationStatus InitializationStatus { get; }
    
    /// <summary> The parsed results of the command line arguments </summary>
    ParseResult? CommandLine { get; }
    
    /// <summary> The list of config files to load </summary>
    List<FileInfo> ConfigFiles { get; }
}
