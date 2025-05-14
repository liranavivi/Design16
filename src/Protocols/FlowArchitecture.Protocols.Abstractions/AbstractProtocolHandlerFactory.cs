using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;
using Microsoft.Extensions.Logging;

namespace FlowArchitecture.Protocols.Abstractions
{
    /// <summary>
    /// Base implementation for protocol handler factories in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractProtocolHandlerFactory : IProtocolHandlerFactory
    {
        private readonly ConcurrentDictionary<string, IProtocolHandler> _handlers = new();
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractProtocolHandlerFactory"/> class.
        /// </summary>
        /// <param name="logger">The logger for this factory.</param>
        protected AbstractProtocolHandlerFactory(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all registered protocol handlers.
        /// </summary>
        /// <returns>A collection of protocol handlers.</returns>
        public IEnumerable<IProtocolHandler> GetAllHandlers()
        {
            return _handlers.Values;
        }

        /// <summary>
        /// Gets a protocol handler for the specified protocol.
        /// </summary>
        /// <param name="protocolId">The identifier of the protocol.</param>
        /// <returns>The protocol handler, or <c>null</c> if no handler is found.</returns>
        public IProtocolHandler? GetHandler(string protocolId)
        {
            if (string.IsNullOrEmpty(protocolId))
            {
                return null;
            }

            return _handlers.Values.FirstOrDefault(h => h.CanHandle(protocolId));
        }

        /// <summary>
        /// Creates a protocol instance for the specified protocol.
        /// </summary>
        /// <param name="protocolId">The identifier of the protocol.</param>
        /// <param name="parameters">The parameters for the protocol.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the protocol instance.</returns>
        public virtual async Task<Result<IProtocol>> CreateProtocolAsync(string protocolId, IDictionary<string, object> parameters, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating protocol instance for protocol {ProtocolId}...", protocolId);

                if (string.IsNullOrEmpty(protocolId))
                {
                    return Result<IProtocol>.Failure("PROTOCOL_FACTORY_ERROR", "Protocol ID cannot be null or empty.");
                }

                if (parameters == null)
                {
                    return Result<IProtocol>.Failure("PROTOCOL_FACTORY_ERROR", "Parameters cannot be null.");
                }

                var protocol = await OnCreateProtocolAsync(protocolId, parameters, cancellationToken);
                if (protocol == null)
                {
                    return Result<IProtocol>.Failure("PROTOCOL_FACTORY_ERROR", $"Failed to create protocol instance for protocol {protocolId}.");
                }

                var initResult = await protocol.InitializeAsync(parameters, cancellationToken);
                if (!initResult.IsSuccess)
                {
                    return Result<IProtocol>.Failure(initResult.Errors.ToArray());
                }

                _logger.LogInformation("Protocol instance for protocol {ProtocolId} created successfully.", protocolId);

                return Result<IProtocol>.Success(protocol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating protocol instance for protocol {ProtocolId}: {ErrorMessage}", protocolId, ex.Message);
                return Result<IProtocol>.Failure("PROTOCOL_FACTORY_ERROR", ex.Message);
            }
        }

        /// <summary>
        /// Registers a protocol handler.
        /// </summary>
        /// <param name="handler">The protocol handler to register.</param>
        /// <returns>A result indicating whether the registration was successful.</returns>
        public virtual Result<bool> RegisterHandler(IProtocolHandler handler)
        {
            try
            {
                if (handler == null)
                {
                    return Result<bool>.Failure("HANDLER_REGISTER_ERROR", "Handler cannot be null.");
                }

                _logger.LogInformation("Registering protocol handler {HandlerId} ({HandlerName})...", handler.Id, handler.Name);

                if (_handlers.TryGetValue(handler.Id, out _))
                {
                    return Result<bool>.Failure("HANDLER_REGISTER_ERROR", $"Handler with ID {handler.Id} is already registered.");
                }

                if (_handlers.TryAdd(handler.Id, handler))
                {
                    _logger.LogInformation("Protocol handler {HandlerId} ({HandlerName}) registered successfully.", handler.Id, handler.Name);
                    return Result<bool>.Success(true);
                }

                return Result<bool>.Failure("HANDLER_REGISTER_ERROR", $"Failed to register handler with ID {handler.Id}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering protocol handler {HandlerId}: {ErrorMessage}", handler?.Id, ex.Message);
                return Result<bool>.Failure("HANDLER_REGISTER_ERROR", ex.Message);
            }
        }

        /// <summary>
        /// Unregisters a protocol handler.
        /// </summary>
        /// <param name="handlerId">The identifier of the protocol handler to unregister.</param>
        /// <returns>A result indicating whether the unregistration was successful.</returns>
        public virtual Result<bool> UnregisterHandler(string handlerId)
        {
            try
            {
                if (string.IsNullOrEmpty(handlerId))
                {
                    return Result<bool>.Failure("HANDLER_UNREGISTER_ERROR", "Handler ID cannot be null or empty.");
                }

                _logger.LogInformation("Unregistering protocol handler {HandlerId}...", handlerId);

                if (_handlers.TryRemove(handlerId, out var handler))
                {
                    _logger.LogInformation("Protocol handler {HandlerId} ({HandlerName}) unregistered successfully.", handlerId, handler.Name);
                    return Result<bool>.Success(true);
                }

                return Result<bool>.Failure("HANDLER_UNREGISTER_ERROR", $"Handler with ID {handlerId} is not registered.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering protocol handler {HandlerId}: {ErrorMessage}", handlerId, ex.Message);
                return Result<bool>.Failure("HANDLER_UNREGISTER_ERROR", ex.Message);
            }
        }

        /// <summary>
        /// Called when a protocol instance is being created.
        /// </summary>
        /// <param name="protocolId">The identifier of the protocol.</param>
        /// <param name="parameters">The parameters for the protocol.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the protocol instance.</returns>
        protected abstract Task<IProtocol?> OnCreateProtocolAsync(string protocolId, IDictionary<string, object> parameters, CancellationToken cancellationToken);
    }
}
