using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;
using Microsoft.Extensions.Logging;

namespace FlowArchitecture.Protocols.Abstractions
{
    /// <summary>
    /// Base implementation for REST-based protocols in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractRestProtocol : AbstractProtocol, IRestProtocol
    {
        private const string BaseUrlParameterName = "BaseUrl";
        private const string AuthenticationTypeParameterName = "AuthenticationType";
        private const string UsernameParameterName = "Username";
        private const string PasswordParameterName = "Password";
        private const string ApiKeyParameterName = "ApiKey";
        private const string TimeoutParameterName = "Timeout";

        /// <summary>
        /// Gets the base URL for the REST API.
        /// </summary>
        public string BaseUrl { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the authentication type for the REST API.
        /// </summary>
        public string AuthenticationType { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the REST API requires authentication.
        /// </summary>
        public bool RequiresAuthentication => !string.IsNullOrEmpty(AuthenticationType);

        /// <summary>
        /// Gets the username for authentication.
        /// </summary>
        protected string? Username { get; private set; }

        /// <summary>
        /// Gets the password for authentication.
        /// </summary>
        protected string? Password { get; private set; }

        /// <summary>
        /// Gets the API key for authentication.
        /// </summary>
        protected string? ApiKey { get; private set; }

        /// <summary>
        /// Gets the timeout for HTTP requests.
        /// </summary>
        protected TimeSpan Timeout { get; private set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets the HTTP client for making requests.
        /// </summary>
        protected HttpClient HttpClient { get; private set; }

        /// <summary>
        /// Gets the parameters required by the protocol.
        /// </summary>
        public override IReadOnlyList<ProtocolParameter> Parameters => new List<ProtocolParameter>
        {
            new ProtocolParameter(
                BaseUrlParameterName,
                typeof(string),
                true,
                "Base URL",
                "The base URL for the REST API."),

            new ProtocolParameter(
                AuthenticationTypeParameterName,
                typeof(string),
                false,
                "Authentication Type",
                "The authentication type for the REST API (e.g., Basic, Bearer, ApiKey)."),

            new ProtocolParameter(
                UsernameParameterName,
                typeof(string),
                false,
                "Username",
                "The username for authentication."),

            new ProtocolParameter(
                PasswordParameterName,
                typeof(string),
                false,
                "Password",
                "The password for authentication."),

            new ProtocolParameter(
                ApiKeyParameterName,
                typeof(string),
                false,
                "API Key",
                "The API key for authentication."),

            new ProtocolParameter(
                TimeoutParameterName,
                typeof(int),
                false,
                "Timeout",
                "The timeout for HTTP requests in seconds.",
                30)
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractRestProtocol"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the protocol.</param>
        /// <param name="name">The name of the protocol.</param>
        /// <param name="version">The version of the protocol.</param>
        /// <param name="description">The description of the protocol.</param>
        /// <param name="logger">The logger for this protocol.</param>
        /// <param name="httpClient">The HTTP client for making requests.</param>
        protected AbstractRestProtocol(string id, string name, string version, string description, ILogger logger, HttpClient httpClient)
            : base(id, name, version, description, logger)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Called when the protocol is being initialized.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override Task<Result<bool>> OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (ProtocolParameters.TryGetValue(BaseUrlParameterName, out var baseUrlObj) && baseUrlObj is string baseUrl)
            {
                BaseUrl = baseUrl;
                HttpClient.BaseAddress = new Uri(baseUrl);
            }

            if (ProtocolParameters.TryGetValue(AuthenticationTypeParameterName, out var authTypeObj) && authTypeObj is string authType)
            {
                AuthenticationType = authType;
            }

            if (ProtocolParameters.TryGetValue(UsernameParameterName, out var usernameObj) && usernameObj is string username)
            {
                Username = username;
            }

            if (ProtocolParameters.TryGetValue(PasswordParameterName, out var passwordObj) && passwordObj is string password)
            {
                Password = password;
            }

            if (ProtocolParameters.TryGetValue(ApiKeyParameterName, out var apiKeyObj) && apiKeyObj is string apiKey)
            {
                ApiKey = apiKey;
            }

            if (ProtocolParameters.TryGetValue(TimeoutParameterName, out var timeoutObj) && timeoutObj is int timeoutSeconds)
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds);
                HttpClient.Timeout = Timeout;
            }

            return base.OnInitializeAsync(cancellationToken);
        }

        /// <summary>
        /// Called when the protocol is being executed.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the execution.</returns>
        protected override async Task<Result<object>> OnExecuteAsync(ProtocolExecutionContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(BaseUrl))
            {
                return Result<object>.Failure("REST_PROTOCOL_ERROR", "Base URL is not set.");
            }

            // The specific HTTP method to use should be determined by the context
            if (context.Parameters.TryGetValue("Method", out var methodObj) && methodObj is string method)
            {
                switch (method.ToUpperInvariant())
                {
                    case "GET":
                        return await GetAsync(
                            GetEndpointFromContext(context),
                            GetQueryParametersFromContext(context),
                            GetHeadersFromContext(context),
                            context,
                            cancellationToken);

                    case "POST":
                        return await PostAsync(
                            GetEndpointFromContext(context),
                            context.InputData,
                            GetHeadersFromContext(context),
                            context,
                            cancellationToken);

                    case "PUT":
                        return await PutAsync(
                            GetEndpointFromContext(context),
                            context.InputData,
                            GetHeadersFromContext(context),
                            context,
                            cancellationToken);

                    case "DELETE":
                        var result = await DeleteAsync(
                            GetEndpointFromContext(context),
                            GetHeadersFromContext(context),
                            context,
                            cancellationToken);

                        return result.IsSuccess
                            ? Result<object>.Success(result.Value)
                            : Result<object>.Failure(result.Errors.ToArray());

                    default:
                        return Result<object>.Failure("REST_PROTOCOL_ERROR", $"Unsupported HTTP method: {method}");
                }
            }

            // Default to GET if no method is specified
            return await GetAsync(
                GetEndpointFromContext(context),
                GetQueryParametersFromContext(context),
                GetHeadersFromContext(context),
                context,
                cancellationToken);
        }

        /// <summary>
        /// Gets the endpoint from the execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The endpoint.</returns>
        protected virtual string GetEndpointFromContext(ProtocolExecutionContext context)
        {
            return context.Parameters.TryGetValue("Endpoint", out var endpointObj) && endpointObj is string endpoint
                ? endpoint
                : string.Empty;
        }

        /// <summary>
        /// Gets the query parameters from the execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The query parameters.</returns>
        protected virtual IDictionary<string, string>? GetQueryParametersFromContext(ProtocolExecutionContext context)
        {
            return context.Parameters.TryGetValue("QueryParameters", out var queryParamsObj) && queryParamsObj is IDictionary<string, string> queryParams
                ? queryParams
                : null;
        }

        /// <summary>
        /// Gets the headers from the execution context.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <returns>The headers.</returns>
        protected virtual IDictionary<string, string>? GetHeadersFromContext(ProtocolExecutionContext context)
        {
            return context.Parameters.TryGetValue("Headers", out var headersObj) && headersObj is IDictionary<string, string> headers
                ? headers
                : null;
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
        public abstract Task<Result<object>> GetAsync(
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
        public abstract Task<Result<object>> PostAsync(
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
        public abstract Task<Result<object>> PutAsync(
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
        public abstract Task<Result<bool>> DeleteAsync(
            string endpoint,
            IDictionary<string, string>? headers,
            ProtocolExecutionContext context,
            CancellationToken cancellationToken = default);
    }
}
