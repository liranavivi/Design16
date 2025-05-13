using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Common.Services;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Messaging;
using FlowArchitecture.Core.Services;
using Microsoft.Extensions.Logging;

namespace FlowArchitecture.Services.Abstractions
{
    /// <summary>
    /// Base implementation for service managers in the Flow Architecture system.
    /// </summary>
    /// <typeparam name="TService">The type of service managed by this manager.</typeparam>
    public abstract class AbstractServiceManager<TService> : AbstractServiceBase, IServiceManager<TService>, IMessageConsumer<ServiceRegistrationCommand<TService>> where TService : IService
    {
        /// <summary>
        /// Gets the registered services.
        /// </summary>
        protected IDictionary<string, IDictionary<string, TService>> RegisteredServices { get; }
        
        /// <summary>
        /// Gets the service statuses.
        /// </summary>
        protected IDictionary<string, IDictionary<string, ServiceStatusInfo>> ServiceStatuses { get; }
        
        /// <summary>
        /// Gets the synchronization object for thread safety.
        /// </summary>
        protected object SyncRoot { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractServiceManager{TService}"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        protected AbstractServiceManager(string id, string name, string description, ILogger logger)
            : base(id, name, description, logger)
        {
            RegisteredServices = new Dictionary<string, IDictionary<string, TService>>();
            ServiceStatuses = new Dictionary<string, IDictionary<string, ServiceStatusInfo>>();
            SyncRoot = new object();
        }
        
        /// <summary>
        /// Registers a service with the manager.
        /// </summary>
        /// <param name="service">The service to register.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the registration.</returns>
        public virtual async Task<RegistrationResult> RegisterAsync(TService service, CancellationToken cancellationToken = default)
        {
            try
            {
                if (Status != ServiceStatus.Running)
                {
                    return RegistrationResult.Failure($"Service manager is not running. Current status: {Status}");
                }
                
                if (service == null)
                {
                    return RegistrationResult.Failure("Service cannot be null.");
                }
                
                var validationResult = ValidateService(service);
                if (!validationResult.IsValid)
                {
                    return RegistrationResult.ValidationFailure(validationResult);
                }
                
                Logger.LogInformation("Registering service {ServiceId} (version {Version})...", service.Id, service.Name);
                
                var result = await OnRegisterAsync(service, cancellationToken);
                
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Service {ServiceId} (version {Version}) registered successfully.", service.Id, service.Name);
                }
                else
                {
                    Logger.LogWarning("Failed to register service {ServiceId} (version {Version}): {ErrorMessage}", 
                        service.Id, service.Name, result.ErrorMessage);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error registering service: {ErrorMessage}", ex.Message);
                return RegistrationResult.Failure(ex.Message);
            }
        }
        
        /// <summary>
        /// Validates a service.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>A validation result indicating whether the service is valid.</returns>
        public virtual ValidationResult ValidateService(TService service)
        {
            var errors = new List<ValidationError>();
            
            if (service == null)
            {
                return ValidationResult.Failure("service", "Service cannot be null.");
            }
            
            if (string.IsNullOrEmpty(service.Id))
            {
                errors.Add(new ValidationError("Id", "Service ID is required."));
            }
            
            if (string.IsNullOrEmpty(service.Name))
            {
                errors.Add(new ValidationError("Name", "Service name is required."));
            }
            
            return new ValidationResult(errors);
        }
        
        /// <summary>
        /// Gets a service by its identifier and version.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the service if found; otherwise, null.</returns>
        public virtual Task<TService> GetServiceAsync(string serviceId, string version, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serviceId))
            {
                throw new ArgumentException("Service ID cannot be null or empty.", nameof(serviceId));
            }
            
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Version cannot be null or empty.", nameof(version));
            }
            
            lock (SyncRoot)
            {
                if (RegisteredServices.TryGetValue(serviceId, out var versions) && versions.TryGetValue(version, out var service))
                {
                    return Task.FromResult(service);
                }
            }
            
            return Task.FromResult<TService>(default);
        }
        
        /// <summary>
        /// Gets all services with the specified identifier.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the services with the specified identifier.</returns>
        public virtual Task<IEnumerable<TService>> GetAllServicesAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serviceId))
            {
                throw new ArgumentException("Service ID cannot be null or empty.", nameof(serviceId));
            }
            
            lock (SyncRoot)
            {
                if (RegisteredServices.TryGetValue(serviceId, out var versions))
                {
                    return Task.FromResult<IEnumerable<TService>>(versions.Values.ToList());
                }
            }
            
            return Task.FromResult<IEnumerable<TService>>(Enumerable.Empty<TService>());
        }
        
        /// <summary>
        /// Gets all registered services.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing all registered services.</returns>
        public virtual Task<IEnumerable<TService>> GetAllServicesAsync(CancellationToken cancellationToken = default)
        {
            lock (SyncRoot)
            {
                var services = new List<TService>();
                
                foreach (var versions in RegisteredServices.Values)
                {
                    services.AddRange(versions.Values);
                }
                
                return Task.FromResult<IEnumerable<TService>>(services);
            }
        }
        
        /// <summary>
        /// Unregisters a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing a value indicating whether the service was unregistered.</returns>
        public virtual async Task<bool> UnregisterServiceAsync(string serviceId, string version, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serviceId))
            {
                throw new ArgumentException("Service ID cannot be null or empty.", nameof(serviceId));
            }
            
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Version cannot be null or empty.", nameof(version));
            }
            
            Logger.LogInformation("Unregistering service {ServiceId} (version {Version})...", serviceId, version);
            
            var result = await OnUnregisterAsync(serviceId, version, cancellationToken);
            
            if (result)
            {
                Logger.LogInformation("Service {ServiceId} (version {Version}) unregistered successfully.", serviceId, version);
            }
            else
            {
                Logger.LogWarning("Failed to unregister service {ServiceId} (version {Version}).", serviceId, version);
            }
            
            return result;
        }
        
        /// <summary>
        /// Gets the status of a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the service status.</returns>
        public virtual Task<ServiceStatusInfo> GetServiceStatusAsync(string serviceId, string version, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serviceId))
            {
                throw new ArgumentException("Service ID cannot be null or empty.", nameof(serviceId));
            }
            
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Version cannot be null or empty.", nameof(version));
            }
            
            lock (SyncRoot)
            {
                if (ServiceStatuses.TryGetValue(serviceId, out var versions) && versions.TryGetValue(version, out var status))
                {
                    return Task.FromResult(status);
                }
            }
            
            return Task.FromResult<ServiceStatusInfo>(null);
        }
        
        /// <summary>
        /// Updates the status of a service.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="status">The new service status.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual Task UpdateServiceStatusAsync(string serviceId, string version, ServiceStatus status, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(serviceId))
            {
                throw new ArgumentException("Service ID cannot be null or empty.", nameof(serviceId));
            }
            
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException("Version cannot be null or empty.", nameof(version));
            }
            
            Logger.LogInformation("Updating status of service {ServiceId} (version {Version}) to {Status}...", serviceId, version, status);
            
            lock (SyncRoot)
            {
                if (!ServiceStatuses.TryGetValue(serviceId, out var versions))
                {
                    versions = new Dictionary<string, ServiceStatusInfo>();
                    ServiceStatuses[serviceId] = versions;
                }
                
                versions[version] = new ServiceStatusInfo(status);
            }
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Determines whether this consumer can handle the specified message.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns><c>true</c> if this consumer can handle the message; otherwise, <c>false</c>.</returns>
        public virtual bool CanConsume(ServiceRegistrationCommand<TService> message)
        {
            return message != null && Status == ServiceStatus.Running;
        }
        
        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message to consume.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task ConsumeAsync(ServiceRegistrationCommand<TService> message, CancellationToken cancellationToken = default)
        {
            if (!CanConsume(message))
            {
                Logger.LogWarning("Cannot consume service registration command. Current status: {Status}", Status);
                return;
            }
            
            try
            {
                Logger.LogInformation("Consuming service registration command for service {ServiceId}...", message.Service?.Id);
                
                var result = await RegisterAsync(message.Service, cancellationToken);
                
                await OnRegistrationCompletedAsync(message, result, cancellationToken);
                
                Logger.LogInformation("Service registration command for service {ServiceId} processed successfully.", message.Service?.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error consuming service registration command for service {ServiceId}: {ErrorMessage}", 
                    message.Service?.Id, ex.Message);
                
                await OnRegistrationFailedAsync(message, ex, cancellationToken);
            }
        }
        
        /// <summary>
        /// Called when a service is being registered.
        /// </summary>
        /// <param name="service">The service to register.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the registration.</returns>
        protected virtual Task<RegistrationResult> OnRegisterAsync(TService service, CancellationToken cancellationToken)
        {
            lock (SyncRoot)
            {
                if (!RegisteredServices.TryGetValue(service.Id, out var versions))
                {
                    versions = new Dictionary<string, TService>();
                    RegisteredServices[service.Id] = versions;
                }
                
                versions[service.Name] = service;
                
                if (!ServiceStatuses.TryGetValue(service.Id, out var statusVersions))
                {
                    statusVersions = new Dictionary<string, ServiceStatusInfo>();
                    ServiceStatuses[service.Id] = statusVersions;
                }
                
                statusVersions[service.Name] = service.GetStatusInfo();
            }
            
            return Task.FromResult(RegistrationResult.Success(service.Id, service.Name));
        }
        
        /// <summary>
        /// Called when a service is being unregistered.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing a value indicating whether the service was unregistered.</returns>
        protected virtual Task<bool> OnUnregisterAsync(string serviceId, string version, CancellationToken cancellationToken)
        {
            bool result = false;
            
            lock (SyncRoot)
            {
                if (RegisteredServices.TryGetValue(serviceId, out var versions))
                {
                    result = versions.Remove(version);
                    
                    if (versions.Count == 0)
                    {
                        RegisteredServices.Remove(serviceId);
                    }
                }
                
                if (ServiceStatuses.TryGetValue(serviceId, out var statusVersions))
                {
                    statusVersions.Remove(version);
                    
                    if (statusVersions.Count == 0)
                    {
                        ServiceStatuses.Remove(serviceId);
                    }
                }
            }
            
            return Task.FromResult(result);
        }
        
        /// <summary>
        /// Called when a service registration has completed successfully.
        /// </summary>
        /// <param name="command">The service registration command.</param>
        /// <param name="result">The registration result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnRegistrationCompletedAsync(ServiceRegistrationCommand<TService> command, RegistrationResult result, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called when a service registration has failed.
        /// </summary>
        /// <param name="command">The service registration command.</param>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnRegistrationFailedAsync(ServiceRegistrationCommand<TService> command, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
    
    /// <summary>
    /// Represents a command to register a service.
    /// </summary>
    /// <typeparam name="TService">The type of service to register.</typeparam>
    public class ServiceRegistrationCommand<TService> where TService : IService
    {
        /// <summary>
        /// Gets the service to register.
        /// </summary>
        public TService Service { get; }
        
        /// <summary>
        /// Gets the correlation identifier for tracking related operations.
        /// </summary>
        public string CorrelationId { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationCommand{TService}"/> class.
        /// </summary>
        /// <param name="service">The service to register.</param>
        /// <param name="correlationId">The correlation identifier for tracking related operations.</param>
        public ServiceRegistrationCommand(TService service, string correlationId = null)
        {
            Service = service;
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
        }
    }
}
