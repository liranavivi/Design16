using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;

namespace FlowArchitecture.Protocols.Abstractions
{
    /// <summary>
    /// Interface for REST-based protocols in the Flow Architecture system.
    /// </summary>
    public interface IRestProtocol : IProtocol
    {
        /// <summary>
        /// Gets the base URL for the REST API.
        /// </summary>
        string BaseUrl { get; }
        
        /// <summary>
        /// Gets the authentication type for the REST API.
        /// </summary>
        string AuthenticationType { get; }
        
        /// <summary>
        /// Gets a value indicating whether the REST API requires authentication.
        /// </summary>
        bool RequiresAuthentication { get; }
        
        /// <summary>
        /// Sends a GET request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <param name="queryParameters">The query parameters for the request.</param>
        /// <param name="headers">The headers for the request.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the GET request.</returns>
        Task<Result<object>> GetAsync(
            string endpoint, 
            IDictionary<string, string>? queryParameters, 
            IDictionary<string, string>? headers, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Sends a POST request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="headers">The headers for the request.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the POST request.</returns>
        Task<Result<object>> PostAsync(
            string endpoint, 
            object data, 
            IDictionary<string, string>? headers, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Sends a PUT request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="headers">The headers for the request.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the PUT request.</returns>
        Task<Result<object>> PutAsync(
            string endpoint, 
            object data, 
            IDictionary<string, string>? headers, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Sends a DELETE request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <param name="headers">The headers for the request.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the DELETE request.</returns>
        Task<Result<bool>> DeleteAsync(
            string endpoint, 
            IDictionary<string, string>? headers, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default);
    }
}
