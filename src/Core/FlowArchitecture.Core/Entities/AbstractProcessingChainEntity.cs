using System.Collections.Generic;
using FlowArchitecture.Core.Common;

namespace FlowArchitecture.Core.Entities
{
    /// <summary>
    /// Base implementation for processing chain entities in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractProcessingChainEntity : AbstractEntity
    {
        /// <summary>
        /// Gets or sets the name of the processing chain.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the processing chain.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the processing chain is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets the steps in the processing chain.
        /// </summary>
        public IList<ProcessingStep> Steps { get; set; }
        
        /// <summary>
        /// Gets or sets the processing chain configuration.
        /// </summary>
        public IDictionary<string, object> Configuration { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractProcessingChainEntity"/> class.
        /// </summary>
        protected AbstractProcessingChainEntity() : base()
        {
            Name = string.Empty;
            Description = string.Empty;
            IsEnabled = false;
            Steps = new List<ProcessingStep>();
            Configuration = new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractProcessingChainEntity"/> class with the specified identifier.
        /// </summary>
        /// <param name="id">The identifier for the entity.</param>
        protected AbstractProcessingChainEntity(string id) : base(id)
        {
            Name = string.Empty;
            Description = string.Empty;
            IsEnabled = false;
            Steps = new List<ProcessingStep>();
            Configuration = new Dictionary<string, object>();
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
            
            if (Steps == null || Steps.Count == 0)
            {
                errors.Add(new ValidationError(nameof(Steps), "At least one processing step is required."));
            }
            else
            {
                for (int i = 0; i < Steps.Count; i++)
                {
                    var step = Steps[i];
                    if (string.IsNullOrEmpty(step.ProtocolId))
                    {
                        errors.Add(new ValidationError($"{nameof(Steps)}[{i}].{nameof(ProcessingStep.ProtocolId)}", "Protocol ID is required."));
                    }
                }
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
            dict.Add(nameof(Steps), Steps);
            dict.Add(nameof(Configuration), Configuration);
            
            return dict;
        }
    }
    
    /// <summary>
    /// Represents a step in a processing chain.
    /// </summary>
    public class ProcessingStep
    {
        /// <summary>
        /// Gets or sets the name of the step.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the step.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the protocol identifier.
        /// </summary>
        public string ProtocolId { get; set; }
        
        /// <summary>
        /// Gets or sets the protocol parameters.
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }
        
        /// <summary>
        /// Gets or sets the order of the step in the chain.
        /// </summary>
        public int Order { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the step is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to continue processing if this step fails.
        /// </summary>
        public bool ContinueOnError { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessingStep"/> class.
        /// </summary>
        public ProcessingStep()
        {
            Name = string.Empty;
            Description = string.Empty;
            ProtocolId = string.Empty;
            Parameters = new Dictionary<string, object>();
            Order = 0;
            IsEnabled = true;
            ContinueOnError = false;
        }
    }
}
