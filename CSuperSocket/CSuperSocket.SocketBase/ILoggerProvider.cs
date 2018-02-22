
using Dynamic.Core.Log;

namespace CSuperSocket.SocketBase
{
    /// <summary>
    /// The interface for who provides logger
    /// </summary>
    public interface ILoggerProvider
    {
        /// <summary>
        /// Gets the logger assosiated with this object.
        /// </summary>
        ILogger Logger { get; }
    }
}
