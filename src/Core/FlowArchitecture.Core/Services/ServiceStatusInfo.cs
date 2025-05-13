using System;
using System.Collections.Generic;

namespace FlowArchitecture.Core.Services
{
    /// <summary>
    /// Contains detailed status information about a service.
    /// </summary>
    public class ServiceStatusInfo
    {
        /// <summary>
        /// Gets the current status of the service.
        /// </summary>
        public ServiceStatus Status { get; }
        
        /// <summary>
        /// Gets the timestamp when the status was last updated.
        /// </summary>
        public DateTime LastUpdated { get; }
        
        /// <summary>
        /// Gets the error message if the service is in an error state.
        /// </summary>
        public string ErrorMessage { get; }
        
        /// <summary>
        /// Gets additional properties providing more details about the service status.
        /// </summary>
        public IDictionary<string, object> Properties { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceStatusInfo"/> class.
        /// </summary>
        /// <param name="status">The current status of the service.</param>
        /// <param name="errorMessage">The error message if the service is in an error state.</param>
        /// <param name="properties">Additional properties providing more details about the service status.</param>
        public ServiceStatusInfo(
            ServiceStatus status, 
            string errorMessage = null, 
            IDictionary<string, object> properties = null)
        {
            Status = status;
            LastUpdated = DateTime.UtcNow;
            ErrorMessage = errorMessage;
            Properties = properties ?? new Dictionary<string, object>();
        }
    }
}
