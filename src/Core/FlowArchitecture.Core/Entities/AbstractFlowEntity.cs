using System;
using System.Collections.Generic;
using FlowArchitecture.Core.Common;

namespace FlowArchitecture.Core.Entities
{
    /// <summary>
    /// Base implementation for flow entities in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractFlowEntity : AbstractEntity
    {
        /// <summary>
        /// Gets or sets the name of the flow.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the flow.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the flow is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the processing chain identifier.
        /// </summary>
        public string ProcessingChainId { get; set; }
        
        /// <summary>
        /// Gets or sets the source identifiers.
        /// </summary>
        public IList<string> SourceIds { get; set; }
        
        /// <summary>
        /// Gets or sets the destination identifiers.
        /// </summary>
        public IList<string> DestinationIds { get; set; }
        
        /// <summary>
        /// Gets or sets the flow configuration.
        /// </summary>
        public IDictionary<string, object> Configuration { get; set; }
        
        /// <summary>
        /// Gets or sets the flow status.
        /// </summary>
        public FlowStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the flow was last executed.
        /// </summary>
        public DateTime? LastExecutedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the timestamp when the flow is scheduled to be executed next.
        /// </summary>
        public DateTime? NextExecutionAt { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractFlowEntity"/> class.
        /// </summary>
        protected AbstractFlowEntity() : base()
        {
            Name = string.Empty;
            Description = string.Empty;
            IsEnabled = false;
            ProcessingChainId = string.Empty;
            SourceIds = new List<string>();
            DestinationIds = new List<string>();
            Configuration = new Dictionary<string, object>();
            Status = FlowStatus.Idle;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractFlowEntity"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier for the entity.</param>
        protected AbstractFlowEntity(string id) : base(id)
        {
            Name = string.Empty;
            Description = string.Empty;
            IsEnabled = false;
            ProcessingChainId = string.Empty;
            SourceIds = new List<string>();
            DestinationIds = new List<string>();
            Configuration = new Dictionary<string, object>();
            Status = FlowStatus.Idle;
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
            
            if (string.IsNullOrEmpty(ProcessingChainId))
            {
                errors.Add(new ValidationError(nameof(ProcessingChainId), "Processing chain ID is required."));
            }
            
            if (SourceIds == null || SourceIds.Count == 0)
            {
                errors.Add(new ValidationError(nameof(SourceIds), "At least one source ID is required."));
            }
            
            if (DestinationIds == null || DestinationIds.Count == 0)
            {
                errors.Add(new ValidationError(nameof(DestinationIds), "At least one destination ID is required."));
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
            dict.Add(nameof(ProcessingChainId), ProcessingChainId);
            dict.Add(nameof(SourceIds), SourceIds);
            dict.Add(nameof(DestinationIds), DestinationIds);
            dict.Add(nameof(Configuration), Configuration);
            dict.Add(nameof(Status), Status);
            dict.Add(nameof(LastExecutedAt), LastExecutedAt);
            dict.Add(nameof(NextExecutionAt), NextExecutionAt);
            
            return dict;
        }
    }
    
    /// <summary>
    /// Represents the status of a flow.
    /// </summary>
    public enum FlowStatus
    {
        /// <summary>
        /// The flow is idle.
        /// </summary>
        Idle = 0,
        
        /// <summary>
        /// The flow is running.
        /// </summary>
        Running = 1,
        
        /// <summary>
        /// The flow has completed successfully.
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// The flow has failed.
        /// </summary>
        Failed = 3,
        
        /// <summary>
        /// The flow is paused.
        /// </summary>
        Paused = 4
    }
}
