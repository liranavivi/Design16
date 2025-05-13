using System.Linq;
using FlowArchitecture.Core.Common;
using Xunit;

namespace FlowArchitecture.Tests.Unit.Core
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldCreateSuccessResult()
        {
            // Arrange
            var value = "Test value";
            
            // Act
            var result = Result<string>.Success(value);
            
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(value, result.Value);
            Assert.Empty(result.Errors);
        }
        
        [Fact]
        public void Failure_WithErrors_ShouldCreateFailureResult()
        {
            // Arrange
            var error1 = new Error("ERR001", "Error message 1");
            var error2 = new Error("ERR002", "Error message 2");
            
            // Act
            var result = Result<string>.Failure(error1, error2);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.Code == "ERR001" && e.Message == "Error message 1");
            Assert.Contains(result.Errors, e => e.Code == "ERR002" && e.Message == "Error message 2");
        }
        
        [Fact]
        public void Failure_WithCodeAndMessage_ShouldCreateFailureResult()
        {
            // Act
            var result = Result<string>.Failure("ERR001", "Error message");
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Null(result.Value);
            Assert.Single(result.Errors);
            Assert.Equal("ERR001", result.Errors.First().Code);
            Assert.Equal("Error message", result.Errors.First().Message);
        }
    }
}
