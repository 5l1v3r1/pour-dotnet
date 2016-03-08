namespace Pour.Client.Library
{
    /// <summary>
    /// Interface for log operations.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Critical"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        void Critical(string message);

        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Error"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        void Error(string message);

        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Warning"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        void Warning(string message);

        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Info"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        void Info(string message);

        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Verbose"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        void Verbose(string message);
    }
}