using Auturge.Common.Infrastructure;

namespace Auturge.Common.Configuration;

public abstract class ConfigFileLoader<TConfig> where TConfig : IAppConfig
{
    /// <summary>
    /// Load config and script files, and apply configuration to the Config
    /// </summary>
    /// <param name="config">The configuration object to resolve.</param>
    public abstract void Load(TConfig config);
}
