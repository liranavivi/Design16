using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Common.Services;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Services;

using ExecutionContextType = FlowArchitecture.Common.Services.ExecutionContext;

namespace FlowArchitecture.Services.Abstractions
{
    /// <summary>
    /// Interface for processor services in the Flow Architecture system.
    /// </summary>
    public interface IProcessorService : IService
    {
        /// <summary>
        /// Processes data.
        /// </summary>
        /// <param name="parameters">The processing parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the processing.</returns>
        Task<ProcessingResult> ProcessAsync(ProcessParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the input schema for the processor.
        /// </summary>
        /// <returns>The input schema.</returns>
        SchemaDefinition GetInputSchema();

        /// <summary>
        /// Gets the output schema for the processor.
        /// </summary>
        /// <returns>The output schema.</returns>
        SchemaDefinition GetOutputSchema();

        /// <summary>
        /// Validates the processing parameters.
        /// </summary>
        /// <param name="parameters">The processing parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        ValidationResult ValidateParameters(ProcessParameters parameters);

        /// <summary>
        /// Validates input data against the input schema.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <returns>A validation result indicating whether the data is valid.</returns>
        ValidationResult ValidateInputData(object data);
    }

    /// <summary>
    /// Represents parameters for a processing operation.
    /// </summary>
    public class ProcessParameters
    {
        /// <summary>
        /// Gets the input data to process.
        /// </summary>
        public object InputData { get; }

        /// <summary>
        /// Gets the processing options.
        /// </summary>
        public IDictionary<string, object> Options { get; }

        /// <summary>
        /// Gets additional parameters for the processing operation.
        /// </summary>
        public IDictionary<string, object> AdditionalParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessParameters"/> class.
        /// </summary>
        /// <param name="inputData">The input data to process.</param>
        /// <param name="options">The processing options.</param>
        /// <param name="additionalParameters">Additional parameters for the processing operation.</param>
        public ProcessParameters(
            object inputData,
            IDictionary<string, object> options = null,
            IDictionary<string, object> additionalParameters = null)
        {
            InputData = inputData;
            Options = options ?? new Dictionary<string, object>();
            AdditionalParameters = additionalParameters ?? new Dictionary<string, object>();
        }
    }
}
