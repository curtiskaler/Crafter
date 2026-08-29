using Auturge.Common.Processing;

namespace Auturge.Common.Infrastructure;

public class InitializationStatus : ProcessStatus
{
    /// <summary>
    /// A boolean property to indicate if the initialization has completed or terminated 
    /// </summary>
    public bool IsComplete => CompletionTime != null;


    /// <summary>
    /// A boolean property to indicate if the initialization was successful 
    /// </summary>
    public bool IsSuccess => IsComplete && CurrentState == LifecycleState.Completed;
    
    /// <summary>
    /// A boolean property to indicate if the initialization was specifically failure 
    /// </summary>
    public bool IsFailure => CurrentState == LifecycleState.Failed;
}
