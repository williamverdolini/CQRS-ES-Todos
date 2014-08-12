
namespace Todo.Infrastructure
{
    /// <summary>
    /// This is the main message handler interface of Rebus. Implement these to be
    /// called when messages are dispatched.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message being handled. Can be any assignable type as well.</typeparam>
    public interface IHandleMessages<in TMessage>
    {
        /// <summary>
        /// Handler method that will be called by the dispatcher for each logical message contained in the received transport message
        /// </summary>
        void Handle(TMessage message);
    }
}
