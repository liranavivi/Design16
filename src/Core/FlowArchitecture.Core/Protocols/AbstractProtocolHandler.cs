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
    /// Base implementation for protocol handlers in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractProtocolHandler : IProtocolHandler
    {
        /// <summary>
        /// Gets the logger for this protocol handler.
        /// </summary>
        protected ILogger Logger { get; }
        
        /// <summary>
        /// Gets the unique identifier for the protocol handler.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Gets the name of the protocol handler.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the protocols supported by this handler.
        /// </summary>
        public abstract IReadOnlyList<string> SupportedProtocols { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractProtocolHandler"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the protocol handler.</param>
        /// <param name="name">The name of the protocol handler.</param>
        /// <param name="logger">The logger for this protocol handler.</param>
        protected AbstractProtocolHandler(string id, string name, ILogger logger)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Determines whether this handler can handle the specified protocol.
        /// </summary>
        /// <param name="protocolId">The identifier of the protocol to check.</param>
        /// <returns><c>true</c> if this handler can handle the protocol; otherwise, <c>false</c>.</returns>
        public virtual bool CanHandle(string protocolId)
        {
            return !string.IsNullOrEmpty(protocolId) && SupportedProtocols.Contains(protocolId);
        }
        
        /// <summary>
        /// Handles the specified protocol with the provided context.
        /// </summary>
        /// <param name="protocol">The protocol to handle.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the execution.</returns>
        public virtual async Task<Result<object>> HandleAsync(IProtocol protocol, ProtocolExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (protocol == null)
                {
                    return Result<object>.Failure("HANDLER_ERROR", "Protocol cannot be null.");
                }
                
                if (context == null)
                {
                    return Result<object>.Failure("HANDLER_ERROR", "Execution context cannot be null.");
                }
                
                if (!CanHandle(protocol.Id))
                {
                    return Result<object>.Failure("HANDLER_ERROR", $"Handler {Id} does not support protocol {protocol.Id}.");
                }
                
                Logger.LogInformation("Handling protocol {ProtocolId} ({ProtocolName}) with handler {HandlerId} ({HandlerName}) and context {ContextId}...", 
                    protocol.Id, protocol.Name, Id, Name, context.Id);
                
                var result = await OnHandleAsync(protocol, context, cancellationToken);
                
                if (result.IsSuccess)
                {
                    Logger.LogInformation("Protocol {ProtocolId} ({ProtocolName}) handled successfully with handler {HandlerId} ({HandlerName}) and context {ContextId}.", 
                        protocol.Id, protocol.Name, Id, Name, context.Id);
                }
                else
                {
                    Logger.LogWarning("Protocol {ProtocolId} ({ProtocolName}) handling failed with handler {HandlerId} ({HandlerName}) and context {ContextId}: {ErrorMessage}", 
                        protocol.Id, protocol.Name, Id, Name, context.Id, string.Join(", ", result.Errors.Select(e => e.Message)));
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling protocol {ProtocolId} with handler {HandlerId} ({HandlerName}) and context {ContextId}: {ErrorMessage}", 
                    protocol?.Id, Id, Name, context?.Id, ex.Message);
                return Result<object>.Failure("HANDLER_ERROR", ex.Message);
            }
        }
        
        /// <summary>
        /// Called when the protocol is being handled.
        /// </summary>
        /// <param name="protocol">The protocol to handle.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the execution.</returns>
        protected abstract Task<Result<object>> OnHandleAsync(IProtocol protocol, ProtocolExecutionContext context, CancellationToken cancellationToken);
    }
}
