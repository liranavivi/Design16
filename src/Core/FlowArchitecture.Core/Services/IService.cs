using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlowArchitecture.Core.Services
{
    /// <summary>
    /// Base interface for all services in the Flow Architecture system.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Gets the unique identifier for the service.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Gets the display name of the service.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the description of the service.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Gets the current status of the service.
        /// </summary>
        ServiceStatus Status { get; }
        
        /// <summary>
        /// Initializes the service with the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuration parameters for the service.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InitializeAsync(IServiceConfiguration configuration, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StopAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets detailed status information about the service.
        /// </summary>
        /// <returns>A <see cref="ServiceStatusInfo"/> object containing detailed status information.</returns>
        ServiceStatusInfo GetStatusInfo();
    }
}
