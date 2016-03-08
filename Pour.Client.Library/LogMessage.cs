using System;

namespace Pour.Client.Library
{
    internal class LogMessage
    {
        private const string RowKeyFormat = "{0}-{1}";

        private const string JsonFormat = "{{ {0} }}";

        private string[] values = new string[3];


        internal const string EventTimeKey = "EventTime";

        internal string MessageJson
        {
            get
            {
                return Utility.Join(values);
            }
        }

        internal string RowKey { get; private set; }

        internal DateTime Time { get; private set; }

        internal const string MessageKey = "Message";

        internal const string LevelKey = "Level";

        internal const int MaxMessageLength = 10240;

        internal LogMessage(string message, Utility.Level level, int id = 0)
        {
            // Set the time
            Time = DateTime.UtcNow;

            // Set the row key
            RowKey = string.Format(RowKeyFormat, DateTime.MaxValue.Subtract(Time).Ticks, id);

            // Trim the message if length is greater than max limit
            if (message.Length > MaxMessageLength)
            {
                message = message.Substring(0, MaxMessageLength);
            }

            // Set the message json
            try
            {
                values[0] = Utility.GetJsonRepresentation(MessageKey, message);
                values[1] = Utility.GetJsonRepresentation(LevelKey, (int)level);
                values[2] = Utility.GetJsonRepresentation(EventTimeKey, Time);
            }
            catch (Exception e)
            {
                Utility.Output("Message formatting is failed for message: {0}. Exception: {1}",
                    message, e.Message);
            }
        }

        internal string GetJson(string contextJson = "")
        {
            string rowKeyJson = Utility.GetJsonWithoutType(Utility.RowKey, RowKey);
            string partitionKeyJson = Utility.GetJsonWithoutType(Utility.PartitionKey, Utility.GetPartitionKey(RowKey));

            if (string.IsNullOrWhiteSpace(contextJson))
            {
                return string.Format("{{ {0}, {1}, {2}, {3}, {4} }}",
                    values[0], values[1], values[2], rowKeyJson, partitionKeyJson);
            }
            else
            {
                return string.Format("{{ {0}, {1}, {2}, {3}, {4}, {5} }}",
                    values[0], values[1], values[2], rowKeyJson, partitionKeyJson, contextJson);
            }
        }
    }
}
