using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;
using FlowArchitecture.Protocols.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlowArchitecture.Tests.Unit.Protocols
{
    public class ProtocolTests
    {
        [Fact]
        public void FileProtocol_Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FileProtocol>>();

            // Act
            var protocol = new FileProtocol(loggerMock.Object);

            // Assert
            Assert.Equal("file", protocol.Id);
            Assert.Equal("File Protocol", protocol.Name);
            Assert.Equal("1.0", protocol.Version);
            Assert.Equal("Protocol for reading and writing files.", protocol.Description);
        }

        [Fact]
        public void RestProtocol_Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RestProtocol>>();
            var httpClient = new HttpClient();

            // Act
            var protocol = new RestProtocol(loggerMock.Object, httpClient);

            // Assert
            Assert.Equal("rest", protocol.Id);
            Assert.Equal("REST Protocol", protocol.Name);
            Assert.Equal("1.0", protocol.Version);
            Assert.Equal("Protocol for interacting with REST APIs.", protocol.Description);
        }

        [Fact]
        public void DatabaseProtocol_Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DatabaseProtocol>>();

            // Act
            var protocol = new DatabaseProtocol(loggerMock.Object);

            // Assert
            Assert.Equal("database", protocol.Id);
            Assert.Equal("Database Protocol", protocol.Name);
            Assert.Equal("1.0", protocol.Version);
            Assert.Equal("Protocol for interacting with databases.", protocol.Description);
        }

        [Fact]
        public void FileProtocol_Parameters_ShouldIncludeFilePath()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<FileProtocol>>();
            var protocol = new FileProtocol(loggerMock.Object);

            // Act
            var parameters = protocol.Parameters;

            // Assert
            Assert.Contains(parameters, p => p.Name == "FilePath");
        }

        [Fact]
        public async Task RestProtocol_InitializeAsync_WithValidParameters_ShouldSucceed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RestProtocol>>();
            var httpClient = new HttpClient();
            var protocol = new RestProtocol(loggerMock.Object, httpClient);
            var parameters = new Dictionary<string, object>
            {
                { "BaseUrl", "https://api.example.com" },
                { "AuthenticationType", "Bearer" },
                { "ApiKey", "test-api-key" }
            };

            // Act
            var result = await protocol.InitializeAsync(parameters);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task DatabaseProtocol_InitializeAsync_WithValidParameters_ShouldSucceed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DatabaseProtocol>>();
            var protocol = new DatabaseProtocol(loggerMock.Object);
            var parameters = new Dictionary<string, object>
            {
                { "ConnectionString", "Server=localhost;Database=test;User=test;Password=test;" },
                { "DatabaseType", "SQL Server" }
            };

            // Act
            var result = await protocol.InitializeAsync(parameters);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ProtocolParameter_Constructor_ShouldInitializeProperties()
        {
            // Arrange & Act
            var parameter = new ProtocolParameter(
                "TestParam",
                typeof(string),
                true,
                "Test Parameter",
                "A test parameter",
                "default");

            // Assert
            Assert.Equal("TestParam", parameter.Name);
            Assert.Equal("Test Parameter", parameter.DisplayName);
            Assert.Equal("A test parameter", parameter.Description);
            Assert.Equal(typeof(string), parameter.ParameterType);
            Assert.True(parameter.IsRequired);
            Assert.Equal("default", parameter.DefaultValue);
        }

        [Fact]
        public void ProtocolParameter_Constructor_WithMinimalParameters_ShouldInitializeProperties()
        {
            // Arrange & Act
            var parameter = new ProtocolParameter(
                "TestParam",
                typeof(string),
                false);

            // Assert
            Assert.Equal("TestParam", parameter.Name);
            Assert.Equal("TestParam", parameter.DisplayName);
            Assert.Null(parameter.Description);
            Assert.Equal(typeof(string), parameter.ParameterType);
            Assert.False(parameter.IsRequired);
            Assert.Null(parameter.DefaultValue);
        }
    }
}
