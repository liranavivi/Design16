using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;

namespace FlowArchitecture.Core.Protocols
{
    /// <summary>
    /// Base interface for all protocols in the Flow Architecture system.
    /// </summary>
    public interface IProtocol
    {
        /// <summary>
        /// Gets the unique identifier for the protocol.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Gets the name of the protocol.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets the version of the protocol.
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// Gets the description of the protocol.
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Gets the parameters required by the protocol.
        /// </summary>
        IReadOnlyList<ProtocolParameter> Parameters { get; }
        
        /// <summary>
        /// Initializes the protocol with the provided parameters.
        /// </summary>
        /// <param name="parameters">The parameters for the protocol.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<Result<bool>> InitializeAsync(IDictionary<string, object> parameters, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Executes the protocol with the provided context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the execution.</returns>
        Task<Result<object>> ExecuteAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Validates the protocol parameters.
        /// </summary>
        /// <param name="parameters">The parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        ValidationResult ValidateParameters(IDictionary<string, object> parameters);
    }
}
