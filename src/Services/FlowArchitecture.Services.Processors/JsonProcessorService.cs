using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FlowArchitecture.Common.Services;
using FlowArchitecture.Core.Common;
using FlowArchitecture.Services.Abstractions;
using Microsoft.Extensions.Logging;

using ExecutionContextType = FlowArchitecture.Common.Services.ExecutionContext;

namespace FlowArchitecture.Services.Processors
{
    /// <summary>
    /// Processor service for JSON transformation.
    /// </summary>
    public class JsonProcessorService : AbstractProcessorService
    {
        private readonly SchemaDefinition _inputSchema;
        private readonly SchemaDefinition _outputSchema;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonProcessorService"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the service.</param>
        /// <param name="name">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        /// <param name="logger">The logger for this service.</param>
        public JsonProcessorService(string id, string name, string description, ILogger<JsonProcessorService> logger)
            : base(id, name, description, logger)
        {
            _inputSchema = CreateInputSchema();
            _outputSchema = CreateOutputSchema();
        }

        /// <summary>
        /// Gets the input schema for the processor.
        /// </summary>
        /// <returns>The input schema.</returns>
        public override SchemaDefinition GetInputSchema()
        {
            return _inputSchema;
        }

        /// <summary>
        /// Gets the output schema for the processor.
        /// </summary>
        /// <returns>The output schema.</returns>
        public override SchemaDefinition GetOutputSchema()
        {
            return _outputSchema;
        }

        /// <summary>
        /// Called when a processing operation is being performed.
        /// </summary>
        /// <param name="parameters">The processing parameters.</param>
        /// <param name="context">The execution context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the result of the processing.</returns>
        protected override async Task<ProcessingResult> OnProcessAsync(ProcessParameters parameters, ExecutionContextType context, CancellationToken cancellationToken)
        {
            try
            {
                var inputData = parameters.InputData as string;

                if (string.IsNullOrEmpty(inputData))
                {
                    return ProcessingResult.Failure("Input data is null or empty.");
                }

                var transformationType = GetTransformationType(parameters);
                var result = await TransformJsonAsync(inputData, transformationType, parameters, cancellationToken);

                var details = new Dictionary<string, object>
                {
                    { "TransformationType", transformationType.ToString() },
                    { "InputLength", inputData.Length },
                    { "OutputLength", result.ToString().Length }
                };

                return ProcessingResult.Success(result, details);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing JSON: {ErrorMessage}", ex.Message);
                return ProcessingResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Validates data against a schema.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <param name="schema">The schema to validate against.</param>
        /// <returns>A validation result indicating whether the data is valid.</returns>
        protected override ValidationResult ValidateDataAgainstSchema(object data, SchemaDefinition schema)
        {
            var errors = new List<ValidationError>();

            if (data == null)
            {
                return ValidationResult.Failure("data", "Data cannot be null.");
            }

            if (!(data is string jsonString))
            {
                return ValidationResult.Failure("data", "Data must be a string.");
            }

            try
            {
                using var document = JsonDocument.Parse(jsonString);

                // Basic validation - just check if it's valid JSON
                // In a real implementation, we would validate against the schema
            }
            catch (JsonException ex)
            {
                errors.Add(new ValidationError("data", $"Invalid JSON: {ex.Message}"));
            }

            return new ValidationResult(errors);
        }

        /// <summary>
        /// Validates the processing parameters.
        /// </summary>
        /// <param name="parameters">The processing parameters to validate.</param>
        /// <returns>A validation result indicating whether the parameters are valid.</returns>
        public override ValidationResult ValidateParameters(ProcessParameters parameters)
        {
            var baseResult = base.ValidateParameters(parameters);
            var errors = new List<ValidationError>(baseResult.Errors);

            if (!(parameters.InputData is string))
            {
                errors.Add(new ValidationError("InputData", "Input data must be a string."));
            }

            if (!parameters.Options.ContainsKey("TransformationType"))
            {
                errors.Add(new ValidationError("Options.TransformationType", "TransformationType is required."));
            }
            else if (parameters.Options.TryGetValue("TransformationType", out var transformationTypeObj) &&
                     transformationTypeObj is string transformationTypeStr &&
                     !Enum.TryParse<JsonTransformationType>(transformationTypeStr, true, out _))
            {
                errors.Add(new ValidationError("Options.TransformationType", $"Invalid transformation type: {transformationTypeStr}."));
            }

            return new ValidationResult(errors);
        }

        /// <summary>
        /// Creates the input schema.
        /// </summary>
        /// <returns>The input schema.</returns>
        private SchemaDefinition CreateInputSchema()
        {
            return new SchemaDefinition(
                "JsonInput",
                "1.0",
                "Input schema for JSON processor",
                new[]
                {
                    new SchemaField("JsonString", SchemaFieldType.String, isRequired: true, description: "JSON string to process")
                });
        }

        /// <summary>
        /// Creates the output schema.
        /// </summary>
        /// <returns>The output schema.</returns>
        private SchemaDefinition CreateOutputSchema()
        {
            return new SchemaDefinition(
                "JsonOutput",
                "1.0",
                "Output schema for JSON processor",
                new[]
                {
                    new SchemaField("ProcessedJson", SchemaFieldType.String, isRequired: true, description: "Processed JSON string")
                });
        }

        /// <summary>
        /// Gets the transformation type from the processing parameters.
        /// </summary>
        /// <param name="parameters">The processing parameters.</param>
        /// <returns>The transformation type.</returns>
        private JsonTransformationType GetTransformationType(ProcessParameters parameters)
        {
            if (parameters.Options.TryGetValue("TransformationType", out var transformationTypeObj) &&
                transformationTypeObj is string transformationTypeStr &&
                Enum.TryParse<JsonTransformationType>(transformationTypeStr, true, out var transformationType))
            {
                return transformationType;
            }

            return JsonTransformationType.None;
        }

        /// <summary>
        /// Transforms JSON asynchronously.
        /// </summary>
        /// <param name="jsonString">The JSON string to transform.</param>
        /// <param name="transformationType">The transformation type.</param>
        /// <param name="parameters">The processing parameters.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, containing the transformed JSON.</returns>
        private Task<object> TransformJsonAsync(string jsonString, JsonTransformationType transformationType, ProcessParameters parameters, CancellationToken cancellationToken)
        {
            using var document = JsonDocument.Parse(jsonString);

            switch (transformationType)
            {
                case JsonTransformationType.Minify:
                    return Task.FromResult<object>(MinifyJson(document));

                case JsonTransformationType.Prettify:
                    return Task.FromResult<object>(PrettifyJson(document));

                case JsonTransformationType.Filter:
                    return Task.FromResult<object>(FilterJson(document, parameters));

                case JsonTransformationType.Transform:
                    return Task.FromResult<object>(TransformJson(document, parameters));

                default:
                    return Task.FromResult<object>(jsonString);
            }
        }

        /// <summary>
        /// Minifies JSON.
        /// </summary>
        /// <param name="document">The JSON document to minify.</param>
        /// <returns>The minified JSON.</returns>
        private string MinifyJson(JsonDocument document)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            return JsonSerializer.Serialize(document.RootElement, options);
        }

        /// <summary>
        /// Prettifies JSON.
        /// </summary>
        /// <param name="document">The JSON document to prettify.</param>
        /// <returns>The prettified JSON.</returns>
        private string PrettifyJson(JsonDocument document)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            return JsonSerializer.Serialize(document.RootElement, options);
        }

        /// <summary>
        /// Filters JSON.
        /// </summary>
        /// <param name="document">The JSON document to filter.</param>
        /// <param name="parameters">The processing parameters.</param>
        /// <returns>The filtered JSON.</returns>
        private string FilterJson(JsonDocument document, ProcessParameters parameters)
        {
            // In a real implementation, we would filter the JSON based on the parameters
            // For now, we just return the original JSON
            return JsonSerializer.Serialize(document.RootElement);
        }

        /// <summary>
        /// Transforms JSON.
        /// </summary>
        /// <param name="document">The JSON document to transform.</param>
        /// <param name="parameters">The processing parameters.</param>
        /// <returns>The transformed JSON.</returns>
        private string TransformJson(JsonDocument document, ProcessParameters parameters)
        {
            // In a real implementation, we would transform the JSON based on the parameters
            // For now, we just return the original JSON
            return JsonSerializer.Serialize(document.RootElement);
        }
    }

    /// <summary>
    /// Represents the type of JSON transformation.
    /// </summary>
    public enum JsonTransformationType
    {
        /// <summary>
        /// No transformation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Minify JSON.
        /// </summary>
        Minify = 1,

        /// <summary>
        /// Prettify JSON.
        /// </summary>
        Prettify = 2,

        /// <summary>
        /// Filter JSON.
        /// </summary>
        Filter = 3,

        /// <summary>
        /// Transform JSON.
        /// </summary>
        Transform = 4
    }
}
