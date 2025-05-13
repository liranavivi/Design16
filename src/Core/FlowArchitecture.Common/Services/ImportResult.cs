using System;
using System.Collections.Generic;

namespace FlowArchitecture.Common.Services
{
    /// <summary>
    /// Represents the result of an import operation.
    /// </summary>
    public class ImportResult
    {
        /// <summary>
        /// Gets a value indicating whether the import operation was successful.
        /// </summary>
        public bool IsSuccess { get; }
        
        /// <summary>
        /// Gets the imported data.
        /// </summary>
        public object Data { get; }
        
        /// <summary>
        /// Gets the timestamp when the import operation was completed.
        /// </summary>
        public DateTime CompletedAt { get; }
        
        /// <summary>
        /// Gets the error message if the import operation failed.
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Gets additional details about the import operation.
        /// </summary>
        public IDictionary<string, object> Details { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportResult"/> class representing a successful import operation.
        /// </summary>
        /// <param name="data">The imported data.</param>
        /// <param name="details">Additional details about the import operation.</param>
        private ImportResult(object data, IDictionary<string, object> details = null)
        {
            IsSuccess = true;
            Data = data;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = null;
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportResult"/> class representing a failed import operation.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="details">Additional details about the import operation.</param>
        private ImportResult(string errorMessage, IDictionary<string, object> details = null)
        {
            IsSuccess = false;
            Data = null;
            CompletedAt = DateTime.UtcNow;
            ErrorMessage = errorMessage;
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Creates a successful import result with the specified data.
        /// </summary>
        /// <param name="data">The imported data.</param>
        /// <param name="details">Additional details about the import operation.</param>
        /// <returns>A successful import result.</returns>
        public static ImportResult Success(object data, IDictionary<string, object> details = null)
        {
            return new ImportResult(data, details);
        }
        
        /// <summary>
        /// Creates a failed import result with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="details">Additional details about the import operation.</param>
        /// <returns>A failed import result.</returns>
        public static ImportResult Failure(string errorMessage, IDictionary<string, object> details = null)
        {
            return new ImportResult(errorMessage, details);
        }
    }
}
