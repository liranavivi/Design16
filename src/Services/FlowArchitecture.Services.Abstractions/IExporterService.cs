using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Common.Services;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Services;

using ExecutionContextType = FlowArchitecture.Common.Services.ExecutionContext;

namespace FlowArchitecture.Services.Abstractions
{
    /// <summary>
    /// Interface for exporter services in the Flow Architecture system.
    /// </summary>
    public interface IExporterService : IService
    {
        /// <summary>
        /// Gets the protocol identifier used by this exporter.
        /// </summary>
        string ProtocolId { get; }

        /// <summary>
        /// Gets the capabilities of the protocol used by this exporter.
        /// </summary>
        /// <returns>The protocol capabilities.</returns>
        ProtocolCapabilities GetProtocolCapabilities();

        /// <summary>
        /// Gets the merge capabilities of this exporter.
        /// </summary>
        /// <returns>The merge capabilities.</returns>
        MergeCapabilities GetMergeCapabilities();

        /// <summary>
        /// Exports data to a destination.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        Task<ExportResult> ExportAsync(ExportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Merges data from multiple branches and exports it to a destination.
        /// </summary>
        /// <param name="branchData">The data from multiple branches.</param>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="strategy">The merge strategy.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        Task<ExportResult> MergeAndExportAsync(
            IDictionary<string, object> branchData,
            ExportParameters parameters,
            MergeStrategy strategy,
            ExecutionContextType context,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the export parameters.
        /// </summary>
        /// <param name="parameters">The export parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        ValidationResult ValidateParameters(ExportParameters parameters);
    }

    /// <summary>
    /// Represents parameters for an export operation.
    /// </summary>
    public class ExportParameters
    {
        /// <summary>
        /// Gets the destination identifier.
        /// </summary>
        public string DestinationId { get; }

        /// <summary>
        /// Gets the data to export.
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Gets the protocol parameters.
        /// </summary>
        public IDictionary<string, object> ProtocolParameters { get; }

        /// <summary>
        /// Gets additional parameters for the export operation.
        /// </summary>
        public IDictionary<string, object> AdditionalParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportParameters"/> class.
        /// </summary>
        /// <param name="destinationId">The destination identifier.</param>
        /// <param name="data">The data to export.</param>
        /// <param name="protocolParameters">The protocol parameters.</param>
        /// <param name="additionalParameters">Additional parameters for the export operation.</param>
        public ExportParameters(
            string destinationId,
            object data,
            IDictionary<string, object> protocolParameters = null,
            IDictionary<string, object> additionalParameters = null)
        {
            DestinationId = destinationId;
            Data = data;
            ProtocolParameters = protocolParameters ?? new Dictionary<string, object>();
            AdditionalParameters = additionalParameters ?? new Dictionary<string, object>();
        }
    }
}
