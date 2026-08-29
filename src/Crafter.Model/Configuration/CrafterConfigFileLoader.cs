using Auturge.Common.Configuration;

namespace Crafter.Model.Configuration;

public class CrafterConfigFileLoader : ConfigFileLoader<CrafterConfig>
{
    public List<Exception> Exceptions { get; set; } = [];
    
    // public override CrafterConfig Load(List<FileInfo>? configFiles) => Load(configFiles, []);

    public override void Load(CrafterConfig config)
    {
        Exceptions.Clear();
        
        LoadConfigFromConfigFiles(config.ConfigFiles);

        // script files override the config from the config files.
        LoadConfigFromScriptFiles(config.ScriptFiles);
    }
    
    // public CrafterConfig Load(List<FileInfo>? configFiles, List<FileInfo>? scriptFiles)
    // {
    //     Exceptions.Clear();
    //     var result = new CrafterConfig(configFiles, scriptFiles);
    //     LoadConfigFromConfigFiles(configFiles);
    //
    //     // script files override the config from the config files.
    //     LoadConfigFromScriptFiles(scriptFiles);
    //     
    //     return result;
    // }

    private void LoadConfigFromConfigFiles(List<FileInfo>? configFiles)
    {
        if (configFiles == null || configFiles.Count == 0)
        {
            return;
        }

        // TODO: parse the files, and generate the config
        return;
    }

    private void LoadConfigFromScriptFiles(List<FileInfo>? scriptFiles)
    {
        if (scriptFiles == null || scriptFiles.Count == 0)
        {
            return;
        }

        // TODO: parse the files, and generate the config
        return;
    }
}
