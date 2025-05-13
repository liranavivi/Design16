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
    /// Manager for importer services in the Flow Architecture system.
    /// </summary>
    public class ImporterServiceManager : AbstractServiceManager<IImporterService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImporterServiceManager"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        public ImporterServiceManager(string id, string name, string description, ILogger<ImporterServiceManager> logger)
            : base(id, name, description, logger)
        {
        }

        /// <summary>
        /// Validates a service.
        /// </summary>
        /// <param name="service">The service to validate.</param>
        /// <returns>A validation result indicating whether the service is valid.</returns>
        public override ValidationResult ValidateService(IImporterService service)
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
        /// Gets an importer service by protocol ID.
        /// </summary>
        /// <param name="protocolId">The protocol ID.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the importer service if found; otherwise, null.</returns>
        public async Task<IImporterService> GetImporterByProtocolAsync(string protocolId, CancellationToken cancellationToken = default)
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
        /// Imports data using the specified importer service.
        /// </summary>
        /// <param name="importerService">The importer service to use.</param>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the import.</returns>
        public async Task<ImportResult> ImportAsync(IImporterService importerService, ImportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default)
        {
            if (importerService == null)
            {
                return ImportResult.Failure("Importer service is null.");
            }

            if (parameters == null)
            {
                return ImportResult.Failure("Import parameters are null.");
            }

            if (context == null)
            {
                context = new ExecutionContextType(cancellationToken: cancellationToken);
            }

            Logger.LogInformation("Importing data using importer {ImporterId} with protocol {ProtocolId}...", importerService.Id, importerService.ProtocolId);

            var result = await importerService.ImportAsync(parameters, context, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully imported data using importer {ImporterId} with protocol {ProtocolId}.", importerService.Id, importerService.ProtocolId);
            }
            else
            {
                Logger.LogWarning("Failed to import data using importer {ImporterId} with protocol {ProtocolId}: {ErrorMessage}",
                    importerService.Id, importerService.ProtocolId, result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Imports data using an importer service with the specified protocol ID.
        /// </summary>
        /// <param name="protocolId">The protocol ID.</param>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the import.</returns>
        public async Task<ImportResult> ImportByProtocolAsync(string protocolId, ImportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default)
        {
            var importerService = await GetImporterByProtocolAsync(protocolId, cancellationToken);

            if (importerService == null)
            {
                return ImportResult.Failure($"No importer service found for protocol {protocolId}.");
            }

            return await ImportAsync(importerService, parameters, context, cancellationToken);
        }
    }
}
