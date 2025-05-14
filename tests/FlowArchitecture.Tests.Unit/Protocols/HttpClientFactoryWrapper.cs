using System.Net.Http;
using FlowArchitecture.Protocols.Implementations;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace FlowArchitecture.Tests.Unit.Protocols
{
    /// <summary>
    /// Wrapper class to adapt System.Net.Http.IHttpClientFactory to FlowArchitecture.Protocols.Implementations.IHttpClientFactory
    /// </summary>
    public class HttpClientFactoryWrapper : FlowArchitecture.Protocols.Implementations.IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public HttpClientFactoryWrapper(IHttpClientFactory httpClientFactory)
        {
            // We don't actually use the httpClientFactory in tests
            _httpClient = new HttpClient();
        }

        public HttpClient CreateClient()
        {
            return _httpClient;
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }
}
