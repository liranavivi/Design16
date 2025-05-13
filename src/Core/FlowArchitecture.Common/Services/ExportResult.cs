using System;
using System.Collections.Generic;

namespace FlowArchitecture.Common.Services
{
    /// <summary>
    /// Represents the result of an export operation.
    /// </summary>
    public class ExportResult
    {
        /// <summary>
        /// Gets a value indicating whether the export operation was successful.
        /// </summary>
        public bool IsSuccess { get; }
        
        /// <summary>
        /// Gets the timestamp when the export operation was completed.
        /// </summary>
        public DateTime CompletedAt { get; }
        
        /// <summary>
        /// Gets the error message if the export operation failed.
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Gets additional details about the export operation.
        /// </summary>
        public IDictionary<string, object> Details { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportResult"/> class representing a successful export operation.
        /// </summary>
        /// <param name="details">Additional details about the export operation.</param>
        private ExportResult(IDictionary<string, object> details = null)
        {
            IsSuccess = true;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = null;
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportResult"/> class representing a failed export operation.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="details">Additional details about the export operation.</param>
        private ExportResult(string errorMessage, IDictionary<string, object> details = null)
        {
            IsSuccess = false;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = errorMessage;
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Creates a successful export result.
        /// </summary>
        /// <param name="details">Additional details about the export operation.</param>
        /// <returns>A successful export result.</returns>
        public static ExportResult Success(IDictionary<string, object> details = null)
        {
            return new ExportResult(details);
        }
        
        /// <summary>
        /// Creates a failed export result with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="details">Additional details about the export operation.</param>
        /// <returns>A failed export result.</returns>
        public static ExportResult Failure(string errorMessage, IDictionary<string, object> details = null)
        {
            return new ExportResult(errorMessage, details);
        }
    }
}
