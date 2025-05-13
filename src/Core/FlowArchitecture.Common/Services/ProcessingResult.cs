using System;
using System.Collections.Generic;

namespace FlowArchitecture.Common.Services
{
    /// <summary>
    /// Represents the result of a processing operation.
    /// </summary>
    public class ProcessingResult
    {
        /// <summary>
        /// Gets a value indicating whether the processing operation was successful.
        /// </summary>
        public bool IsSuccess { get; }
        
        /// <summary>
        /// Gets the processed data.
        /// </summary>
        public object Data { get; }
        
        /// <summary>
        /// Gets the timestamp when the processing operation was completed.
        /// </summary>
        public DateTime CompletedAt { get; }
        
        /// <summary>
        /// Gets the error message if the processing operation failed.
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Gets additional details about the processing operation.
        /// </summary>
        public IDictionary<string, object> Details { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingResult"/> class representing a successful processing operation.
        /// </summary>
        /// <param name="data">The processed data.</param>
        /// <param name="details">Additional details about the processing operation.</param>
        private ProcessingResult(object data, IDictionary<string, object> details = null)
        {
            IsSuccess = true;
            Data = data;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = null;
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingResult"/> class representing a failed processing operation.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="details">Additional details about the processing operation.</param>
        private ProcessingResult(string errorMessage, IDictionary<string, object> details = null)
        {
            IsSuccess = false;
            Data = null;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = errorMessage;
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Creates a successful processing result with the specified data.
        /// </summary>
        /// <param name="data">The processed data.</param>
        /// <param name="details">Additional details about the processing operation.</param>
        /// <returns>A successful processing result.</returns>
        public static ProcessingResult Success(object data, IDictionary<string, object> details = null)
        {
            return new ProcessingResult(data, details);
        }
        
        /// <summary>
        /// Creates a failed processing result with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="details">Additional details about the processing operation.</param>
        /// <returns>A failed processing result.</returns>
        public static ProcessingResult Failure(string errorMessage, IDictionary<string, object> details = null)
        {
            return new ProcessingResult(errorMessage, details);
        }
    }
}
