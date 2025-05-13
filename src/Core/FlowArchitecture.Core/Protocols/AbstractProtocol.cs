using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using Microsoft.Extensions.Logging;

namespace FlowArchitecture.Core.Protocols
{
    /// <summary>
    /// Base implementation for all protocols in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractProtocol : IProtocol
    {
        /// <summary>
        /// Gets the logger for this protocol.
        /// </summary>
        protected ILogger Logger { get; }
        
        /// <summary>
        /// Gets the unique identifier for the protocol.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Gets the name of the protocol.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the version of the protocol.
        /// </summary>
        public string Version { get; }
        
        /// <summary>
        /// Gets the description of the protocol.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Gets the parameters required by the protocol.
        /// </summary>
        public abstract IReadOnlyList<ProtocolParameter> Parameters { get; }
        
        /// <summary>
        /// Gets the parameters for the protocol.
        /// </summary>
        protected IDictionary<string, object> ProtocolParameters { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol is initialized.
        /// </summary>
        protected bool IsInitialized { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractProtocol"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the protocol.</param>
        /// <param name="name">The name of the protocol.</param>
        /// <param name="version">The version of the protocol.</param>
        /// <param name="description">The description of the protocol.</param>
        /// <param name="logger">The logger for this protocol.</param>
        protected AbstractProtocol(string id, string name, string version, string description, ILogger logger)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            Description = description;
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ProtocolParameters = new Dictionary<string, object>();
            IsInitialized = false;
        }
        
        /// <summary>
        /// Initializes the protocol with the provided parameters.
        /// </summary>
        /// <param name="parameters">The parameters for the protocol.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task<Result<bool>> InitializeAsync(IDictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                Logger.LogInformation("Initializing protocol {ProtocolId} ({ProtocolName})...", Id, Name);
                
                if (parameters == null)
                {
                    return Result<bool>.Failure("PROTOCOL_INIT_ERROR", "Parameters cannot be null.");
                }
                
                var validationResult = ValidateParameters(parameters);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => 
                        new Error("PROTOCOL_PARAM_ERROR", $"{e.PropertyName}: {e.ErrorMessage}")).ToArray();
                    return Result<bool>.Failure(errors);
                }
                
                ProtocolParameters = new Dictionary<string, object>(parameters);
                
                var result = await OnInitializeAsync(cancellationToken);
                if (!result.IsSuccess)
                {
                    return result;
                }
                
                IsInitialized = true;
                
                Logger.LogInformation("Protocol {ProtocolId} ({ProtocolName}) initialized successfully.", Id, Name);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error initializing protocol {ProtocolId} ({ProtocolName}): {ErrorMessage}", Id, Name, ex.Message);
                return Result<bool>.Failure("PROTOCOL_INIT_ERROR", ex.Message);
            }
        }
        
        /// <summary>
        /// Executes the protocol with the provided context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the execution.</returns>
        public virtual async Task<Result<object>> ExecuteAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!IsInitialized)
                {
                    return Result<object>.Failure("PROTOCOL_NOT_INITIALIZED", "Protocol is not initialized.");
                }
                
                if (context == null)
                {
                    return Result<object>.Failure("PROTOCOL_EXEC_ERROR", "Execution context cannot be null.");
                }
                
                Logger.LogInformation("Executing protocol {ProtocolId} ({ProtocolName}) with context {ContextId}...", Id, Name, context.Id);
                
                var result = await OnExecuteAsync(context, cancellationToken);
                
                context.EndTime = DateTime.UtcNow;
                
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Protocol {ProtocolId} ({ProtocolName}) executed successfully with context {ContextId}.", Id, Name, context.Id);
                }
                else
                {
                    Logger.LogWarning("Protocol {ProtocolId} ({ProtocolName}) execution failed with context {ContextId}: {ErrorMessage}", 
                        Id, Name, context.Id, string.Join(", ", result.Errors.Select(e => e.Message)));
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing protocol {ProtocolId} ({ProtocolName}) with context {ContextId}: {ErrorMessage}", 
                    Id, Name, context?.Id, ex.Message);
                return Result<object>.Failure("PROTOCOL_EXEC_ERROR", ex.Message);
            }
        }
        
        /// <summary>
        /// Validates the protocol parameters.
        /// </summary>
        /// <param name="parameters">The parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        public virtual ValidationResult ValidateParameters(IDictionary<string, object> parameters)
        {
            var errors = new List<ValidationError>();
            
            if (parameters == null)
            {
                return ValidationResult.Failure("parameters", "Parameters cannot be null.");
            }
            
            foreach (var parameter in Parameters.Where(p => p.IsRequired))
            {
                if (!parameters.ContainsKey(parameter.Name) || parameters[parameter.Name] == null)
                {
                    errors.Add(new ValidationError(parameter.Name, $"Parameter '{parameter.Name}' is required."));
                }
            }
            
            return new ValidationResult(errors);
        }
        
        /// <summary>
        /// Called when the protocol is being initialized.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task<Result<bool>> OnInitializeAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<bool>.Success(true));
        }
        
        /// <summary>
        /// Called when the protocol is being executed.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the execution.</returns>
        protected abstract Task<Result<object>> OnExecuteAsync(ProtocolExecutionContext context, CancellationToken cancellationToken);
    }
}
