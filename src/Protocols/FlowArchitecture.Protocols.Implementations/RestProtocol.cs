using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;
using FlowArchitecture.Protocols.Abstractions;
using Microsoft.Extensions.Logging;

namespace FlowArchitecture.Protocols.Implementations
{
    /// <summary>
    /// Implementation of a REST-based protocol in the Flow Architecture system.
    /// </summary>
    public class RestProtocol : AbstractRestProtocol
    {
        private const string ContentTypeParameterName = "ContentType";
        private const string AcceptParameterName = "Accept";
        
        /// <summary>
        /// Gets the content type for HTTP requests.
        /// </summary>
        protected string ContentType { get; private set; } = "application/json";
        
        /// <summary>
        /// Gets the accept header for HTTP requests.
        /// </summary>
        protected string Accept { get; private set; } = "application/json";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RestProtocol"/> class.
        /// </summary>
        /// <param name="logger">The logger for this protocol.</param>
        /// <param name="httpClient">The HTTP client for making requests.</param>
        public RestProtocol(ILogger<RestProtocol> logger, HttpClient httpClient)
            : base("rest", "REST Protocol", "1.0", "Protocol for interacting with REST APIs.", logger, httpClient)
        {
        }
        
        /// <summary>
        /// Called when the protocol is being initialized.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override Task<Result<bool>> OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (ProtocolParameters.TryGetValue(ContentTypeParameterName, out var contentTypeObj) && contentTypeObj is string contentType)
            {
                ContentType = contentType;
            }
            
            if (ProtocolParameters.TryGetValue(AcceptParameterName, out var acceptObj) && acceptObj is string accept)
            {
                Accept = accept;
            }
            
            return base.OnInitializeAsync(cancellationToken);
        }
        
        /// <summary>
        /// Sends a GET request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <param name="queryParameters">The query parameters for the request.</param>
        /// <param name="headers">The headers for the request.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the GET request.</returns>
        public override async Task<Result<object>> GetAsync(
            string endpoint, 
            IDictionary<string, string>? queryParameters, 
            IDictionary<string, string>? headers, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(endpoint))
                {
                    return Result<object>.Failure("REST_GET_ERROR", "Endpoint cannot be null or empty.");
                }
                
                string url = BuildUrl(endpoint, queryParameters);
                
                Logger.LogInformation("Sending GET request to {Url}...", url);
                
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                
                AddHeaders(request, headers);
                
                if (RequiresAuthentication)
                {
                    AddAuthentication(request);
                }
                
                using var response = await HttpClient.SendAsync(request, cancellationToken);
                
                return await ProcessResponseAsync(response, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending GET request to {Endpoint}: {ErrorMessage}", endpoint, ex.Message);
                return Result<object>.Failure("REST_GET_ERROR", ex.Message);
            }
        }
        
        /// <summary>
        /// Sends a POST request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="headers">The headers for the request.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the POST request.</returns>
        public override async Task<Result<object>> PostAsync(
            string endpoint, 
            object data, 
            IDictionary<string, string>? headers, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(endpoint))
                {
                    return Result<object>.Failure("REST_POST_ERROR", "Endpoint cannot be null or empty.");
                }
                
                Logger.LogInformation("Sending POST request to {Endpoint}...", endpoint);
                
                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                
                AddHeaders(request, headers);
                
                if (RequiresAuthentication)
                {
                    AddAuthentication(request);
                }
                
                if (data != null)
                {
                    string json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, ContentType);
                }
                
                using var response = await HttpClient.SendAsync(request, cancellationToken);
                
                return await ProcessResponseAsync(response, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending POST request to {Endpoint}: {ErrorMessage}", endpoint, ex.Message);
                return Result<object>.Failure("REST_POST_ERROR", ex.Message);
            }
        }
        
        /// <summary>
        /// Sends a PUT request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <param name="data">The data to send in the request body.</param>
        /// <param name="headers">The headers for the request.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the PUT request.</returns>
        public override async Task<Result<object>> PutAsync(
            string endpoint, 
            object data, 
            IDictionary<string, string>? headers, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(endpoint))
                {
                    return Result<object>.Failure("REST_PUT_ERROR", "Endpoint cannot be null or empty.");
                }
                
                Logger.LogInformation("Sending PUT request to {Endpoint}...", endpoint);
                
                using var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
                
                AddHeaders(request, headers);
                
                if (RequiresAuthentication)
                {
                    AddAuthentication(request);
                }
                
                if (data != null)
                {
                    string json = JsonSerializer.Serialize(data);
                    request.Content = new StringContent(json, Encoding.UTF8, ContentType);
                }
                
                using var response = await HttpClient.SendAsync(request, cancellationToken);
                
                return await ProcessResponseAsync(response, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending PUT request to {Endpoint}: {ErrorMessage}", endpoint, ex.Message);
                return Result<object>.Failure("REST_PUT_ERROR", ex.Message);
            }
        }
        
        /// <summary>
        /// Sends a DELETE request to the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to send the request to.</param>
        /// <param name="headers">The headers for the request.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the DELETE request.</returns>
        public override async Task<Result<bool>> DeleteAsync(
            string endpoint, 
            IDictionary<string, string>? headers, 
            ProtocolExecutionContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(endpoint))
                {
                    return Result<bool>.Failure("REST_DELETE_ERROR", "Endpoint cannot be null or empty.");
                }
                
                Logger.LogInformation("Sending DELETE request to {Endpoint}...", endpoint);
                
                using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                
                AddHeaders(request, headers);
                
                if (RequiresAuthentication)
                {
                    AddAuthentication(request);
                }
                
                using var response = await HttpClient.SendAsync(request, cancellationToken);
                
                response.EnsureSuccessStatusCode();
                
                Logger.LogInformation("DELETE request to {Endpoint} completed successfully with status code {StatusCode}.", endpoint, response.StatusCode);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error sending DELETE request to {Endpoint}: {ErrorMessage}", endpoint, ex.Message);
                return Result<bool>.Failure("REST_DELETE_ERROR", ex.Message);
            }
        }
        
        /// <summary>
        /// Builds a URL with query parameters.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="queryParameters">The query parameters.</param>
        /// <returns>The URL with query parameters.</returns>
        protected virtual string BuildUrl(string endpoint, IDictionary<string, string>? queryParameters)
        {
            if (queryParameters == null || queryParameters.Count == 0)
            {
                return endpoint;
            }
            
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            
            foreach (var parameter in queryParameters)
            {
                queryString[parameter.Key] = parameter.Value;
            }
            
            return $"{endpoint}?{queryString}";
        }
        
        /// <summary>
        /// Adds headers to an HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="headers">The headers to add.</param>
        protected virtual void AddHeaders(HttpRequestMessage request, IDictionary<string, string>? headers)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Accept));
            
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }
        
        /// <summary>
        /// Adds authentication to an HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        protected virtual void AddAuthentication(HttpRequestMessage request)
        {
            switch (AuthenticationType.ToLowerInvariant())
            {
                case "basic":
                    if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
                    {
                        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                    }
                    break;
                
                case "bearer":
                    if (!string.IsNullOrEmpty(ApiKey))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                    }
                    break;
                
                case "apikey":
                    if (!string.IsNullOrEmpty(ApiKey))
                    {
                        request.Headers.Add("X-API-Key", ApiKey);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Processes an HTTP response.
        /// </summary>
        /// <param name="response">The HTTP response.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the response processing.</returns>
        protected virtual async Task<Result<object>> ProcessResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                response.EnsureSuccessStatusCode();
                
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                if (string.IsNullOrEmpty(content))
                {
                    return Result<object>.Success(new object());
                }
                
                if (response.Content.Headers.ContentType?.MediaType?.Contains("json") == true)
                {
                    var result = JsonSerializer.Deserialize<object>(content);
                    return Result<object>.Success(result ?? new object());
                }
                
                return Result<object>.Success(content);
            }
            catch (Exception ex)
            {
                return Result<object>.Failure("REST_RESPONSE_ERROR", ex.Message);
            }
        }
    }
}
