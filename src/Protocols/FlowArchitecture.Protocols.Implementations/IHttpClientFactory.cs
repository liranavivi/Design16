using System.Net.Http;

namespace FlowArchitecture.Protocols.Implementations
{
    /// <summary>
    /// Interface for HTTP client factories.
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Creates an HTTP client.
        /// </summary>
        /// <returns>The HTTP client.</returns>
        HttpClient CreateClient();
        
        /// <summary>
        /// Creates an HTTP client with the specified name.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <returns>The HTTP client.</returns>
        HttpClient CreateClient(string name);
    }
}
