using System;
using System.Collections.Generic;
using FlowArchitecture.Core.Protocols;
using Xunit;

namespace FlowArchitecture.Tests.Unit.Protocols
{
    public class ProtocolExecutionContextTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var inputData = "Test input data";
            var parameters = new Dictionary<string, object>
            {
                { "param1", "value1" },
                { "param2", 123 }
            };
            
            // Act
            var context = new ProtocolExecutionContext(inputData, parameters);
            
            // Assert
            Assert.NotNull(context.Id);
            Assert.Equal(inputData, context.InputData);
            Assert.Equal(parameters, context.Parameters);
            Assert.NotEqual(default, context.StartTime);
            Assert.Null(context.EndTime);
            Assert.NotNull(context.State);
            Assert.Empty(context.State);
        }
        
        [Fact]
        public void Constructor_WithNullParameters_ShouldInitializeEmptyParameters()
        {
            // Arrange
            var inputData = "Test input data";
            
            // Act
            var context = new ProtocolExecutionContext(inputData, null);
            
            // Assert
            Assert.NotNull(context.Parameters);
            Assert.Empty(context.Parameters);
        }
        
        [Fact]
        public void EndTime_ShouldBeSettable()
        {
            // Arrange
            var context = new ProtocolExecutionContext("Test", new Dictionary<string, object>());
            var endTime = DateTime.UtcNow.AddSeconds(10);
            
            // Act
            context.EndTime = endTime;
            
            // Assert
            Assert.Equal(endTime, context.EndTime);
        }
        
        [Fact]
        public void State_ShouldBeSettable()
        {
            // Arrange
            var context = new ProtocolExecutionContext("Test", new Dictionary<string, object>());
            var state = new Dictionary<string, object>
            {
                { "key1", "value1" },
                { "key2", 123 }
            };
            
            // Act
            context.State = state;
            
            // Assert
            Assert.Equal(state, context.State);
        }
    }
}
