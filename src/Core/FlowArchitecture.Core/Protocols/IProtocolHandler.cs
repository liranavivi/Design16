using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;

namespace FlowArchitecture.Core.Protocols
{
    /// <summary>
    /// Interface for protocol handlers in the Flow Architecture system.
    /// </summary>
    public interface IProtocolHandler
    {
        /// <summary>
        /// Gets the unique identifier for the protocol handler.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Gets the name of the protocol handler.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the protocols supported by this handler.
        /// </summary>
        IReadOnlyList<string> SupportedProtocols { get; }
        
        /// <summary>
        /// Determines whether this handler can handle the specified protocol.
        /// </summary>
        /// <param name="protocolId">The identifier of the protocol to check.</param>
        /// <returns><c>true</c> if this handler can handle the protocol; otherwise, <c>false</c>.</returns>
        bool CanHandle(string protocolId);
        
        /// <summary>
        /// Handles the specified protocol with the provided context.
        /// </summary>
        /// <param name="protocol">The protocol to handle.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the execution.</returns>
        Task<Result<object>> HandleAsync(IProtocol protocol, ProtocolExecutionContext context, CancellationToken cancellationToken = default);
    }
}
