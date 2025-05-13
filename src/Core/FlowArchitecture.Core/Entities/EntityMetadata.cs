using System;
using System.Collections.Generic;

namespace FlowArchitecture.Core.Entities
{
    /// <summary>
    /// Represents metadata for an entity.
    /// </summary>
    public class EntityMetadata
    {
        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public string EntityType { get; }
        
        /// <summary>
        /// Gets the entity schema version.
        /// </summary>
        public string SchemaVersion { get; }
        
        /// <summary>
        /// Gets the timestamp when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; }
        
        /// <summary>
        /// Gets the timestamp when the entity was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; }
        
        /// <summary>
        /// Gets the user who created the entity.
        /// </summary>
        public string CreatedBy { get; }
        
        /// <summary>
        /// Gets the user who last updated the entity.
        /// </summary>
        public string UpdatedBy { get; }
        
        /// <summary>
        /// Gets additional metadata properties.
        /// </summary>
        public IDictionary<string, object> Properties { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMetadata"/> class.
        /// </summary>
        /// <param name="entityType">The entity type.</param>
        /// <param name="schemaVersion">The entity schema version.</param>
        /// <param name="createdAt">The timestamp when the entity was created.</param>
        /// <param name="updatedAt">The timestamp when the entity was last updated.</param>
        /// <param name="createdBy">The user who created the entity.</param>
        /// <param name="updatedBy">The user who last updated the entity.</param>
        /// <param name="properties">Additional metadata properties.</param>
        public EntityMetadata(
            string entityType,
            string schemaVersion,
            DateTime createdAt,
            DateTime updatedAt,
            string createdBy = null,
            string updatedBy = null,
            IDictionary<string, object> properties = null)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            SchemaVersion = schemaVersion ?? throw new ArgumentNullException(nameof(schemaVersion));
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            CreatedBy = createdBy;
            UpdatedBy = updatedBy;
            Properties = properties ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="EntityMetadata"/> class with updated timestamp and user.
        /// </summary>
        /// <param name="updatedBy">The user who updated the entity.</param>
        /// <returns>A new instance of the <see cref="EntityMetadata"/> class.</returns>
        public EntityMetadata WithUpdate(string updatedBy = null)
        {
            return new EntityMetadata(
                EntityType,
                SchemaVersion,
                CreatedAt,
                DateTime.UtcNow,
                CreatedBy,
                updatedBy ?? UpdatedBy,
                new Dictionary<string, object>(Properties));
        }
    }
}
