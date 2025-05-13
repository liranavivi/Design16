using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;

namespace FlowArchitecture.Core.Entities
{
    /// <summary>
    /// Base implementation for all entities in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractEntity : IEntity
    {
        /// <summary>
        /// Gets the unique identifier for the entity.
        /// </summary>
        public string Id { get; protected set; }
        
        /// <summary>
        /// Gets the version of the entity for optimistic concurrency.
        /// </summary>
        public string Version { get; protected set; }
        
        /// <summary>
        /// Gets the timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; protected set; }
        
        /// <summary>
        /// Gets the timestamp when the entity was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; protected set; }
        
        /// <summary>
        /// Gets the metadata for the entity.
        /// </summary>
        public EntityMetadata Metadata { get; protected set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEntity"/> class.
        /// </summary>
        protected AbstractEntity()
        {
            Id = Guid.NewGuid().ToString();
            Version = "1";
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
            InitializeMetadata();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractEntity"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier for the entity.</param>
        protected AbstractEntity(string id) : this()
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }
        
        /// <summary>
        /// Initializes the metadata for the entity.
        /// </summary>
        protected virtual void InitializeMetadata()
        {
            Metadata = new EntityMetadata(
                GetType().Name,
                "1.0",
                CreatedAt,
                UpdatedAt);
        }
        
        /// <summary>
        /// Updates the entity version and timestamp.
        /// </summary>
        protected virtual void UpdateVersionAndTimestamp()
        {
            Version = (int.Parse(Version) + 1).ToString();
            UpdatedAt = DateTime.UtcNow;
            Metadata = Metadata.WithUpdate();
        }
        
        /// <summary>
        /// Validates the entity state.
        /// </summary>
        /// <returns>A validation result indicating whether the entity is valid.</returns>
        public virtual ValidationResult Validate()
        {
            var errors = new List<ValidationError>();
            
            if (string.IsNullOrEmpty(Id))
            {
                errors.Add(new ValidationError(nameof(Id), "Id is required."));
            }
            
            if (string.IsNullOrEmpty(Version))
            {
                errors.Add(new ValidationError(nameof(Version), "Version is required."));
            }
            
            return new ValidationResult(errors);
        }
        
        /// <summary>
        /// Validates the entity state asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing a validation result.</returns>
        public virtual Task<ValidationResult> ValidateAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Validate());
        }
        
        /// <summary>
        /// Creates a deep clone of the entity.
        /// </summary>
        /// <returns>A new instance of the entity with the same state.</returns>
        public abstract IEntity Clone();
        
        /// <summary>
        /// Converts the entity to a dictionary representation.
        /// </summary>
        /// <returns>A dictionary containing the entity's properties and values.</returns>
        public virtual IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                { nameof(Id), Id },
                { nameof(Version), Version },
                { nameof(CreatedAt), CreatedAt },
                { nameof(UpdatedAt), UpdatedAt },
                { nameof(Metadata), Metadata }
            };
        }
        
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{GetType().Name} [Id={Id}, Version={Version}]";
        }
    }
}
