using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;

namespace FlowArchitecture.Protocols.Abstractions
{
    /// <summary>
    /// Interface for file-based protocols in the Flow Architecture system.
    /// </summary>
    public interface IFileProtocol : IProtocol
    {
        /// <summary>
        /// Gets the file path.
        /// </summary>
        string FilePath { get; }
        
        /// <summary>
        /// Gets the file format.
        /// </summary>
        string FileFormat { get; }
        
        /// <summary>
        /// Gets a value indicating whether the file exists.
        /// </summary>
        bool FileExists { get; }
        
        /// <summary>
        /// Reads data from a file.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the read operation.</returns>
        Task<Result<object>> ReadAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Writes data to a file.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the write operation.</returns>
        Task<Result<bool>> WriteAsync(object data, ProtocolExecutionContext context, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the delete operation.</returns>
        Task<Result<bool>> DeleteAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default);
    }
}
