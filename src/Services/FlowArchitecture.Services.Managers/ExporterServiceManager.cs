using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Common.Services;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Services.Abstractions;
using Microsoft.Extensions.Logging;

using ExecutionContextType = FlowArchitecture.Common.Services.ExecutionContext;

namespace FlowArchitecture.Services.Managers
{
    /// <summary>
    /// Manager for exporter services in the Flow Architecture system.
    /// </summary>
    public class ExporterServiceManager : AbstractServiceManager<IExporterService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterServiceManager"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        public ExporterServiceManager(string id, string name, string description, ILogger<ExporterServiceManager> logger)
            : base(id, name, description, logger)
        {
        }

        /// <summary>
        /// Validates a service.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>A validation result indicating whether the service is valid.</returns>
        public override ValidationResult ValidateService(IExporterService service)
        {
            var baseResult = base.ValidateService(service);
            var errors = new List<ValidationError>(baseResult.Errors);

            if (string.IsNullOrEmpty(service.ProtocolId))
            {
                errors.Add(new ValidationError("ProtocolId", "Protocol ID is required."));
            }

            return new ValidationResult(errors);
        }

        /// <summary>
        /// Gets an exporter service by protocol ID.
        /// </summary>
        /// <param name="protocolId">The protocol ID.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the exporter service if found; otherwise, null.</returns>
        public async Task<IExporterService> GetExporterByProtocolAsync(string protocolId, CancellationToken cancellationToken = default)
        {
            var services = await GetAllServicesAsync(cancellationToken);

            foreach (var service in services)
            {
                if (service.ProtocolId == protocolId)
                {
                    return service;
                }
            }

            return null;
        }

        /// <summary>
        /// Exports data using the specified exporter service.
        /// </summary>
        /// <param name="exporterService">The exporter service to use.</param>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        public async Task<ExportResult> ExportAsync(IExporterService exporterService, ExportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default)
        {
            if (exporterService == null)
            {
                return ExportResult.Failure("Exporter service is null.");
            }

            if (parameters == null)
            {
                return ExportResult.Failure("Export parameters are null.");
            }

            if (context == null)
            {
                context = new ExecutionContextType(cancellationToken: cancellationToken);
            }

            Logger.LogInformation("Exporting data using exporter {ExporterId} with protocol {ProtocolId}...", exporterService.Id, exporterService.ProtocolId);

            var result = await exporterService.ExportAsync(parameters, context, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully exported data using exporter {ExporterId} with protocol {ProtocolId}.", exporterService.Id, exporterService.ProtocolId);
            }
            else
            {
                Logger.LogWarning("Failed to export data using exporter {ExporterId} with protocol {ProtocolId}: {ErrorMessage}",
                    exporterService.Id, exporterService.ProtocolId, result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Exports data using an exporter service with the specified protocol ID.
        /// </summary>
        /// <param name="protocolId">The protocol ID.</param>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        public async Task<ExportResult> ExportByProtocolAsync(string protocolId, ExportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default)
        {
            var exporterService = await GetExporterByProtocolAsync(protocolId, cancellationToken);

            if (exporterService == null)
            {
                return ExportResult.Failure($"No exporter service found for protocol {protocolId}.");
            }

            return await ExportAsync(exporterService, parameters, context, cancellationToken);
        }

        /// <summary>
        /// Merges data from multiple branches and exports it using the specified exporter service.
        /// </summary>
        /// <param name="exporterService">The exporter service to use.</param>
        /// <param name="branchData">The data from multiple branches.</param>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="strategy">The merge strategy.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        public async Task<ExportResult> MergeAndExportAsync(
            IExporterService exporterService,
            IDictionary<string, object> branchData,
            ExportParameters parameters,
            MergeStrategy strategy,
            ExecutionContextType context,
            CancellationToken cancellationToken = default)
        {
            if (exporterService == null)
            {
                return ExportResult.Failure("Exporter service is null.");
            }

            if (branchData == null || branchData.Count == 0)
            {
                return ExportResult.Failure("Branch data is null or empty.");
            }

            if (parameters == null)
            {
                return ExportResult.Failure("Export parameters are null.");
            }

            if (context == null)
            {
                context = new ExecutionContextType(cancellationToken: cancellationToken);
            }

            Logger.LogInformation("Merging and exporting data using exporter {ExporterId} with protocol {ProtocolId} and strategy {Strategy}...",
                exporterService.Id, exporterService.ProtocolId, strategy);

            var result = await exporterService.MergeAndExportAsync(branchData, parameters, strategy, context, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully merged and exported data using exporter {ExporterId} with protocol {ProtocolId} and strategy {Strategy}.",
                    exporterService.Id, exporterService.ProtocolId, strategy);
            }
            else
            {
                Logger.LogWarning("Failed to merge and export data using exporter {ExporterId} with protocol {ProtocolId} and strategy {Strategy}: {ErrorMessage}",
                    exporterService.Id, exporterService.ProtocolId, strategy, result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Merges data from multiple branches and exports it using an exporter service with the specified protocol ID.
        /// </summary>
        /// <param name="protocolId">The protocol ID.</param>
        /// <param name="branchData">The data from multiple branches.</param>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="strategy">The merge strategy.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        public async Task<ExportResult> MergeAndExportByProtocolAsync(
            string protocolId,
            IDictionary<string, object> branchData,
            ExportParameters parameters,
            MergeStrategy strategy,
            ExecutionContextType context,
            CancellationToken cancellationToken = default)
        {
            var exporterService = await GetExporterByProtocolAsync(protocolId, cancellationToken);

            if (exporterService == null)
            {
                return ExportResult.Failure($"No exporter service found for protocol {protocolId}.");
            }

            return await MergeAndExportAsync(exporterService, branchData, parameters, strategy, context, cancellationToken);
        }
    }
}
