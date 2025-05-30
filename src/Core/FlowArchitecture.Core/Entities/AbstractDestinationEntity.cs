using System.Collections.Generic;
using FlowArchitecture.Core.Common;

namespace FlowArchitecture.Core.Entities
{
    /// <summary>
    /// Base implementation for destination entities in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractDestinationEntity : AbstractEntity
    {
        /// <summary>
        /// Gets or sets the name of the destination.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the destination.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the destination is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the type of the destination.
        /// </summary>
        public string DestinationType { get; set; }
        
        /// <summary>
        /// Gets or sets the protocol identifier.
        /// </summary>
        public string ProtocolId { get; set; }
        
        /// <summary>
        /// Gets or sets the protocol parameters.
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDestinationEntity"/> class.
        /// </summary>
        protected AbstractDestinationEntity() : base()
        {
            Name = string.Empty;
            Description = string.Empty;
            IsEnabled = false;
            DestinationType = string.Empty;
            ProtocolId = string.Empty;
            Parameters = new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDestinationEntity"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier for the entity.</param>
        protected AbstractDestinationEntity(string id) : base(id)
        {
            Name = string.Empty;
            Description = string.Empty;
            IsEnabled = false;
            DestinationType = string.Empty;
            ProtocolId = string.Empty;
            Parameters = new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Validates the entity state.
        /// </summary>
        /// <returns>A validation result indicating whether the entity is valid.</returns>
        public override ValidationResult Validate()
        {
            var baseResult = base.Validate();
            var errors = new List<ValidationError>(baseResult.Errors);
            
            if (string.IsNullOrEmpty(Name))
            {
                errors.Add(new ValidationError(nameof(Name), "Name is required."));
            }
            
            if (string.IsNullOrEmpty(DestinationType))
            {
                errors.Add(new ValidationError(nameof(DestinationType), "Destination type is required."));
            }
            
            if (string.IsNullOrEmpty(ProtocolId))
            {
                errors.Add(new ValidationError(nameof(ProtocolId), "Protocol ID is required."));
            }
            
            return new ValidationResult(errors);
        }
        
        /// <summary>
        /// Converts the entity to a dictionary representation.
        /// </summary>
        /// <returns>A dictionary containing the entity's properties and values.</returns>
        public override IDictionary<string, object> ToDictionary()
        {
            var dict = base.ToDictionary();
            
            dict.Add(nameof(Name), Name);
            dict.Add(nameof(Description), Description);
            dict.Add(nameof(IsEnabled), IsEnabled);
            dict.Add(nameof(DestinationType), DestinationType);
            dict.Add(nameof(ProtocolId), ProtocolId);
            dict.Add(nameof(Parameters), Parameters);
            
            return dict;
        }
    }
}
