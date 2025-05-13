namespace FlowArchitecture.Core.Services
{
    /// <summary>
    /// Represents the status of a service.
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// The service is not initialized.
        /// </summary>
        NotInitialized = 0,
        
        /// <summary>
        /// The service is initialized but not started.
        /// </summary>
        Initialized = 1,
        
        /// <summary>
        /// The service is starting.
        /// </summary>
        Starting = 2,
        
        /// <summary>
        /// The service is running.
        /// </summary>
        Running = 3,
        
        /// <summary>
        /// The service is stopping.
        /// </summary>
        Stopping = 4,
        
        /// <summary>
        /// The service is stopped.
        /// </summary>
        Stopped = 5,
        
        /// <summary>
        /// The service has encountered an error.
        /// </summary>
        Error = 6
    }
}
