using System;
using System.Collections.Generic;

namespace FlowArchitecture.Core.Protocols
{
    /// <summary>
    /// Represents the execution context for a protocol.
    /// </summary>
    public class ProtocolExecutionContext
    {
        /// <summary>
        /// Gets the unique identifier for the execution context.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Gets the input data for the protocol execution.
        /// </summary>
        public object InputData { get; }
        
        /// <summary>
        /// Gets the parameters for the protocol execution.
        /// </summary>
        public IDictionary<string, object> Parameters { get; }
        
        /// <summary>
        /// Gets the timestamp when the execution started.
        /// </summary>
        public DateTime StartTime { get; }
        
        /// <summary>
        /// Gets or sets the timestamp when the execution completed.
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// Gets or sets the execution state.
        /// </summary>
        public IDictionary<string, object> State { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolExecutionContext"/> class.
        /// </summary>
        /// <param name="inputData">The input data for the protocol execution.</param>
        /// <param name="parameters">The parameters for the protocol execution.</param>
        public ProtocolExecutionContext(object inputData, IDictionary<string, object> parameters)
        {
            Id = Guid.NewGuid().ToString();
            InputData = inputData;
            Parameters = parameters ?? new Dictionary<string, object>();
            StartTime = DateTime.UtcNow;
            State = new Dictionary<string, object>();
        }
    }
}
