using Auturge.Common.Processing;

namespace Auturge.Common.Infrastructure;

public interface IApplicationLifecycle<TConfig> where TConfig : IAppConfig
{
    abstract TConfig Config { get; }
    
    abstract TConfig Initialize(string[] args);

    abstract ProcessStatus Start(TConfig config);
    
    // abstract ProcessStatus Stop(); // pass in the reason somehow?
}
