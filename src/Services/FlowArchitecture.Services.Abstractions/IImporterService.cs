using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Common.Services;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Services;

using ExecutionContextType = FlowArchitecture.Common.Services.ExecutionContext;

namespace FlowArchitecture.Services.Abstractions
{
    /// <summary>
    /// Interface for importer services in the Flow Architecture system.
    /// </summary>
    public interface IImporterService : IService
    {
        /// <summary>
        /// Gets the protocol identifier used by this importer.
        /// </summary>
        string ProtocolId { get; }

        /// <summary>
        /// Gets the capabilities of the protocol used by this importer.
        /// </summary>
        /// <returns>The protocol capabilities.</returns>
        ProtocolCapabilities GetProtocolCapabilities();

        /// <summary>
        /// Imports data from a source.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the import.</returns>
        Task<ImportResult> ImportAsync(ImportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the import parameters.
        /// </summary>
        /// <param name="parameters">The import parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        ValidationResult ValidateParameters(ImportParameters parameters);
    }

    /// <summary>
    /// Represents parameters for an import operation.
    /// </summary>
    public class ImportParameters
    {
        /// <summary>
        /// Gets the source identifier.
        /// </summary>
        public string SourceId { get; }

        /// <summary>
        /// Gets the protocol parameters.
        /// </summary>
        public IDictionary<string, object> ProtocolParameters { get; }

        /// <summary>
        /// Gets additional parameters for the import operation.
        /// </summary>
        public IDictionary<string, object> AdditionalParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportParameters"/> class.
        /// </summary>
        /// <param name="sourceId">The source identifier.</param>
        /// <param name="protocolParameters">The protocol parameters.</param>
        /// <param name="additionalParameters">Additional parameters for the import operation.</param>
        public ImportParameters(
            string sourceId,
            IDictionary<string, object> protocolParameters = null,
            IDictionary<string, object> additionalParameters = null)
        {
            SourceId = sourceId;
            ProtocolParameters = protocolParameters ?? new Dictionary<string, object>();
            AdditionalParameters = additionalParameters ?? new Dictionary<string, object>();
        }
    }
}
