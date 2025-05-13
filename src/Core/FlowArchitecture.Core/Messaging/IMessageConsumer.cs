using System.Threading;
using System.Threading.Tasks;

namespace FlowArchitecture.Core.Messaging
{
    /// <summary>
    /// Represents a consumer of messages of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of messages this consumer can handle.</typeparam>
    public interface IMessageConsumer<T> where T : class
    {
        /// <summary>
        /// Determines whether this consumer can handle the specified message.
        /// </summary>
        /// <param name="message">The message to check.</param>
        /// <returns><c>true</c> if this consumer can handle the message; otherwise, <c>false</c>.</returns>
        bool CanConsume(T message);
        
        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message to consume.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ConsumeAsync(T message, CancellationToken cancellationToken = default);
    }
}
