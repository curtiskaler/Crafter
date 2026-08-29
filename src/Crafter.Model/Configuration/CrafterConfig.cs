using System.CommandLine;
using Auturge.Common.Infrastructure;

namespace Crafter.Model.Configuration;

public class CrafterConfig(List<FileInfo>? cFiles, List<FileInfo>? sFiles) : IAppConfig
{
    public static CrafterConfig Empty => new([], []);

    public InitializationStatus InitializationStatus { get; } = new();
    public ParseResult? CommandLine { get; set; } = null;
    public List<FileInfo> ConfigFiles { get; } = cFiles ?? [];
    public List<FileInfo> ScriptFiles { get; } = sFiles ?? [];
}
