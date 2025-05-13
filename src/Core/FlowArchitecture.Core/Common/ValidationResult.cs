using System.Collections.Generic;
using System.Linq;

namespace FlowArchitecture.Core.Common
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid => !Errors.Any();
        
        /// <summary>
        /// Gets the collection of validation errors.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="errors">The validation errors.</param>
        public ValidationResult(IEnumerable<ValidationError> errors)
        {
            Errors = errors?.ToList() ?? new List<ValidationError>();
        }
        
        /// <summary>
        /// Creates a successful validation result with no errors.
        /// </summary>
        /// <returns>A successful validation result.</returns>
        public static ValidationResult Success() => new ValidationResult(Enumerable.Empty<ValidationError>());
        
        /// <summary>
        /// Creates a failed validation result with the specified errors.
        /// </summary>
        /// <param name="errors">The validation errors.</param>
        /// <returns>A failed validation result.</returns>
        public static ValidationResult Failure(params ValidationError[] errors) => new ValidationResult(errors);
        
        /// <summary>
        /// Creates a failed validation result with a single error.
        /// </summary>
        /// <param name="propertyName">The name of the property that failed validation.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>A failed validation result.</returns>
        public static ValidationResult Failure(string propertyName, string errorMessage) => 
            new ValidationResult(new[] { new ValidationError(propertyName, errorMessage) });
    }
    
    /// <summary>
    /// Represents a validation error for a specific property.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Gets the name of the property that failed validation.
        /// </summary>
        public string PropertyName { get; }
        
        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property that failed validation.</param>
        /// <param name="errorMessage">The error message.</param>
        public ValidationError(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }
    }
}
