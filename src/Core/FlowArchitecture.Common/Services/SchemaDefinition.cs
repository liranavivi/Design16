using System.Collections.Generic;

namespace FlowArchitecture.Common.Services
{
    /// <summary>
    /// Represents a schema definition for data.
    /// </summary>
    public class SchemaDefinition
    {
        /// <summary>
        /// Gets the name of the schema.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the version of the schema.
        /// </summary>
        public string Version { get; }
        
        /// <summary>
        /// Gets the description of the schema.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Gets the fields in the schema.
        /// </summary>
        public IReadOnlyList<SchemaField> Fields { get; }
        
        /// <summary>
        /// Gets the nested schemas.
        /// </summary>
        public IDictionary<string, SchemaDefinition> NestedSchemas { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaDefinition"/> class.
        /// </summary>
        /// <param name="name">The name of the schema.</param>
        /// <param name="version">The version of the schema.</param>
        /// <param name="description">The description of the schema.</param>
        /// <param name="fields">The fields in the schema.</param>
        /// <param name="nestedSchemas">The nested schemas.</param>
        public SchemaDefinition(
            string name,
            string version,
            string description,
            IEnumerable<SchemaField> fields,
            IDictionary<string, SchemaDefinition> nestedSchemas = null)
        {
            Name = name;
            Version = version;
            Description = description;
            Fields = fields != null ? new List<SchemaField>(fields) : new List<SchemaField>();
            NestedSchemas = nestedSchemas ?? new Dictionary<string, SchemaDefinition>();
        }
    }
    
    /// <summary>
    /// Represents a field in a schema.
    /// </summary>
    public class SchemaField
    {
        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Gets the display name of the field.
        /// </summary>
        public string DisplayName { get; }
        
        /// <summary>
        /// Gets the description of the field.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Gets the data type of the field.
        /// </summary>
        public SchemaFieldType DataType { get; }
        
        /// <summary>
        /// Gets a value indicating whether the field is required.
        /// </summary>
        public bool IsRequired { get; }
        
        /// <summary>
        /// Gets a value indicating whether the field is a key field.
        /// </summary>
        public bool IsKey { get; }
        
        /// <summary>
        /// Gets the name of the nested schema if the field is a complex type.
        /// </summary>
        public string NestedSchemaName { get; }
        
        /// <summary>
        /// Gets additional properties of the field.
        /// </summary>
        public IDictionary<string, object> Properties { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaField"/> class.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        /// <param name="dataType">The data type of the field.</param>
        /// <param name="displayName">The display name of the field.</param>
        /// <param name="description">The description of the field.</param>
        /// <param name="isRequired">A value indicating whether the field is required.</param>
        /// <param name="isKey">A value indicating whether the field is a key field.</param>
        /// <param name="nestedSchemaName">The name of the nested schema if the field is a complex type.</param>
        /// <param name="properties">Additional properties of the field.</param>
        public SchemaField(
            string name,
            SchemaFieldType dataType,
            string displayName = null,
            string description = null,
            bool isRequired = false,
            bool isKey = false,
            string nestedSchemaName = null,
            IDictionary<string, object> properties = null)
        {
            Name = name;
            DataType = dataType;
            DisplayName = displayName ?? name;
            Description = description;
            IsRequired = isRequired;
            IsKey = isKey;
            NestedSchemaName = nestedSchemaName;
            Properties = properties ?? new Dictionary<string, object>();
        }
    }
    
    /// <summary>
    /// Represents the data type of a schema field.
    /// </summary>
    public enum SchemaFieldType
    {
        /// <summary>
        /// The field is a string.
        /// </summary>
        String = 0,
        
        /// <summary>
        /// The field is an integer.
        /// </summary>
        Integer = 1,
        
        /// <summary>
        /// The field is a decimal number.
        /// </summary>
        Decimal = 2,
        
        /// <summary>
        /// The field is a boolean.
        /// </summary>
        Boolean = 3,
        
        /// <summary>
        /// The field is a date.
        /// </summary>
        Date = 4,
        
        /// <summary>
        /// The field is a date and time.
        /// </summary>
        DateTime = 5,
        
        /// <summary>
        /// The field is a time.
        /// </summary>
        Time = 6,
        
        /// <summary>
        /// The field is a binary data.
        /// </summary>
        Binary = 7,
        
        /// <summary>
        /// The field is an array.
        /// </summary>
        Array = 8,
        
        /// <summary>
        /// The field is a complex object.
        /// </summary>
        Object = 9,
        
        /// <summary>
        /// The field is of a custom type.
        /// </summary>
        Custom = 10
    }
}
