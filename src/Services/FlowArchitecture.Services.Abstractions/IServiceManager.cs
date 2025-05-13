using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Common.Services;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Services;

namespace FlowArchitecture.Services.Abstractions
{
    /// <summary>
    /// Interface for service managers in the Flow Architecture system.
    /// </summary>
    /// <typeparam name="TService">The type of service managed by this manager.</typeparam>
    public interface IServiceManager<TService> : IService where TService : IService
    {
        /// <summary>
        /// Registers a service with the manager.
        /// </summary>
        /// <param name="service">The service to register.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the registration.</returns>
        Task<RegistrationResult> RegisterAsync(TService service, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Validates a service.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>A validation result indicating whether the service is valid.</returns>
        ValidationResult ValidateService(TService service);
        
        /// <summary>
        /// Gets a service by its identifier and version.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the service if found; otherwise, null.</returns>
        Task<TService> GetServiceAsync(string serviceId, string version, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all services with the specified identifier.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the services with the specified identifier.</returns>
        Task<IEnumerable<TService>> GetAllServicesAsync(string serviceId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all registered services.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing all registered services.</returns>
        Task<IEnumerable<TService>> GetAllServicesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Unregisters a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing a value indicating whether the service was unregistered.</returns>
        Task<bool> UnregisterServiceAsync(string serviceId, string version, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the status of a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the service status.</returns>
        Task<ServiceStatusInfo> GetServiceStatusAsync(string serviceId, string version, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates the status of a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="status">The new service status.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateServiceStatusAsync(string serviceId, string version, ServiceStatus status, CancellationToken cancellationToken = default);
    }
}
