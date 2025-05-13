using System;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;

namespace FlowArchitecture.Core.Entities
{
    /// <summary>
    /// Base interface for all entities in the Flow Architecture system.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Gets the unique identifier for the entity.
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// Gets the version of the entity for optimistic concurrency.
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// Gets the timestamp when the entity was created.
        /// </summary>
        DateTime CreatedAt { get; }
        
        /// <summary>
        /// Gets the timestamp when the entity was last updated.
        /// </summary>
        DateTime UpdatedAt { get; }
        
        /// <summary>
        /// Validates the entity state.
        /// </summary>
        /// <returns>A validation result indicating whether the entity is valid.</returns>
        ValidationResult Validate();
        
        /// <summary>
        /// Validates the entity state asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing a validation result.</returns>
        Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a deep clone of the entity.
        /// </summary>
        /// <returns>A new instance of the entity with the same state.</returns>
        IEntity Clone();
        
        /// <summary>
        /// Converts the entity to a dictionary representation.
        /// </summary>
        /// <returns>A dictionary containing the entity's properties and values.</returns>
        IDictionary<string, object> ToDictionary();
    }
}
