using System;
using System.Diagnostics;
using System.Web;

namespace Pour.Client.Library
{
    internal static class Utility
    {
        internal const string OdataTypeKeySuffix = "@odata.type";

        internal const string OdataTypeValuePrefix = "Edm.";

        internal const string JsonDataFormat = "\"{0}{1}\": \"Edm.{2}\", \"{0}\": \"{3}\"";

        internal const string JsonDataFormatWithoutType = "\"{0}\": \"{1}\"";

        internal const string PartitionKey = "PartitionKey";

        internal const string RowKey = "RowKey";

        internal const string Separator = ",";

        internal enum Level
        {
            Critical = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
            Verbose = 4
        }

        internal static string GetContextName(string name)
        {
            return name?.Replace(" ", string.Empty) ?? string.Empty;
        }

        internal static string GetJsonRepresentation(string name, object value)
        {
            name.RequireNonEmpty("name");
            return string.Format(JsonDataFormat, name, OdataTypeKeySuffix, value.GetType().Name, value);
        }

        internal static string GetJsonRepresentation(string name, string value)
        {
            name.RequireNonEmpty("name");
            string escapedString = HttpUtility.JavaScriptStringEncode(value);
            return string.Format(JsonDataFormatWithoutType, name, escapedString);
        }

        internal static string GetJsonRepresentation(string name, DateTime value)
        {
            name.RequireNonEmpty("name");
            return string.Format(JsonDataFormat, name, OdataTypeKeySuffix, value.GetType().Name, value.ToString("O"));
        }

        internal static string Join(params string[] values)
        {
            return string.Join(Separator, values);
        }

        internal static string GetPartitionKey(string rowKey)
        {
            return rowKey.Substring(0, 5);
        }

        internal static string GetJsonWithoutType(string key, string value)
        {
            return string.Format(JsonDataFormatWithoutType, key, value);
        }

        internal static void RequireNotNull(this object value, string name, string message = "")
        {
            if (value == null)
            {
                throw new ArgumentNullException(string.Format("{0} is null. {1}", name, message));
            }
        }

        internal static void RequireNonEmpty(this string value, string name, string message = "")
        {
            value.RequireNotNull(name);

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(string.Format("{0} is empty. {1}", name, message));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(string.Format("{0} is white space. {1}", name, message));
            }
        }

        internal static void Output(string message, params object[] args)
        {
            string formattedMessage;

            try
            {
                formattedMessage = string.Format(message, args);
            }
            catch (Exception e)
            {
                Output("Message formatting is failed for message {0}. Exception: {1}", message, e.Message);
                formattedMessage = message;
            }

            OutputToDebug(formattedMessage);
            OutputToTrace(formattedMessage);
        }

        [Conditional("DEBUG")]
        private static void OutputToDebug(string message)
        {
            Debug.WriteLine(message);
        }

        [Conditional("TRACE")]
        private static void OutputToTrace(string message)
        {
            Trace.WriteLine(message);
        }
    }
}
