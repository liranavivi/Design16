using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;
using Microsoft.Extensions.Logging;

namespace FlowArchitecture.Protocols.Abstractions
{
    /// <summary>
    /// Base implementation for database protocols in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractDatabaseProtocol : AbstractProtocol, IDatabaseProtocol
    {
        private const string ConnectionStringParameterName = "ConnectionString";
        private const string DatabaseTypeParameterName = "DatabaseType";
        private const string TimeoutParameterName = "Timeout";

        /// <summary>
        /// Gets the connection string for the database.
        /// </summary>
        public string ConnectionString { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the database type.
        /// </summary>
        public string DatabaseType { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the timeout for database operations.
        /// </summary>
        protected TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets a value indicating whether the database connection is open.
        /// </summary>
        public abstract bool IsConnected { get; }

        /// <summary>
        /// Gets the parameters required by the protocol.
        /// </summary>
        public override IReadOnlyList<ProtocolParameter> Parameters => new List<ProtocolParameter>
        {
            new ProtocolParameter(
                ConnectionStringParameterName,
                typeof(string),
                true,
                "Connection String",
                "The connection string for the database."),

            new ProtocolParameter(
                DatabaseTypeParameterName,
                typeof(string),
                true,
                "Database Type",
                "The type of the database (e.g., SQL Server, MySQL, MongoDB)."),

            new ProtocolParameter(
                TimeoutParameterName,
                typeof(int),
                false,
                "Timeout",
                "The timeout for database operations in seconds.",
                30)
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDatabaseProtocol"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the protocol.</param>
        /// <param name="name">The name of the protocol.</param>
        /// <param name="version">The version of the protocol.</param>
        /// <param name="description">The description of the protocol.</param>
        /// <param name="logger">The logger for this protocol.</param>
        protected AbstractDatabaseProtocol(string id, string name, string version, string description, ILogger logger)
            : base(id, name, version, description, logger)
        {
        }

        /// <summary>
        /// Called when the protocol is being initialized.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override Task<Result<bool>> OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (ProtocolParameters.TryGetValue(ConnectionStringParameterName, out var connectionStringObj) && connectionStringObj is string connectionString)
            {
                ConnectionString = connectionString;
            }

            if (ProtocolParameters.TryGetValue(DatabaseTypeParameterName, out var databaseTypeObj) && databaseTypeObj is string databaseType)
            {
                DatabaseType = databaseType;
            }

            if (ProtocolParameters.TryGetValue(TimeoutParameterName, out var timeoutObj) && timeoutObj is int timeoutSeconds)
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            }

            return base.OnInitializeAsync(cancellationToken);
        }

        /// <summary>
        /// Called when the protocol is being executed.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the execution.</returns>
        protected override async Task<Result<object>> OnExecuteAsync(ProtocolExecutionContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                return Result<object>.Failure("DB_PROTOCOL_ERROR", "Connection string is not set.");
            }

            if (!IsConnected)
            {
                var connectResult = await ConnectAsync(context, cancellationToken);
                if (!connectResult.IsSuccess)
                {
                    return Result<object>.Failure(connectResult.Errors.ToArray());
                }
            }

            try
            {
                // The specific database operation to perform should be determined by the context
                if (context.Parameters.TryGetValue("Operation", out var operationObj) && operationObj is string operation)
                {
                    switch (operation.ToUpperInvariant())
                    {
                        case "QUERY":
                            return await ExecuteQueryAsync(
                                GetQueryFromContext(context),
                                GetParametersFromContext(context),
                                context,
                                cancellationToken);

                        case "NONQUERY":
                            var nonQueryResult = await ExecuteNonQueryAsync(
                                GetQueryFromContext(context),
                                GetParametersFromContext(context),
                                context,
                                cancellationToken);

                            return nonQueryResult.IsSuccess
                                ? Result<object>.Success(nonQueryResult.Value)
                                : Result<object>.Failure(nonQueryResult.Errors.ToArray());

                        case "SCALAR":
                            return await ExecuteScalarAsync(
                                GetQueryFromContext(context),
                                GetParametersFromContext(context),
                                context,
                                cancellationToken);

                        default:
                            return Result<object>.Failure("DB_PROTOCOL_ERROR", $"Unsupported database operation: {operation}");
                    }
                }

                // Default to QUERY if no operation is specified
                return await ExecuteQueryAsync(
                    GetQueryFromContext(context),
                    GetParametersFromContext(context),
                    context,
                    cancellationToken);
            }
            finally
            {
                // Disconnect if the connection was opened for this execution
                if (context.Parameters.TryGetValue("KeepConnectionOpen", out var keepOpenObj) &&
                    keepOpenObj is bool keepOpen && !keepOpen)
                {
                    await DisconnectAsync(context, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Gets the query from the execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The query.</returns>
        protected virtual string GetQueryFromContext(ProtocolExecutionContext context)
        {
            return context.Parameters.TryGetValue("Query", out var queryObj) && queryObj is string query
                ? query
                : string.Empty;
        }

        /// <summary>
        /// Gets the parameters from the execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The parameters.</returns>
        protected virtual IDictionary<string, object>? GetParametersFromContext(ProtocolExecutionContext context)
        {
            return context.Parameters.TryGetValue("Parameters", out var parametersObj) && parametersObj is IDictionary<string, object> parameters
                ? parameters
                : null;
        }

        /// <summary>
        /// Opens a connection to the database.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the connection operation.</returns>
        public abstract Task<Result<bool>> ConnectAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the disconnection operation.</returns>
        public abstract Task<Result<bool>> DisconnectAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a query against the database.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="parameters">The parameters for the query.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the query execution.</returns>
        public abstract Task<Result<object>> ExecuteQueryAsync(
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
        public abstract Task<Result<int>> ExecuteNonQueryAsync(
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
        public abstract Task<Result<object>> ExecuteScalarAsync(
            string command,
            IDictionary<string, object>? parameters,
            ProtocolExecutionContext context,
            CancellationToken cancellationToken = default);
    }
}
