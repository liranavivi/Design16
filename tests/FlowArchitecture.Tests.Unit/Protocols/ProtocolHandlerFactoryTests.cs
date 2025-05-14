using System.Collections.Generic;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Protocols.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace FlowArchitecture.Tests.Unit.Protocols
{
    public class ProtocolHandlerFactoryTests
    {
        private readonly Mock<ILogger<ProtocolHandlerFactory>> _loggerMock;
        private readonly Mock<ILoggerFactory> _loggerFactoryMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly ProtocolHandlerFactory _factory;

        public ProtocolHandlerFactoryTests()
        {
            _loggerMock = new Mock<ILogger<ProtocolHandlerFactory>>();
            _loggerFactoryMock = new Mock<ILoggerFactory>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            // Setup logger factory to return mock loggers
            _loggerFactoryMock
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(Mock.Of<ILogger>());

            // Create a real HttpClient for testing
            var httpClient = new System.Net.Http.HttpClient();

            // Create a wrapper for the IHttpClientFactory
            var httpClientFactoryWrapper = new HttpClientFactoryWrapper(_httpClientFactoryMock.Object);

            _factory = new ProtocolHandlerFactory(
                _loggerMock.Object,
                _loggerFactoryMock.Object,
                httpClientFactoryWrapper);
        }

        [Fact]
        public async Task CreateProtocolAsync_WithUnknownProtocolId_ShouldFail()
        {
            // Arrange
            var parameters = new Dictionary<string, object>();

            // Act
            var result = await _factory.CreateProtocolAsync("unknown", parameters);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal("PROTOCOL_FACTORY_ERROR", result.Errors[0].Code);
        }
    }
}
