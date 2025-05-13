using System;
using System.Collections.Generic;
using System.Threading;

namespace FlowArchitecture.Common.Services
{
    /// <summary>
    /// Represents the context for executing an operation.
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// Gets the unique identifier for the execution context.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Gets the timestamp when the execution started.
        /// </summary>
        public DateTime StartTime { get; }
        
        /// <summary>
        /// Gets or sets the timestamp when the execution completed.
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// Gets the correlation identifier for tracking related operations.
        /// </summary>
        public string CorrelationId { get; }
        
        /// <summary>
        /// Gets the cancellation token for the operation.
        /// </summary>
        public CancellationToken CancellationToken { get; }
        
        /// <summary>
        /// Gets the execution state.
        /// </summary>
        public IDictionary<string, object> State { get; }
        
        /// <summary>
        /// Gets the execution parameters.
        /// </summary>
        public IDictionary<string, object> Parameters { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionContext"/> class.
        /// </summary>
        /// <param name="correlationId">The correlation identifier for tracking related operations.</param>
        /// <param name="cancellationToken">The cancellation token for the operation.</param>
        /// <param name="parameters">The execution parameters.</param>
        public ExecutionContext(
            string correlationId = null,
            CancellationToken cancellationToken = default,
            IDictionary<string, object> parameters = null)
        {
            Id = Guid.NewGuid().ToString();
            StartTime = DateTime.UtcNow;
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            CancellationToken = cancellationToken;
            State = new Dictionary<string, object>();
            Parameters = parameters ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Gets a value from the execution state.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key of the value.</param>
        /// <param name="defaultValue">The default value to return if the key is not found.</param>
        /// <returns>The value, or the default value if the key is not found.</returns>
        public T GetState<T>(string key, T defaultValue = default)
        {
            if (State.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            
            return defaultValue;
        }
        
        /// <summary>
        /// Sets a value in the execution state.
        /// </summary>
        /// <param name="key">The key of the value.</param>
        /// <param name="value">The value to set.</param>
        public void SetState(string key, object value)
        {
            State[key] = value;
        }
        
        /// <summary>
        /// Gets a parameter value.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="key">The key of the parameter.</param>
        /// <param name="defaultValue">The default value to return if the parameter is not found.</param>
        /// <returns>The parameter value, or the default value if the parameter is not found.</returns>
        public T GetParameter<T>(string key, T defaultValue = default)
        {
            if (Parameters.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            
            return defaultValue;
        }
    }
}
