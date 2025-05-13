using System;
using System.Collections.Generic;
using FlowArchitecture.Core.Common;

namespace FlowArchitecture.Common.Services
{
    /// <summary>
    /// Represents the result of a service registration operation.
    /// </summary>
    public class RegistrationResult
    {
        /// <summary>
        /// Gets a value indicating whether the registration was successful.
        /// </summary>
        public bool IsSuccess { get; }
        
        /// <summary>
        /// Gets the service identifier.
        /// </summary>
        public string ServiceId { get; }
        
        /// <summary>
        /// Gets the service version.
        /// </summary>
        public string Version { get; }
        
        /// <summary>
        /// Gets the timestamp when the registration was completed.
        /// </summary>
        public DateTime CompletedAt { get; }
        
        /// <summary>
        /// Gets the validation result.
        /// </summary>
        public ValidationResult ValidationResult { get; }
        
        /// <summary>
        /// Gets the error message if the registration failed.
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Gets additional details about the registration.
        /// </summary>
        public IDictionary<string, object> Details { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationResult"/> class representing a successful registration.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="details">Additional details about the registration.</param>
        private RegistrationResult(string serviceId, string version, IDictionary<string, object> details = null)
        {
            IsSuccess = true;
            ServiceId = serviceId;
            Version = version;
            CompletedAt = DateTime.UtcNow;
            ValidationResult = ValidationResult.Success();
            ErrorMessage = null;
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationResult"/> class representing a failed registration due to validation errors.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="details">Additional details about the registration.</param>
        private RegistrationResult(ValidationResult validationResult, IDictionary<string, object> details = null)
        {
            IsSuccess = false;
            ServiceId = null;
            Version = null;
            CompletedAt = DateTime.UtcNow;
            ValidationResult = validationResult;
            ErrorMessage = "Validation failed";
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationResult"/> class representing a failed registration due to an error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="details">Additional details about the registration.</param>
        private RegistrationResult(string errorMessage, IDictionary<string, object> details = null)
        {
            IsSuccess = false;
            ServiceId = null;
            Version = null;
            CompletedAt = DateTime.UtcNow;
            ValidationResult = ValidationResult.Success();
            ErrorMessage = errorMessage;
            Details = details ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Creates a successful registration result.
        /// </summary>
        /// <param name="serviceId">The service identifier.</param>
        /// <param name="version">The service version.</param>
        /// <param name="details">Additional details about the registration.</param>
        /// <returns>A successful registration result.</returns>
        public static RegistrationResult Success(string serviceId, string version, IDictionary<string, object> details = null)
        {
            return new RegistrationResult(serviceId, version, details);
        }
        
        /// <summary>
        /// Creates a failed registration result due to validation errors.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <param name="details">Additional details about the registration.</param>
        /// <returns>A failed registration result.</returns>
        public static RegistrationResult ValidationFailure(ValidationResult validationResult, IDictionary<string, object> details = null)
        {
            return new RegistrationResult(validationResult, details);
        }
        
        /// <summary>
        /// Creates a failed registration result due to an error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="details">Additional details about the registration.</param>
        /// <returns>A failed registration result.</returns>
        public static RegistrationResult Failure(string errorMessage, IDictionary<string, object> details = null)
        {
            return new RegistrationResult(errorMessage, details);
        }
    }
}
