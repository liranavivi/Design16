using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Common.Services;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Services.Abstractions;
using Microsoft.Extensions.Logging;

using ExecutionContextType = FlowArchitecture.Common.Services.ExecutionContext;

namespace FlowArchitecture.Services.Exporters
{
    /// <summary>
    /// Exporter service for file-based destinations.
    /// </summary>
    public class FileExporterService : AbstractExporterService
    {
        /// <summary>
        /// Gets the protocol identifier used by this exporter.
        /// </summary>
        public override string ProtocolId => "file";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileExporterService"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        public FileExporterService(string id, string name, string description, ILogger<FileExporterService> logger)
            : base(id, name, description, logger)
        {
        }

        /// <summary>
        /// Gets the capabilities of the protocol used by this exporter.
        /// </summary>
        /// <returns>The protocol capabilities.</returns>
        public override ProtocolCapabilities GetProtocolCapabilities()
        {
            return new ProtocolCapabilities(
                supportsWriting: true,
                supportsAppending: true);
        }

        /// <summary>
        /// Gets the merge capabilities of this exporter.
        /// </summary>
        /// <returns>The merge capabilities.</returns>
        public override MergeCapabilities GetMergeCapabilities()
        {
            return new MergeCapabilities(
                supportsMerging: true,
                new[]
                {
                    MergeStrategy.Concatenate,
                    MergeStrategy.UseFirst,
                    MergeStrategy.UseLast
                },
                MergeStrategy.Concatenate);
        }

        /// <summary>
        /// Called when an export operation is being performed.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the export.</returns>
        protected override async Task<ExportResult> OnExportAsync(ExportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken)
        {
            try
            {
                var filePath = GetFilePath(parameters);
                var content = parameters.Data as string;

                if (string.IsNullOrEmpty(content))
                {
                    return ExportResult.Failure("Content is null or empty.");
                }

                var encoding = GetEncoding(parameters);
                var append = GetAppendMode(parameters);

                await WriteFileAsync(filePath, content, encoding, append, cancellationToken);

                var details = new Dictionary<string, object>
                {
                    { "FilePath", filePath },
                    { "ContentLength", content.Length },
                    { "Encoding", encoding.WebName },
                    { "Append", append }
                };

                return ExportResult.Success(details);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error exporting to file: {ErrorMessage}", ex.Message);
                return ExportResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Called when a merge operation is being performed.
        /// </summary>
        /// <param name="branchData">The data from multiple branches.</param>
        /// <param name="strategy">The merge strategy.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the merged data.</returns>
        protected override Task<object> OnMergeAsync(IDictionary<string, object> branchData, MergeStrategy strategy, ExecutionContextType context, CancellationToken cancellationToken)
        {
            if (branchData == null || branchData.Count == 0)
            {
                return Task.FromResult<object>(string.Empty);
            }

            switch (strategy)
            {
                case MergeStrategy.Concatenate:
                    return Task.FromResult<object>(ConcatenateBranches(branchData));

                case MergeStrategy.UseFirst:
                    return Task.FromResult<object>(GetFirstBranch(branchData));

                case MergeStrategy.UseLast:
                    return Task.FromResult<object>(GetLastBranch(branchData));

                default:
                    return Task.FromResult<object>(ConcatenateBranches(branchData));
            }
        }

        /// <summary>
        /// Validates the export parameters.
        /// </summary>
        /// <param name="parameters">The export parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        public override ValidationResult ValidateParameters(ExportParameters parameters)
        {
            var baseResult = base.ValidateParameters(parameters);
            var errors = new List<ValidationError>(baseResult.Errors);

            if (!parameters.ProtocolParameters.ContainsKey("FilePath") && !parameters.ProtocolParameters.ContainsKey("Directory"))
            {
                errors.Add(new ValidationError("ProtocolParameters.FilePath", "FilePath or Directory is required."));
            }

            if (parameters.ProtocolParameters.ContainsKey("Directory") && !parameters.ProtocolParameters.ContainsKey("FileName"))
            {
                errors.Add(new ValidationError("ProtocolParameters.FileName", "FileName is required when Directory is specified."));
            }

            if (!(parameters.Data is string))
            {
                errors.Add(new ValidationError("Data", "Data must be a string."));
            }

            return new ValidationResult(errors);
        }

        /// <summary>
        /// Gets the file path from the export parameters.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <returns>The file path.</returns>
        private string GetFilePath(ExportParameters parameters)
        {
            if (parameters.ProtocolParameters.TryGetValue("FilePath", out var filePathObj) && filePathObj is string filePath)
            {
                return filePath;
            }

            if (parameters.ProtocolParameters.TryGetValue("Directory", out var directoryObj) &&
                parameters.ProtocolParameters.TryGetValue("FileName", out var fileNameObj) &&
                directoryObj is string directory &&
                fileNameObj is string fileName)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return Path.Combine(directory, fileName);
            }

            throw new ArgumentException("FilePath or Directory and FileName must be specified.");
        }

        /// <summary>
        /// Gets the encoding from the export parameters.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <returns>The encoding.</returns>
        private Encoding GetEncoding(ExportParameters parameters)
        {
            if (parameters.ProtocolParameters.TryGetValue("Encoding", out var encodingObj) && encodingObj is string encodingName)
            {
                try
                {
                    return Encoding.GetEncoding(encodingName);
                }
                catch (ArgumentException)
                {
                    Logger.LogWarning("Invalid encoding name: {EncodingName}. Using UTF-8.", encodingName);
                }
            }

            return Encoding.UTF8;
        }

        /// <summary>
        /// Gets the append mode from the export parameters.
        /// </summary>
        /// <param name="parameters">The export parameters.</param>
        /// <returns>A value indicating whether to append to the file.</returns>
        private bool GetAppendMode(ExportParameters parameters)
        {
            if (parameters.ProtocolParameters.TryGetValue("Append", out var appendObj) && appendObj is bool append)
            {
                return append;
            }

            return false;
        }

        /// <summary>
        /// Writes a file asynchronously.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="content">The content to write.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="append">A value indicating whether to append to the file.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task WriteFileAsync(string filePath, string content, Encoding encoding, bool append, CancellationToken cancellationToken)
        {
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new StreamWriter(filePath, append, encoding);
            await writer.WriteAsync(content);
        }

        /// <summary>
        /// Concatenates data from multiple branches.
        /// </summary>
        /// <param name="branchData">The data from multiple branches.</param>
        /// <returns>The concatenated data.</returns>
        private string ConcatenateBranches(IDictionary<string, object> branchData)
        {
            var builder = new StringBuilder();

            foreach (var branch in branchData.Values)
            {
                if (branch is string content)
                {
                    builder.Append(content);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the data from the first branch.
        /// </summary>
        /// <param name="branchData">The data from multiple branches.</param>
        /// <returns>The data from the first branch.</returns>
        private string GetFirstBranch(IDictionary<string, object> branchData)
        {
            foreach (var branch in branchData.Values)
            {
                if (branch is string content)
                {
                    return content;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the data from the last branch.
        /// </summary>
        /// <param name="branchData">The data from multiple branches.</param>
        /// <returns>The data from the last branch.</returns>
        private string GetLastBranch(IDictionary<string, object> branchData)
        {
            string lastContent = string.Empty;

            foreach (var branch in branchData.Values)
            {
                if (branch is string content)
                {
                    lastContent = content;
                }
            }

            return lastContent;
        }
    }
}
