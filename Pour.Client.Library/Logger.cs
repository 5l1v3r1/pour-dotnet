using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Pour.Client.Library
{
    /// <summary>
    /// Logger to log messages on different levels.
    /// </summary>
    public class Logger : ILogger
    {
        private static int _logCount = 0;

        private readonly BlockingCollection<LogMessage> _logCollection;

        internal Logger(BlockingCollection<LogMessage> logCollecion)
        {
            _logCollection = logCollecion;
        }

        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Critical"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        public void Critical(string message)
        {
            AddToCollection(message, Utility.Level.Critical);
        }

        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Error"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        public void Error(string message)
        {
            AddToCollection(message, Utility.Level.Error);
        }

        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Warning"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        public void Warning(string message)
        {
            AddToCollection(message, Utility.Level.Warning);
        }

        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Info"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        public void Info(string message)
        {
            AddToCollection(message, Utility.Level.Info);
        }

        /// <summary>
        /// Logs a message of type <see cref="Utility.Level.Verbose"/>.
        /// </summary>
        /// <param name="message">Message to be logged and written to storage tables</param>
        public void Verbose(string message)
        {
            AddToCollection(message, Utility.Level.Verbose);
        }

        #region Private Helpers

        private void AddToCollection(string message, Utility.Level level)
        {
            message.RequireNotNull("message");

            try
            {
                _logCollection.Add(new LogMessage(message, level, Interlocked.Increment(ref _logCount)));
            }
            catch (Exception e)
            {
                Utility.Output("Message {0} with level {1} couldn't be logged. Exception: {2}",
                    message, level, e.Message);
            }
        }

        #endregion
    }
}
