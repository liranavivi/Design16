using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Core.Protocols;
using FlowArchitecture.Protocols.Abstractions;
using Microsoft.Extensions.Logging;

namespace FlowArchitecture.Protocols.Implementations
{
    /// <summary>
    /// Implementation of a file-based protocol in the Flow Architecture system.
    /// </summary>
    public class FileProtocol : AbstractFileProtocol
    {
        private const string EncodingParameterName = "Encoding";
        private const string CreateDirectoryParameterName = "CreateDirectory";
        
        /// <summary>
        /// Gets the encoding to use for file operations.
        /// </summary>
        protected Encoding Encoding { get; private set; } = Encoding.UTF8;
        
        /// <summary>
        /// Gets a value indicating whether to create the directory if it doesn't exist.
        /// </summary>
        protected bool CreateDirectory { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FileProtocol"/> class.
        /// </summary>
        /// <param name="logger">The logger for this protocol.</param>
        public FileProtocol(ILogger<FileProtocol> logger)
            : base("file", "File Protocol", "1.0", "Protocol for reading and writing files.", logger)
        {
        }
        
        /// <summary>
        /// Called when the protocol is being initialized.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override Task<Result<bool>> OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (ProtocolParameters.TryGetValue(EncodingParameterName, out var encodingObj) && encodingObj is string encodingName)
            {
                try
                {
                    Encoding = Encoding.GetEncoding(encodingName);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Invalid encoding {EncodingName}. Using UTF-8 instead.", encodingName);
                }
            }
            
            if (ProtocolParameters.TryGetValue(CreateDirectoryParameterName, out var createDirObj) && createDirObj is bool createDir)
            {
                CreateDirectory = createDir;
            }
            
            return base.OnInitializeAsync(cancellationToken);
        }
        
        /// <summary>
        /// Reads data from a file.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the read operation.</returns>
        public override async Task<Result<object>> ReadAsync(ProtocolExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    return Result<object>.Failure("FILE_READ_ERROR", "File path is not set.");
                }
                
                if (!FileExists)
                {
                    return Result<object>.Failure("FILE_READ_ERROR", $"File does not exist: {FilePath}");
                }
                
                Logger.LogInformation("Reading file {FilePath} with format {FileFormat}...", FilePath, FileFormat);
                
                string content = await File.ReadAllTextAsync(FilePath, Encoding, cancellationToken);
                
                object result;
                
                switch (FileFormat.ToLowerInvariant())
                {
                    case "json":
                        result = JsonSerializer.Deserialize<object>(content) ?? new object();
                        break;
                    
                    case "xml":
                        // XML deserialization would be implemented here
                        return Result<object>.Failure("FILE_READ_ERROR", "XML format is not supported yet.");
                    
                    case "csv":
                        // CSV parsing would be implemented here
                        return Result<object>.Failure("FILE_READ_ERROR", "CSV format is not supported yet.");
                    
                    default:
                        // For other formats, return the raw content
                        result = content;
                        break;
                }
                
                Logger.LogInformation("File {FilePath} read successfully.", FilePath);
                
                return Result<object>.Success(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error reading file {FilePath}: {ErrorMessage}", FilePath, ex.Message);
                return Result<object>.Failure("FILE_READ_ERROR", ex.Message);
            }
        }
        
        /// <summary>
        /// Writes data to a file.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the write operation.</returns>
        public override async Task<Result<bool>> WriteAsync(object data, ProtocolExecutionContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    return Result<bool>.Failure("FILE_WRITE_ERROR", "File path is not set.");
                }
                
                if (FileExists && !Overwrite)
                {
                    return Result<bool>.Failure("FILE_WRITE_ERROR", $"File already exists and overwrite is not enabled: {FilePath}");
                }
                
                if (CreateDirectory)
                {
                    string? directory = Path.GetDirectoryName(FilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }
                
                Logger.LogInformation("Writing to file {FilePath} with format {FileFormat}...", FilePath, FileFormat);
                
                string content;
                
                switch (FileFormat.ToLowerInvariant())
                {
                    case "json":
                        content = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                        break;
                    
                    case "xml":
                        // XML serialization would be implemented here
                        return Result<bool>.Failure("FILE_WRITE_ERROR", "XML format is not supported yet.");
                    
                    case "csv":
                        // CSV formatting would be implemented here
                        return Result<bool>.Failure("FILE_WRITE_ERROR", "CSV format is not supported yet.");
                    
                    default:
                        // For other formats, use the string representation
                        content = data?.ToString() ?? string.Empty;
                        break;
                }
                
                await File.WriteAllTextAsync(FilePath, content, Encoding, cancellationToken);
                
                Logger.LogInformation("File {FilePath} written successfully.", FilePath);
                
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error writing to file {FilePath}: {ErrorMessage}", FilePath, ex.Message);
                return Result<bool>.Failure("FILE_WRITE_ERROR", ex.Message);
            }
        }
    }
}
