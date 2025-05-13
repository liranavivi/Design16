using System;
using System.Collections.Generic;
using FlowArchitecture.Core.Common;
using Xunit;

namespace FlowArchitecture.Tests.Unit.Core
{
    public class ErrorTests
    {
        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateError()
        {
            // Arrange
            var code = "ERR001";
            var message = "Error message";
            var details = new Dictionary<string, object> { { "Key1", "Value1" } };
            
            // Act
            var error = new Error(code, message, details);
            
            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(message, error.Message);
            Assert.Equal(details, error.Details);
        }
        
        [Fact]
        public void Constructor_WithNullCode_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Error(null, "Error message"));
        }
        
        [Fact]
        public void Constructor_WithNullMessage_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new Error("ERR001", null));
        }
        
        [Fact]
        public void Constructor_WithNullDetails_ShouldCreateEmptyDetails()
        {
            // Act
            var error = new Error("ERR001", "Error message");
            
            // Assert
            Assert.NotNull(error.Details);
            Assert.Empty(error.Details);
        }
        
        [Fact]
        public void Create_WithCodeAndMessage_ShouldCreateError()
        {
            // Arrange
            var code = "ERR001";
            var message = "Error message";
            
            // Act
            var error = Error.Create(code, message);
            
            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(message, error.Message);
            Assert.Empty(error.Details);
        }
        
        [Fact]
        public void Create_WithCodeMessageAndDetails_ShouldCreateError()
        {
            // Arrange
            var code = "ERR001";
            var message = "Error message";
            var details = new Dictionary<string, object> { { "Key1", "Value1" } };
            
            // Act
            var error = Error.Create(code, message, details);
            
            // Assert
            Assert.Equal(code, error.Code);
            Assert.Equal(message, error.Message);
            Assert.Equal(details, error.Details);
        }
    }
}
