using System.Collections.Generic;

namespace FlowArchitecture.Common.Services
{
    /// <summary>
    /// Represents the capabilities of a protocol.
    /// </summary>
    public class ProtocolCapabilities
    {
        /// <summary>
        /// Gets a value indicating whether the protocol supports reading.
        /// </summary>
        public bool SupportsReading { get; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol supports writing.
        /// </summary>
        public bool SupportsWriting { get; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol supports appending.
        /// </summary>
        public bool SupportsAppending { get; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol supports deleting.
        /// </summary>
        public bool SupportsDeleting { get; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol supports listing.
        /// </summary>
        public bool SupportsListing { get; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol supports searching.
        /// </summary>
        public bool SupportsSearching { get; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol supports transactions.
        /// </summary>
        public bool SupportsTransactions { get; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol supports streaming.
        /// </summary>
        public bool SupportsStreaming { get; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol supports authentication.
        /// </summary>
        public bool SupportsAuthentication { get; }
        
        /// <summary>
        /// Gets a value indicating whether the protocol supports encryption.
        /// </summary>
        public bool SupportsEncryption { get; }
        
        /// <summary>
        /// Gets additional capabilities of the protocol.
        /// </summary>
        public IDictionary<string, object> AdditionalCapabilities { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolCapabilities"/> class.
        /// </summary>
        /// <param name="supportsReading">A value indicating whether the protocol supports reading.</param>
        /// <param name="supportsWriting">A value indicating whether the protocol supports writing.</param>
        /// <param name="supportsAppending">A value indicating whether the protocol supports appending.</param>
        /// <param name="supportsDeleting">A value indicating whether the protocol supports deleting.</param>
        /// <param name="supportsListing">A value indicating whether the protocol supports listing.</param>
        /// <param name="supportsSearching">A value indicating whether the protocol supports searching.</param>
        /// <param name="supportsTransactions">A value indicating whether the protocol supports transactions.</param>
        /// <param name="supportsStreaming">A value indicating whether the protocol supports streaming.</param>
        /// <param name="supportsAuthentication">A value indicating whether the protocol supports authentication.</param>
        /// <param name="supportsEncryption">A value indicating whether the protocol supports encryption.</param>
        /// <param name="additionalCapabilities">Additional capabilities of the protocol.</param>
        public ProtocolCapabilities(
            bool supportsReading = false,
            bool supportsWriting = false,
            bool supportsAppending = false,
            bool supportsDeleting = false,
            bool supportsListing = false,
            bool supportsSearching = false,
            bool supportsTransactions = false,
            bool supportsStreaming = false,
            bool supportsAuthentication = false,
            bool supportsEncryption = false,
            IDictionary<string, object> additionalCapabilities = null)
        {
            SupportsReading = supportsReading;
            SupportsWriting = supportsWriting;
            SupportsAppending = supportsAppending;
            SupportsDeleting = supportsDeleting;
            SupportsListing = supportsListing;
            SupportsSearching = supportsSearching;
            SupportsTransactions = supportsTransactions;
            SupportsStreaming = supportsStreaming;
            SupportsAuthentication = supportsAuthentication;
            SupportsEncryption = supportsEncryption;
            AdditionalCapabilities = additionalCapabilities ?? new Dictionary<string, object>();
        }
    }
}
