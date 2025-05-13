using System.Collections.Generic;

namespace FlowArchitecture.Common.Services
{
    /// <summary>
    /// Represents the merge capabilities of a service.
    /// </summary>
    public class MergeCapabilities
    {
        /// <summary>
        /// Gets a value indicating whether the service supports merging.
        /// </summary>
        public bool SupportsMerging { get; }
        
        /// <summary>
        /// Gets the supported merge strategies.
        /// </summary>
        public IReadOnlyList<MergeStrategy> SupportedStrategies { get; }
        
        /// <summary>
        /// Gets the default merge strategy.
        /// </summary>
        public MergeStrategy DefaultStrategy { get; }
        
        /// <summary>
        /// Gets additional capabilities related to merging.
        /// </summary>
        public IDictionary<string, object> AdditionalCapabilities { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MergeCapabilities"/> class.
        /// </summary>
        /// <param name="supportsMerging">A value indicating whether the service supports merging.</param>
        /// <param name="supportedStrategies">The supported merge strategies.</param>
        /// <param name="defaultStrategy">The default merge strategy.</param>
        /// <param name="additionalCapabilities">Additional capabilities related to merging.</param>
        public MergeCapabilities(
            bool supportsMerging,
            IEnumerable<MergeStrategy> supportedStrategies,
            MergeStrategy defaultStrategy,
            IDictionary<string, object> additionalCapabilities = null)
        {
            SupportsMerging = supportsMerging;
            SupportedStrategies = supportedStrategies != null ? new List<MergeStrategy>(supportedStrategies) : new List<MergeStrategy>();
            DefaultStrategy = defaultStrategy;
            AdditionalCapabilities = additionalCapabilities ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Creates a merge capabilities object that does not support merging.
        /// </summary>
        /// <returns>A merge capabilities object that does not support merging.</returns>
        public static MergeCapabilities None()
        {
            return new MergeCapabilities(false, new List<MergeStrategy>(), MergeStrategy.None);
        }
    }
    
    /// <summary>
    /// Represents a strategy for merging data from multiple branches.
    /// </summary>
    public enum MergeStrategy
    {
        /// <summary>
        /// No merging is performed.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Data from all branches is concatenated.
        /// </summary>
        Concatenate = 1,
        
        /// <summary>
        /// Data from all branches is merged by key.
        /// </summary>
        MergeByKey = 2,
        
        /// <summary>
        /// Data from all branches is merged by position.
        /// </summary>
        MergeByPosition = 3,
        
        /// <summary>
        /// Only data from the first branch is used.
        /// </summary>
        UseFirst = 4,
        
        /// <summary>
        /// Only data from the last branch is used.
        /// </summary>
        UseLast = 5,
        
        /// <summary>
        /// A custom merging strategy is used.
        /// </summary>
        Custom = 6
    }
}
