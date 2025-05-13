using System.Linq;
using FlowArchitecture.Core.Common;
using Xunit;

namespace FlowArchitecture.Tests.Unit.Core
{
    public class ValidationResultTests
    {
        [Fact]
        public void Success_ShouldCreateValidResult()
        {
            // Act
            var result = ValidationResult.Success();
            
            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }
        
        [Fact]
        public void Failure_WithErrors_ShouldCreateInvalidResult()
        {
            // Arrange
            var error1 = new ValidationError("Property1", "Error message 1");
            var error2 = new ValidationError("Property2", "Error message 2");
            
            // Act
            var result = ValidationResult.Failure(error1, error2);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.PropertyName == "Property1" && e.ErrorMessage == "Error message 1");
            Assert.Contains(result.Errors, e => e.PropertyName == "Property2" && e.ErrorMessage == "Error message 2");
        }
        
        [Fact]
        public void Failure_WithPropertyAndMessage_ShouldCreateInvalidResult()
        {
            // Act
            var result = ValidationResult.Failure("PropertyName", "Error message");
            
            // Assert
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Equal("PropertyName", result.Errors.First().PropertyName);
            Assert.Equal("Error message", result.Errors.First().ErrorMessage);
        }
    }
}
