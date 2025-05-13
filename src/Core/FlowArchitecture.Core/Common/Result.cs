using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowArchitecture.Core.Common
{
    /// <summary>
    /// Represents the result of an operation that can either succeed with a value or fail with errors.
    /// </summary>
    /// <typeparam name="T">The type of the value in case of success.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }
        
        /// <summary>
        /// Gets the value in case of success.
        /// </summary>
        public T Value { get; }
        
        /// <summary>
        /// Gets the errors in case of failure.
        /// </summary>
        public IReadOnlyList<Error> Errors { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class representing a successful operation.
        /// </summary>
        /// <param name="value">The value.</param>
        private Result(T value)
        {
            IsSuccess = true;
            Value = value;
            Errors = Array.Empty<Error>();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class representing a failed operation.
        /// </summary>
        /// <param name="errors">The errors.</param>
        private Result(IEnumerable<Error> errors)
        {
            IsSuccess = false;
            Value = default;
            Errors = errors?.ToList() ?? new List<Error>();
        }
        
        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A successful result.</returns>
        public static Result<T> Success(T value) => new Result<T>(value);
        
        /// <summary>
        /// Creates a failed result with the specified errors.
        /// </summary>
        /// <param name="errors">The errors.</param>
        /// <returns>A failed result.</returns>
        public static Result<T> Failure(params Error[] errors) => new Result<T>(errors);
        
        /// <summary>
        /// Creates a failed result with a single error.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        /// <returns>A failed result.</returns>
        public static Result<T> Failure(string code, string message) => 
            new Result<T>(new[] { new Error(code, message) });
    }
}
