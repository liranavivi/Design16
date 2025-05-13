using System;

namespace FlowArchitecture.Core.Protocols
{
    /// <summary>
    /// Represents a parameter for a protocol.
    /// </summary>
    public class ProtocolParameter
    {
        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the display name of the parameter.
        /// </summary>
        public string DisplayName { get; }
        
        /// <summary>
        /// Gets the description of the parameter.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Gets the type of the parameter.
        /// </summary>
        public Type ParameterType { get; }
        
        /// <summary>
        /// Gets a value indicating whether the parameter is required.
        /// </summary>
        public bool IsRequired { get; }
        
        /// <summary>
        /// Gets the default value of the parameter.
        /// </summary>
        public object DefaultValue { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolParameter"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameterType">The type of the parameter.</param>
        /// <param name="isRequired">A value indicating whether the parameter is required.</param>
        /// <param name="displayName">The display name of the parameter.</param>
        /// <param name="description">The description of the parameter.</param>
        /// <param name="defaultValue">The default value of the parameter.</param>
        public ProtocolParameter(
            string name,
            Type parameterType,
            bool isRequired,
            string displayName = null,
            string description = null,
            object defaultValue = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
            IsRequired = isRequired;
            DisplayName = displayName ?? name;
            Description = description;
            DefaultValue = defaultValue;
        }
    }
}
