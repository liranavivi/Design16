using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;

namespace FlowArchitecture.Protocols.Abstractions
{
    /// <summary>
    /// Interface for database protocols in the Flow Architecture system.
    /// </summary>
    public interface IDatabaseProtocol : IProtocol
    {
        /// <summary>
        /// Gets the connection string for the database.
        /// </summary>
        string ConnectionString { get; }
        
        /// <summary>
        /// Gets the database type.
        /// </summary>
        string DatabaseType { get; }
        
        /// <summary>
        /// Gets a value indicating whether the database connection is open.
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// Opens a connection to the database.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the connection operation.</returns>
        Task<Result<bool>> ConnectAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the disconnection operation.</returns>
        Task<Result<bool>> DisconnectAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Executes a query against the database.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="parameters">The parameters for the query.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the query execution.</returns>
        Task<Result<object>> ExecuteQueryAsync(
            string query, 
            IDictionary<string, object>? parameters, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Executes a non-query command against the database.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">The parameters for the command.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the command execution.</returns>
        Task<Result<int>> ExecuteNonQueryAsync(
            string command, 
            IDictionary<string, object>? parameters, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Executes a scalar command against the database.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">The parameters for the command.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the scalar command execution.</returns>
        Task<Result<object>> ExecuteScalarAsync(
            string command, 
            IDictionary<string, object>? parameters, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default);
    }
}
