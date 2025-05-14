using System.Net.Http;

namespace FlowArchitecture.Protocols.Implementations
{
    /// <summary>
    /// Default implementation of the HTTP client factory.
    /// </summary>
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        /// <summary>
        /// Creates an HTTP client.
        /// </summary>
        /// <returns>The HTTP client.</returns>
        public HttpClient CreateClient()
        {
            return new HttpClient();
        }
        
        /// <summary>
        /// Creates an HTTP client with the specified name.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <returns>The HTTP client.</returns>
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }
}
