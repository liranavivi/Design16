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
    /// Base implementation for exporter services in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractExporterService : AbstractServiceBase, IExporterService, IMessageConsumer<ExportCommand>
    {
        /// <summary>
        /// Gets the protocol identifier used by this exporter.
        /// </summary>
        public abstract string ProtocolId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractExporterService"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        protected AbstractExporterService(string id, string name, string description, ILogger logger)
            : base(id, name, description, logger)
        {
        }

        /// <summary>
        /// Gets the capabilities of the protocol used by this exporter.
        /// </summary>
        /// <returns>The protocol capabilities.</returns>
        public abstract ProtocolCapabilities GetProtocolCapabilities();

        /// <summary>
        /// Gets the merge capabilities of this exporter.
        /// </summary>
        /// <returns>The merge capabilities.</returns>
        public abstract MergeCapabilities GetMergeCapabilities();

        /// <summary>
        /// Exports data to a destination.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        public virtual async Task<ExportResult> ExportAsync(ExportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (Status != ServiceStatus.Running)
                {
                    return ExportResult.Failure($"Service is not running. Current status: {Status}");
                }

                if (parameters == null)
                {
                    return ExportResult.Failure("Export parameters cannot be null.");
                }

                if (context == null)
                {
                    context = new ExecutionContextType(cancellationToken: cancellationToken);
                }

                var validationResult = ValidateParameters(parameters);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors);
                    return ExportResult.Failure($"Invalid export parameters: {errors}");
                }

                Logger.LogInformation("Exporting data to destination {DestinationId} using protocol {ProtocolId}...", parameters.DestinationId, ProtocolId);

                var result = await OnExportAsync(parameters, context, cancellationToken);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Successfully exported data to destination {DestinationId} using protocol {ProtocolId}.", parameters.DestinationId, ProtocolId);
                }
                else
                {
                    Logger.LogWarning("Failed to export data to destination {DestinationId} using protocol {ProtocolId}: {ErrorMessage}",
                        parameters.DestinationId, ProtocolId, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error exporting data to destination {DestinationId} using protocol {ProtocolId}: {ErrorMessage}",
                    parameters?.DestinationId, ProtocolId, ex.Message);

                var errorDetails = GetErrorDetails(ex);
                return ExportResult.Failure(ex.Message, errorDetails);
            }
        }

        /// <summary>
        /// Merges data from multiple branches and exports it to a destination.
        /// </summary>
        /// <param name="branchData">The data from multiple branches.</param>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="strategy">The merge strategy.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        public virtual async Task<ExportResult> MergeAndExportAsync(
            IDictionary<string, object> branchData,
            ExportParameters parameters,
            MergeStrategy strategy,
            ExecutionContextType context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (Status != ServiceStatus.Running)
                {
                    return ExportResult.Failure($"Service is not running. Current status: {Status}");
                }

                if (branchData == null || branchData.Count == 0)
                {
                    return ExportResult.Failure("Branch data cannot be null or empty.");
                }

                if (parameters == null)
                {
                    return ExportResult.Failure("Export parameters cannot be null.");
                }

                if (context == null)
                {
                    context = new ExecutionContextType(cancellationToken: cancellationToken);
                }

                var mergeCapabilities = GetMergeCapabilities();
                if (!mergeCapabilities.SupportsMerging)
                {
                    return ExportResult.Failure("This exporter does not support merging.");
                }

                if (!mergeCapabilities.SupportedStrategies.Contains(strategy))
                {
                    return ExportResult.Failure($"Merge strategy {strategy} is not supported by this exporter.");
                }

                Logger.LogInformation("Merging and exporting data to destination {DestinationId} using protocol {ProtocolId} with strategy {Strategy}...",
                    parameters.DestinationId, ProtocolId, strategy);

                var mergedData = await OnMergeAsync(branchData, strategy, context, cancellationToken);

                var exportParameters = new ExportParameters(
                    parameters.DestinationId,
                    mergedData,
                    parameters.ProtocolParameters,
                    parameters.AdditionalParameters);

                var result = await ExportAsync(exportParameters, context, cancellationToken);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Successfully merged and exported data to destination {DestinationId} using protocol {ProtocolId} with strategy {Strategy}.",
                        parameters.DestinationId, ProtocolId, strategy);
                }
                else
                {
                    Logger.LogWarning("Failed to merge and export data to destination {DestinationId} using protocol {ProtocolId} with strategy {Strategy}: {ErrorMessage}",
                        parameters.DestinationId, ProtocolId, strategy, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error merging and exporting data to destination {DestinationId} using protocol {ProtocolId} with strategy {Strategy}: {ErrorMessage}",
                    parameters?.DestinationId, ProtocolId, strategy, ex.Message);

                var errorDetails = GetErrorDetails(ex);
                return ExportResult.Failure(ex.Message, errorDetails);
            }
        }

        /// <summary>
        /// Validates the export parameters.
        /// </summary>
        /// <param name="parameters">The export parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        public virtual ValidationResult ValidateParameters(ExportParameters parameters)
        {
            var errors = new List<ValidationError>();

            if (parameters == null)
            {
                return ValidationResult.Failure("parameters", "Export parameters cannot be null.");
            }

            if (string.IsNullOrEmpty(parameters.DestinationId))
            {
                errors.Add(new ValidationError("DestinationId", "Destination ID is required."));
            }

            if (parameters.Data == null)
            {
                errors.Add(new ValidationError("Data", "Data is required."));
            }

            return new ValidationResult(errors);
        }

        /// <summary>
        /// Determines whether this consumer can handle the specified message.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns><c>true</c> if this consumer can handle the message; otherwise, <c>false</c>.</returns>
        public virtual bool CanConsume(ExportCommand message)
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
        public virtual async Task ConsumeAsync(ExportCommand message, CancellationToken cancellationToken = default)
        {
            if (!CanConsume(message))
            {
                Logger.LogWarning("Cannot consume export command for protocol {ProtocolId}. Current status: {Status}", message?.ProtocolId, Status);
                return;
            }

            try
            {
                Logger.LogInformation("Consuming export command for destination {DestinationId} using protocol {ProtocolId}...", message.DestinationId, ProtocolId);

                var parameters = new ExportParameters(message.DestinationId, message.Data, message.ProtocolParameters, message.AdditionalParameters);
                var context = new ExecutionContextType(message.CorrelationId, cancellationToken);

                ExportResult result;

                if (message.BranchData != null && message.BranchData.Count > 0)
                {
                    result = await MergeAndExportAsync(message.BranchData, parameters, message.MergeStrategy, context, cancellationToken);
                }
                else
                {
                    result = await ExportAsync(parameters, context, cancellationToken);
                }

                await OnExportCompletedAsync(message, result, cancellationToken);

                Logger.LogInformation("Export command for destination {DestinationId} using protocol {ProtocolId} processed successfully.", message.DestinationId, ProtocolId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error consuming export command for destination {DestinationId} using protocol {ProtocolId}: {ErrorMessage}",
                    message.DestinationId, ProtocolId, ex.Message);

                await OnExportFailedAsync(message, ex, cancellationToken);
            }
        }

        /// <summary>
        /// Called when an export operation is being performed.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        protected abstract Task<ExportResult> OnExportAsync(ExportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken);

        /// <summary>
        /// Called when a merge operation is being performed.
        /// </summary>
        /// <param name="branchData">The data from multiple branches.</param>
        /// <param name="strategy">The merge strategy.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the merged data.</returns>
        protected abstract Task<object> OnMergeAsync(IDictionary<string, object> branchData, MergeStrategy strategy, ExecutionContextType context, CancellationToken cancellationToken);

        /// <summary>
        /// Called when an export operation has completed successfully.
        /// </summary>
        /// <param name="command">The export command.</param>
        /// <param name="result">The export result.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnExportCompletedAsync(ExportCommand command, ExportResult result, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when an export operation has failed.
        /// </summary>
        /// <param name="command">The export command.</param>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected virtual Task OnExportFailedAsync(ExportCommand command, Exception exception, CancellationToken cancellationToken)
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
    /// Represents a command to export data.
    /// </summary>
    public class ExportCommand
    {
        /// <summary>
        /// Gets the destination identifier.
        /// </summary>
        public string DestinationId { get; }

        /// <summary>
        /// Gets the protocol identifier.
        /// </summary>
        public string ProtocolId { get; }

        /// <summary>
        /// Gets the data to export.
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Gets the correlation identifier for tracking related operations.
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// Gets the protocol parameters.
        /// </summary>
        public IDictionary<string, object> ProtocolParameters { get; }

        /// <summary>
        /// Gets additional parameters for the export operation.
        /// </summary>
        public IDictionary<string, object> AdditionalParameters { get; }

        /// <summary>
        /// Gets the data from multiple branches for merging.
        /// </summary>
        public IDictionary<string, object> BranchData { get; }

        /// <summary>
        /// Gets the merge strategy.
        /// </summary>
        public MergeStrategy MergeStrategy { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportCommand"/> class.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <param name="protocolId">The protocol identifier.</param>
        /// <param name="data">The data to export.</param>
        /// <param name="correlationId">The correlation identifier for tracking related operations.</param>
        /// <param name="protocolParameters">The protocol parameters.</param>
        /// <param name="additionalParameters">Additional parameters for the export operation.</param>
        /// <param name="branchData">The data from multiple branches for merging.</param>
        /// <param name="mergeStrategy">The merge strategy.</param>
        public ExportCommand(
            string destinationId,
            string protocolId,
            object data,
            string correlationId = null,
            IDictionary<string, object> protocolParameters = null,
            IDictionary<string, object> additionalParameters = null,
            IDictionary<string, object> branchData = null,
            MergeStrategy mergeStrategy = MergeStrategy.None)
        {
            DestinationId = destinationId;
            ProtocolId = protocolId;
            Data = data;
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            ProtocolParameters = protocolParameters ?? new Dictionary<string, object>();
            AdditionalParameters = additionalParameters ?? new Dictionary<string, object>();
            BranchData = branchData;
            MergeStrategy = mergeStrategy;
        }
    }
}
