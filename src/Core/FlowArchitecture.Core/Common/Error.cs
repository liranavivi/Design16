using System;
using System.Collections.Generic;

namespace FlowArchitecture.Core.Common
{
    /// <summary>
    /// Represents an error that occurred during an operation.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public string Code { get; }
        
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Gets additional details about the error.
        /// </summary>
        public IDictionary<string, object> Details { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="details">Additional details about the error.</param>
        public Error(string code, string message, IDictionary<string, object> details = null)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Creates an error with the specified code and message.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <returns>A new error.</returns>
        public static Error Create(string code, string message) => new Error(code, message);
        
        /// <summary>
        /// Creates an error with the specified code, message, and details.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="details">Additional details about the error.</param>
        /// <returns>A new error.</returns>
        public static Error Create(string code, string message, IDictionary<string, object> details) => 
            new Error(code, message, details);
    }
}
