using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FlowArchitecture.Core.Services
{
    /// <summary>
    /// Base implementation for all services in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractServiceBase : IService
    {
        /// <summary>
        /// Gets the logger for this service.
        /// </summary>
        protected ILogger Logger { get; }
        
        /// <summary>
        /// Gets the unique identifier for the service.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Gets the display name of the service.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the description of the service.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Gets the current status of the service.
        /// </summary>
        public ServiceStatus Status { get; protected set; }
        
        /// <summary>
        /// Gets the configuration for the service.
        /// </summary>
        protected IServiceConfiguration Configuration { get; private set; }
        
        /// <summary>
        /// Gets the timestamp when the service was initialized.
        /// </summary>
        protected DateTime? InitializedAt { get; private set; }
        
        /// <summary>
        /// Gets the timestamp when the service was started.
        /// </summary>
        protected DateTime? StartedAt { get; private set; }
        
        /// <summary>
        /// Gets the timestamp when the service was stopped.
        /// </summary>
        protected DateTime? StoppedAt { get; private set; }
        
        /// <summary>
        /// Gets the error message if the service is in an error state.
        /// </summary>
        protected string ErrorMessage { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractServiceBase"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        protected AbstractServiceBase(string id, string name, string description, ILogger logger)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Status = ServiceStatus.NotInitialized;
        }
        
        /// <summary>
        /// Initializes the service with the provided configuration.
        /// </summary>
        /// <param name="configuration">The configuration parameters for the service.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task InitializeAsync(IServiceConfiguration configuration, CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogInformation("Initializing service {ServiceId} ({ServiceName})...", Id, Name);
                
                Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
                Status = ServiceStatus.Initialized;
                InitializedAt = DateTime.UtcNow;
                
                await OnInitializeAsync(cancellationToken);
                
                Logger.LogInformation("Service {ServiceId} ({ServiceName}) initialized successfully.", Id, Name);
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Error;
                ErrorMessage = ex.Message;
                Logger.LogError(ex, "Error initializing service {ServiceId} ({ServiceName}): {ErrorMessage}", Id, Name, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (Status != ServiceStatus.Initialized && Status != ServiceStatus.Stopped)
                {
                    throw new InvalidOperationException($"Cannot start service in {Status} state.");
                }
                
                Logger.LogInformation("Starting service {ServiceId} ({ServiceName})...", Id, Name);
                
                Status = ServiceStatus.Starting;
                
                await OnStartAsync(cancellationToken);
                
                Status = ServiceStatus.Running;
                StartedAt = DateTime.UtcNow;
                StoppedAt = null;
                
                Logger.LogInformation("Service {ServiceId} ({ServiceName}) started successfully.", Id, Name);
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Error;
                ErrorMessage = ex.Message;
                Logger.LogError(ex, "Error starting service {ServiceId} ({ServiceName}): {ErrorMessage}", Id, Name, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task StopAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (Status != ServiceStatus.Running)
                {
                    throw new InvalidOperationException($"Cannot stop service in {Status} state.");
                }
                
                Logger.LogInformation("Stopping service {ServiceId} ({ServiceName})...", Id, Name);
                
                Status = ServiceStatus.Stopping;
                
                await OnStopAsync(cancellationToken);
                
                Status = ServiceStatus.Stopped;
                StoppedAt = DateTime.UtcNow;
                
                Logger.LogInformation("Service {ServiceId} ({ServiceName}) stopped successfully.", Id, Name);
            }
            catch (Exception ex)
            {
                Status = ServiceStatus.Error;
                ErrorMessage = ex.Message;
                Logger.LogError(ex, "Error stopping service {ServiceId} ({ServiceName}): {ErrorMessage}", Id, Name, ex.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Gets detailed status information about the service.
        /// </summary>
        /// <returns>A <see cref="ServiceStatusInfo"/> object containing detailed status information.</returns>
        public virtual ServiceStatusInfo GetStatusInfo()
        {
            var properties = new Dictionary<string, object>
            {
                { "InitializedAt", InitializedAt },
                { "StartedAt", StartedAt },
                { "StoppedAt", StoppedAt }
            };
            
            return new ServiceStatusInfo(Status, ErrorMessage, properties);
        }
        
        /// <summary>
        /// Called when the service is being initialized.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called when the service is being started.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnStartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called when the service is being stopped.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnStopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
