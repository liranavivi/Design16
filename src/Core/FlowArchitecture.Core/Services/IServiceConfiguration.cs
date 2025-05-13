using System.Collections.Generic;

namespace FlowArchitecture.Core.Services
{
    /// <summary>
    /// Represents configuration parameters for a service.
    /// </summary>
    public interface IServiceConfiguration
    {
        /// <summary>
        /// Gets the configuration parameters.
        /// </summary>
        IDictionary<string, object> Parameters { get; }
        
        /// <summary>
        /// Gets a configuration parameter by name.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="defaultValue">The default value to return if the parameter is not found.</param>
        /// <returns>The parameter value, or the default value if the parameter is not found.</returns>
        T GetParameter<T>(string name, T defaultValue = default);
    }
}
