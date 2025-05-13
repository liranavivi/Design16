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
    /// Base implementation for importer services in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractImporterService : AbstractServiceBase, IImporterService, IMessageConsumer<ImportCommand>
    {
        /// <summary>
        /// Gets the protocol identifier used by this importer.
        /// </summary>
        public abstract string ProtocolId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractImporterService"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        protected AbstractImporterService(string id, string name, string description, ILogger logger)
            : base(id, name, description, logger)
        {
        }

        /// <summary>
        /// Gets the capabilities of the protocol used by this importer.
        /// </summary>
        /// <returns>The protocol capabilities.</returns>
        public abstract ProtocolCapabilities GetProtocolCapabilities();

        /// <summary>
        /// Imports data from a source.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the import.</returns>
        public virtual async Task<ImportResult> ImportAsync(ImportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (Status != ServiceStatus.Running)
                {
                    return ImportResult.Failure($"Service is not running. Current status: {Status}");
                }

                if (parameters == null)
                {
                    return ImportResult.Failure("Import parameters cannot be null.");
                }

                if (context == null)
                {
                    context = new ExecutionContextType(cancellationToken: cancellationToken);
                }

                var validationResult = ValidateParameters(parameters);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors);
                    return ImportResult.Failure($"Invalid import parameters: {errors}");
                }

                Logger.LogInformation("Importing data from source {SourceId} using protocol {ProtocolId}...", parameters.SourceId, ProtocolId);

                var result = await OnImportAsync(parameters, context, cancellationToken);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Successfully imported data from source {SourceId} using protocol {ProtocolId}.", parameters.SourceId, ProtocolId);
                }
                else
                {
                    Logger.LogWarning("Failed to import data from source {SourceId} using protocol {ProtocolId}: {ErrorMessage}",
                        parameters.SourceId, ProtocolId, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error importing data from source {SourceId} using protocol {ProtocolId}: {ErrorMessage}",
                    parameters?.SourceId, ProtocolId, ex.Message);

                var errorDetails = GetErrorDetails(ex);
                return ImportResult.Failure(ex.Message, errorDetails);
            }
        }

        /// <summary>
        /// Validates the import parameters.
        /// </summary>
        /// <param name="parameters">The import parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        public virtual ValidationResult ValidateParameters(ImportParameters parameters)
        {
            var errors = new List<ValidationError>();

            if (parameters == null)
            {
                return ValidationResult.Failure("parameters", "Import parameters cannot be null.");
            }

            if (string.IsNullOrEmpty(parameters.SourceId))
            {
                errors.Add(new ValidationError("SourceId", "Source ID is required."));
            }

            return new ValidationResult(errors);
        }

        /// <summary>
        /// Determines whether this consumer can handle the specified message.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns><c>true</c> if this consumer can handle the message; otherwise, <c>false</c>.</returns>
        public virtual bool CanConsume(ImportCommand message)
        {
            return message != null &&
                   message.ProtocolId == ProtocolId &&
                   Status == ServiceStatus.Running;
        }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message to consume.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task ConsumeAsync(ImportCommand message, CancellationToken cancellationToken = default)
        {
            if (!CanConsume(message))
            {
                Logger.LogWarning("Cannot consume import command for protocol {ProtocolId}. Current status: {Status}", message?.ProtocolId, Status);
                return;
            }

            try
            {
                Logger.LogInformation("Consuming import command for source {SourceId} using protocol {ProtocolId}...", message.SourceId, ProtocolId);

                var parameters = new ImportParameters(message.SourceId, message.ProtocolParameters, message.AdditionalParameters);
                var context = new ExecutionContextType(message.CorrelationId, cancellationToken);

                var result = await ImportAsync(parameters, context, cancellationToken);

                await OnImportCompletedAsync(message, result, cancellationToken);

                Logger.LogInformation("Import command for source {SourceId} using protocol {ProtocolId} processed successfully.", message.SourceId, ProtocolId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error consuming import command for source {SourceId} using protocol {ProtocolId}: {ErrorMessage}",
                    message.SourceId, ProtocolId, ex.Message);

                await OnImportFailedAsync(message, ex, cancellationToken);
            }
        }

        /// <summary>
        /// Called when an import operation is being performed.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the import.</returns>
        protected abstract Task<ImportResult> OnImportAsync(ImportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken);

        /// <summary>
        /// Called when an import operation has completed successfully.
        /// </summary>
        /// <param name="command">The import command.</param>
        /// <param name="result">The import result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnImportCompletedAsync(ImportCommand command, ImportResult result, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when an import operation has failed.
        /// </summary>
        /// <param name="command">The import command.</param>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnImportFailedAsync(ImportCommand command, Exception exception, CancellationToken cancellationToken)
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
    /// Represents a command to import data.
    /// </summary>
    public class ImportCommand
    {
        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        public string SourceId { get; }

        /// <summary>
        /// Gets the protocol identifier.
        /// </summary>
        public string ProtocolId { get; }

        /// <summary>
        /// Gets the correlation identifier for tracking related operations.
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// Gets the protocol parameters.
        /// </summary>
        public IDictionary<string, object> ProtocolParameters { get; }

        /// <summary>
        /// Gets additional parameters for the import operation.
        /// </summary>
        public IDictionary<string, object> AdditionalParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportCommand"/> class.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        /// <param name="protocolId">The protocol identifier.</param>
        /// <param name="correlationId">The correlation identifier for tracking related operations.</param>
        /// <param name="protocolParameters">The protocol parameters.</param>
        /// <param name="additionalParameters">Additional parameters for the import operation.</param>
        public ImportCommand(
            string sourceId,
            string protocolId,
            string correlationId = null,
            IDictionary<string, object> protocolParameters = null,
            IDictionary<string, object> additionalParameters = null)
        {
            SourceId = sourceId;
            ProtocolId = protocolId;
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            ProtocolParameters = protocolParameters ?? new Dictionary<string, object>();
            AdditionalParameters = additionalParameters ?? new Dictionary<string, object>();
        }
    }
}
