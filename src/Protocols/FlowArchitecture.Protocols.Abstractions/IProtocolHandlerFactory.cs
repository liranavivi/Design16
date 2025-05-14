using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;

namespace FlowArchitecture.Protocols.Abstractions
{
    /// <summary>
    /// Interface for protocol handler factories in the Flow Architecture system.
    /// </summary>
    public interface IProtocolHandlerFactory
    {
        /// <summary>
        /// Gets all registered protocol handlers.
        /// </summary>
        /// <returns>A collection of protocol handlers.</returns>
        IEnumerable<IProtocolHandler> GetAllHandlers();
        
        /// <summary>
        /// Gets a protocol handler for the specified protocol.
        /// </summary>
        /// <param name="protocolId">The identifier of the protocol.</param>
        /// <returns>The protocol handler, or <c>null</c> if no handler is found.</returns>
        IProtocolHandler? GetHandler(string protocolId);
        
        /// <summary>
        /// Creates a protocol instance for the specified protocol.
        /// </summary>
        /// <param name="protocolId">The identifier of the protocol.</param>
        /// <param name="parameters">The parameters for the protocol.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the protocol instance.</returns>
        Task<Result<IProtocol>> CreateProtocolAsync(string protocolId, IDictionary<string, object> parameters, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Registers a protocol handler.
        /// </summary>
        /// <param name="handler">The protocol handler to register.</param>
        /// <returns>A result indicating whether the registration was successful.</returns>
        Result<bool> RegisterHandler(IProtocolHandler handler);
        
        /// <summary>
        /// Unregisters a protocol handler.
        /// </summary>
        /// <param name="handlerId">The identifier of the protocol handler to unregister.</param>
        /// <returns>A result indicating whether the unregistration was successful.</returns>
        Result<bool> UnregisterHandler(string handlerId);
    }
}
