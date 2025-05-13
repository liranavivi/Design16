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

namespace FlowArchitecture.Services.Importers
{
    /// <summary>
    /// Importer service for file-based sources.
    /// </summary>
    public class FileImporterService : AbstractImporterService
    {
        /// <summary>
        /// Gets the protocol identifier used by this importer.
        /// </summary>
        public override string ProtocolId => "file";

        /// <summary>
        /// Initializes a new instance of the <see cref="FileImporterService"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        public FileImporterService(string id, string name, string description, ILogger<FileImporterService> logger)
            : base(id, name, description, logger)
        {
        }

        /// <summary>
        /// Gets the capabilities of the protocol used by this importer.
        /// </summary>
        /// <returns>The protocol capabilities.</returns>
        public override ProtocolCapabilities GetProtocolCapabilities()
        {
            return new ProtocolCapabilities(
                supportsReading: true,
                supportsListing: true,
                supportsSearching: true);
        }

        /// <summary>
        /// Called when an import operation is being performed.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the import.</returns>
        protected override async Task<ImportResult> OnImportAsync(ImportParameters parameters, ExecutionContextType context, CancellationToken cancellationToken)
        {
            try
            {
                var filePath = GetFilePath(parameters);

                if (!File.Exists(filePath))
                {
                    return ImportResult.Failure($"File not found: {filePath}");
                }

                var encoding = GetEncoding(parameters);
                var content = await ReadFileAsync(filePath, encoding, cancellationToken);

                var details = new Dictionary<string, object>
                {
                    { "FilePath", filePath },
                    { "FileSize", new FileInfo(filePath).Length },
                    { "Encoding", encoding.WebName }
                };

                return ImportResult.Success(content, details);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error importing file: {ErrorMessage}", ex.Message);
                return ImportResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Validates the import parameters.
        /// </summary>
        /// <param name="parameters">The import parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        public override ValidationResult ValidateParameters(ImportParameters parameters)
        {
            var baseResult = base.ValidateParameters(parameters);
            var errors = new List<ValidationError>(baseResult.Errors);

            if (!parameters.ProtocolParameters.ContainsKey("FilePath") && !parameters.ProtocolParameters.ContainsKey("Directory"))
            {
                errors.Add(new ValidationError("ProtocolParameters.FilePath", "FilePath or Directory is required."));
            }

            if (parameters.ProtocolParameters.ContainsKey("FilePath") && parameters.ProtocolParameters.ContainsKey("Directory"))
            {
                errors.Add(new ValidationError("ProtocolParameters", "Only one of FilePath or Directory should be specified."));
            }

            if (parameters.ProtocolParameters.ContainsKey("Directory") && !parameters.ProtocolParameters.ContainsKey("FilePattern"))
            {
                errors.Add(new ValidationError("ProtocolParameters.FilePattern", "FilePattern is required when Directory is specified."));
            }

            return new ValidationResult(errors);
        }

        /// <summary>
        /// Gets the file path from the import parameters.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <returns>The file path.</returns>
        private string GetFilePath(ImportParameters parameters)
        {
            if (parameters.ProtocolParameters.TryGetValue("FilePath", out var filePathObj) && filePathObj is string filePath)
            {
                return filePath;
            }

            if (parameters.ProtocolParameters.TryGetValue("Directory", out var directoryObj) &&
                parameters.ProtocolParameters.TryGetValue("FilePattern", out var filePatternObj) &&
                directoryObj is string directory &&
                filePatternObj is string filePattern)
            {
                var files = Directory.GetFiles(directory, filePattern);

                if (parameters.ProtocolParameters.TryGetValue("UseLatest", out var useLatestObj) &&
                    useLatestObj is bool useLatest &&
                    useLatest)
                {
                    return GetLatestFile(files);
                }

                return files.Length > 0 ? files[0] : throw new FileNotFoundException($"No files matching pattern {filePattern} found in directory {directory}.");
            }

            throw new ArgumentException("FilePath or Directory and FilePattern must be specified.");
        }

        /// <summary>
        /// Gets the latest file from a list of files.
        /// </summary>
        /// <param name="files">The list of files.</param>
        /// <returns>The latest file.</returns>
        private string GetLatestFile(string[] files)
        {
            if (files.Length == 0)
            {
                throw new FileNotFoundException("No files found.");
            }

            string latestFile = files[0];
            DateTime latestTime = File.GetLastWriteTime(latestFile);

            for (int i = 1; i < files.Length; i++)
            {
                DateTime fileTime = File.GetLastWriteTime(files[i]);

                if (fileTime > latestTime)
                {
                    latestFile = files[i];
                    latestTime = fileTime;
                }
            }

            return latestFile;
        }

        /// <summary>
        /// Gets the encoding from the import parameters.
        /// </summary>
        /// <param name="parameters">The import parameters.</param>
        /// <returns>The encoding.</returns>
        private Encoding GetEncoding(ImportParameters parameters)
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
        /// Reads a file asynchronously.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the file content.</returns>
        private async Task<string> ReadFileAsync(string filePath, Encoding encoding, CancellationToken cancellationToken)
        {
            using var reader = new StreamReader(filePath, encoding);
            return await reader.ReadToEndAsync();
        }
    }
}
