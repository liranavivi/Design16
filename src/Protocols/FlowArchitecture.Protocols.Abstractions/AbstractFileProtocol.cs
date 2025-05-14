using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;
using Microsoft.Extensions.Logging;

namespace FlowArchitecture.Protocols.Abstractions
{
    /// <summary>
    /// Base implementation for file-based protocols in the Flow Architecture system.
    /// </summary>
    public abstract class AbstractFileProtocol : AbstractProtocol, IFileProtocol
    {
        private const string FilePathParameterName = "FilePath";
        private const string FileFormatParameterName = "FileFormat";
        private const string OverwriteParameterName = "Overwrite";

        /// <summary>
        /// Gets the file path.
        /// </summary>
        public string FilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the file format.
        /// </summary>
        public string FileFormat { get; private set; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether to overwrite existing files.
        /// </summary>
        protected bool Overwrite { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the file exists.
        /// </summary>
        public bool FileExists => !string.IsNullOrEmpty(FilePath) && File.Exists(FilePath);

        /// <summary>
        /// Gets the parameters required by the protocol.
        /// </summary>
        public override IReadOnlyList<ProtocolParameter> Parameters => new List<ProtocolParameter>
        {
            new ProtocolParameter(
                FilePathParameterName,
                typeof(string),
                true,
                "File Path",
                "The path to the file."),

            new ProtocolParameter(
                FileFormatParameterName,
                typeof(string),
                true,
                "File Format",
                "The format of the file (e.g., json, xml, csv)."),

            new ProtocolParameter(
                OverwriteParameterName,
                typeof(bool),
                false,
                "Overwrite",
                "Whether to overwrite existing files.",
                false)
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractFileProtocol"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the protocol.</param>
        /// <param name="name">The name of the protocol.</param>
        /// <param name="version">The version of the protocol.</param>
        /// <param name="description">The description of the protocol.</param>
        /// <param name="logger">The logger for this protocol.</param>
        protected AbstractFileProtocol(string id, string name, string version, string description, ILogger logger)
            : base(id, name, version, description, logger)
        {
        }

        /// <summary>
        /// Called when the protocol is being initialized.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override Task<Result<bool>> OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (ProtocolParameters.TryGetValue(FilePathParameterName, out var filePathObj) && filePathObj is string filePath)
            {
                FilePath = filePath;
            }

            if (ProtocolParameters.TryGetValue(FileFormatParameterName, out var fileFormatObj) && fileFormatObj is string fileFormat)
            {
                FileFormat = fileFormat;
            }

            if (ProtocolParameters.TryGetValue(OverwriteParameterName, out var overwriteObj) && overwriteObj is bool overwrite)
            {
                Overwrite = overwrite;
            }

            return base.OnInitializeAsync(cancellationToken);
        }

        /// <summary>
        /// Called when the protocol is being executed.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the execution.</returns>
        protected override async Task<Result<object>> OnExecuteAsync(ProtocolExecutionContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                return Result<object>.Failure("FILE_PROTOCOL_ERROR", "File path is not set.");
            }

            if (context.InputData == null)
            {
                return await ReadAsync(context, cancellationToken);
            }
            else
            {
                var result = await WriteAsync(context.InputData, context, cancellationToken);
                return result.IsSuccess
                    ? Result<object>.Success(context.InputData)
                    : Result<object>.Failure(result.Errors.ToArray());
            }
        }

        /// <summary>
        /// Reads data from a file.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the read operation.</returns>
        public abstract Task<Result<object>> ReadAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes data to a file.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the write operation.</returns>
        public abstract Task<Result<bool>> WriteAsync(object data, ProtocolExecutionContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the delete operation.</returns>
        public virtual async Task<Result<bool>> DeleteAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    return Result<bool>.Failure("FILE_DELETE_ERROR", "File path is not set.");
                }

                if (!FileExists)
                {
                    return Result<bool>.Failure("FILE_DELETE_ERROR", $"File does not exist: {FilePath}");
                }

                Logger.LogInformation("Deleting file {FilePath}...", FilePath);

                File.Delete(FilePath);

                Logger.LogInformation("File {FilePath} deleted successfully.", FilePath);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting file {FilePath}: {ErrorMessage}", FilePath, ex.Message);
                return Result<bool>.Failure("FILE_DELETE_ERROR", ex.Message);
            }
        }
    }
}
