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
    /// Manager for processor services in the Flow Architecture system.
    /// </summary>
    public class ProcessorServiceManager : AbstractServiceManager<IProcessorService>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessorServiceManager"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        public ProcessorServiceManager(string id, string name, string description, ILogger<ProcessorServiceManager> logger)
            : base(id, name, description, logger)
        {
        }

        /// <summary>
        /// Processes data using the specified processor service.
        /// </summary>
        /// <param name="processorService">The processor service to use.</param>
        /// <param name="parameters">The processing parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the processing.</returns>
        public async Task<ProcessingResult> ProcessAsync(IProcessorService processorService, ProcessParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default)
        {
            if (processorService == null)
            {
                return ProcessingResult.Failure("Processor service is null.");
            }

            if (parameters == null)
            {
                return ProcessingResult.Failure("Processing parameters are null.");
            }

            if (context == null)
            {
                context = new ExecutionContextType(cancellationToken: cancellationToken);
            }

            Logger.LogInformation("Processing data using processor {ProcessorId}...", processorService.Id);

            var result = await processorService.ProcessAsync(parameters, context, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully processed data using processor {ProcessorId}.", processorService.Id);
            }
            else
            {
                Logger.LogWarning("Failed to process data using processor {ProcessorId}: {ErrorMessage}",
                    processorService.Id, result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Processes data using a processor service with the specified ID.
        /// </summary>
        /// <param name="processorId">The processor ID.</param>
        /// <param name="parameters">The processing parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the processing.</returns>
        public async Task<ProcessingResult> ProcessByIdAsync(string processorId, ProcessParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default)
        {
            var processorService = await GetServiceAsync(processorId, "1.0", cancellationToken);

            if (processorService == null)
            {
                return ProcessingResult.Failure($"No processor service found with ID {processorId}.");
            }

            return await ProcessAsync(processorService, parameters, context, cancellationToken);
        }

        /// <summary>
        /// Gets a processor service by its input schema.
        /// </summary>
        /// <param name="schemaName">The schema name.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the processor service if found; otherwise, null.</returns>
        public async Task<IProcessorService> GetProcessorByInputSchemaAsync(string schemaName, CancellationToken cancellationToken = default)
        {
            var services = await GetAllServicesAsync(cancellationToken);

            foreach (var service in services)
            {
                var inputSchema = service.GetInputSchema();

                if (inputSchema != null && inputSchema.Name == schemaName)
                {
                    return service;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a processor service by its output schema.
        /// </summary>
        /// <param name="schemaName">The schema name.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the processor service if found; otherwise, null.</returns>
        public async Task<IProcessorService> GetProcessorByOutputSchemaAsync(string schemaName, CancellationToken cancellationToken = default)
        {
            var services = await GetAllServicesAsync(cancellationToken);

            foreach (var service in services)
            {
                var outputSchema = service.GetOutputSchema();

                if (outputSchema != null && outputSchema.Name == schemaName)
                {
                    return service;
                }
            }

            return null;
        }
    }
}
