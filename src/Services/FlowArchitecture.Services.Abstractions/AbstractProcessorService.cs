using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Common.Services;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Messaging;
using FlowArchitecture.Core.Services;
using Microsoft.Extensions.Logging;

using ExecutionContextType = FlowArchitecture.Common.Services.ExecutionContext;

namespace FlowArchitecture.Services.Abstractions
{
    /// <summary>
    /// Base implementation for processor services in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractProcessorService : AbstractServiceBase, IProcessorService, IMessageConsumer<ProcessCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractProcessorService"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        protected AbstractProcessorService(string id, string name, string description, ILogger logger)
            : base(id, name, description, logger)
        {
        }

        /// <summary>
        /// Processes data.
        /// </summary>
        /// <param name="parameters">The processing parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the processing.</returns>
        public virtual async Task<ProcessingResult> ProcessAsync(ProcessParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (Status != ServiceStatus.Running)
                {
                    return ProcessingResult.Failure($"Service is not running. Current status: {Status}");
                }

                if (parameters == null)
                {
                    return ProcessingResult.Failure("Processing parameters cannot be null.");
                }

                if (context == null)
                {
                    context = new ExecutionContextType(cancellationToken: cancellationToken);
                }

                var validationResult = ValidateParameters(parameters);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors);
                    return ProcessingResult.Failure($"Invalid processing parameters: {errors}");
                }

                var inputValidationResult = ValidateInputData(parameters.InputData);
                if (!inputValidationResult.IsValid)
                {
                    var errors = string.Join(", ", inputValidationResult.Errors);
                    return ProcessingResult.Failure($"Invalid input data: {errors}");
                }

                Logger.LogInformation("Processing data with processor {ProcessorId}...", Id);

                var result = await OnProcessAsync(parameters, context, cancellationToken);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Successfully processed data with processor {ProcessorId}.", Id);
                }
                else
                {
                    Logger.LogWarning("Failed to process data with processor {ProcessorId}: {ErrorMessage}", Id, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing data with processor {ProcessorId}: {ErrorMessage}", Id, ex.Message);

                var errorDetails = GetErrorDetails(ex);
                return ProcessingResult.Failure(ex.Message, errorDetails);
            }
        }

        /// <summary>
        /// Gets the input schema for the processor.
        /// </summary>
        /// <returns>The input schema.</returns>
        public abstract SchemaDefinition GetInputSchema();

        /// <summary>
        /// Gets the output schema for the processor.
        /// </summary>
        /// <returns>The output schema.</returns>
        public abstract SchemaDefinition GetOutputSchema();

        /// <summary>
        /// Validates the processing parameters.
        /// </summary>
        /// <param name="parameters">The processing parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        public virtual ValidationResult ValidateParameters(ProcessParameters parameters)
        {
            var errors = new List<ValidationError>();

            if (parameters == null)
            {
                return ValidationResult.Failure("parameters", "Processing parameters cannot be null.");
            }

            if (parameters.InputData == null)
            {
                errors.Add(new ValidationError("InputData", "Input data is required."));
            }

            return new ValidationResult(errors);
        }

        /// <summary>
        /// Validates input data against the input schema.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <returns>A validation result indicating whether the data is valid.</returns>
        public virtual ValidationResult ValidateInputData(object data)
        {
            if (data == null)
            {
                return ValidationResult.Failure("data", "Input data cannot be null.");
            }

            return ValidateDataAgainstSchema(data, GetInputSchema());
        }

        /// <summary>
        /// Validates output data against the output schema.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <returns>A validation result indicating whether the data is valid.</returns>
        protected virtual ValidationResult ValidateOutputData(object data)
        {
            if (data == null)
            {
                return ValidationResult.Failure("data", "Output data cannot be null.");
            }

            return ValidateDataAgainstSchema(data, GetOutputSchema());
        }

        /// <summary>
        /// Validates data against a schema.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <param name="schema">The schema to validate against.</param>
        /// <returns>A validation result indicating whether the data is valid.</returns>
        protected abstract ValidationResult ValidateDataAgainstSchema(object data, SchemaDefinition schema);

        /// <summary>
        /// Determines whether this consumer can handle the specified message.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns><c>true</c> if this consumer can handle the message; otherwise, <c>false</c>.</returns>
        public virtual bool CanConsume(ProcessCommand message)
        {
            return message != null &&
                   message.ProcessorId == Id &&
                   Status == ServiceStatus.Running;
        }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message to consume.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task ConsumeAsync(ProcessCommand message, CancellationToken cancellationToken = default)
        {
            if (!CanConsume(message))
            {
                Logger.LogWarning("Cannot consume process command for processor {ProcessorId}. Current status: {Status}", message?.ProcessorId, Status);
                return;
            }

            try
            {
                Logger.LogInformation("Consuming process command for processor {ProcessorId}...", Id);

                var parameters = new ProcessParameters(message.InputData, message.Options, message.AdditionalParameters);
                var context = new ExecutionContextType(message.CorrelationId, cancellationToken);

                var result = await ProcessAsync(parameters, context, cancellationToken);

                await OnProcessCompletedAsync(message, result, cancellationToken);

                Logger.LogInformation("Process command for processor {ProcessorId} processed successfully.", Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error consuming process command for processor {ProcessorId}: {ErrorMessage}", Id, ex.Message);

                await OnProcessFailedAsync(message, ex, cancellationToken);
            }
        }

        /// <summary>
        /// Called when a processing operation is being performed.
        /// </summary>
        /// <param name="parameters">The processing parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the processing.</returns>
        protected abstract Task<ProcessingResult> OnProcessAsync(ProcessParameters parameters, ExecutionContextType context, CancellationToken cancellationToken);

        /// <summary>
        /// Called when a processing operation has completed successfully.
        /// </summary>
        /// <param name="command">The process command.</param>
        /// <param name="result">The processing result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnProcessCompletedAsync(ProcessCommand command, ProcessingResult result, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when a processing operation has failed.
        /// </summary>
        /// <param name="command">The process command.</param>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnProcessFailedAsync(ProcessCommand command, Exception exception, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets additional details about an error.
        /// </summary>
        /// <param name="exception">The exception that caused the error.</param>
        /// <returns>A dictionary containing additional details about the error.</returns>
        protected virtual IDictionary<string, object> GetErrorDetails(Exception exception)
        {
            return new Dictionary<string, object>
            {
                { "ExceptionType", exception.GetType().Name },
                { "StackTrace", exception.StackTrace }
            };
        }
    }

    /// <summary>
    /// Represents a command to process data.
    /// </summary>
    public class ProcessCommand
    {
        /// <summary>
        /// Gets the processor identifier.
        /// </summary>
        public string ProcessorId { get; }

        /// <summary>
        /// Gets the input data to process.
        /// </summary>
        public object InputData { get; }

        /// <summary>
        /// Gets the correlation identifier for tracking related operations.
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// Gets the processing options.
        /// </summary>
        public IDictionary<string, object> Options { get; }

        /// <summary>
        /// Gets additional parameters for the processing operation.
        /// </summary>
        public IDictionary<string, object> AdditionalParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessCommand"/> class.
        /// </summary>
        /// <param name="processorId">The processor identifier.</param>
        /// <param name="inputData">The input data to process.</param>
        /// <param name="correlationId">The correlation identifier for tracking related operations.</param>
        /// <param name="options">The processing options.</param>
        /// <param name="additionalParameters">Additional parameters for the processing operation.</param>
        public ProcessCommand(
            string processorId,
            object inputData,
            string correlationId = null,
            IDictionary<string, object> options = null,
            IDictionary<string, object> additionalParameters = null)
        {
            ProcessorId = processorId;
            InputData = inputData;
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            Options = options ?? new Dictionary<string, object>();
            AdditionalParameters = additionalParameters ?? new Dictionary<string, object>();
        }
    }
}
