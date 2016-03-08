using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pour.Client.Library
{
    /// <summary>
    /// Manages log operations.
    /// </summary>
    public static class LogManager
    {
        private static BlockingCollection<LogMessage> _pendingLogMessages;

        private static ConcurrentDictionary<string, object> _contextDictionary; 


        internal static string ContextJson { get; private set; }

        private static object _contextJsonLock;


        private static string AppToken;

        private static string TablesUri;

        private static string AccountUri;

        private static string ErrorLogUri;

        private const string SearchUrlFormat = "http://43532227db0942388c5a047e7bdb0db5.cloudapp.net/api/beta/search?token={0}";

        private const string ErrorUrlFormat = "http://43532227db0942388c5a047e7bdb0db5.cloudapp.net/api/beta/trace?account={0}";

        private static string[] NameAndKeySeparator = new string[] { "::" };


        internal static string Account;

        internal static string Key;

        internal static string LogTableUri;

        internal static HMACSHA256 SignMethod;

        internal const string EmulatorAccount = "devstoreaccount1";

        internal const string EmulatorKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==";

        internal const string EmulatorUri = "http://127.0.0.1:10002/devstoreaccount1/";

        /// <summary>
        /// Connects to Azure StorageTable running off of emulator. 
        /// Creates a new instance of <see cref="ILogger"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="ILogger"/></returns>
        public static ILogger Connect()
        {
            // Try to connect to Azure Storage
            InitializeUri(EmulatorAccount, EmulatorKey, EmulatorUri);
            ApiResponse response = ApiHelper.Validate(Account, Key, TablesUri);
            return Connect(response);
        }

        /// <summary>
        /// Connects to Azure StorageTable through application token created on the web portal. 
        /// Creates a new instance of <see cref="ILogger"/>.
        /// </summary>
        /// <param name="appToken">Application token generated on http://pour.cloudapp.net</param>
        /// <returns>A new instance of <see cref="ILogger"/></returns>
        public static ILogger Connect(string appToken)
        {
            // Search for token
            Tuple<string, string> result = Search(appToken);

            // App token is now validated, cache it
            AppToken = appToken;

            // Try to connect to Azure Storage
            InitializeUri(result.Item1, result.Item2);
            ApiResponse response = ApiHelper.Validate(result.Item1, result.Item2, TablesUri);
            return Connect(response);
        }

        /// <summary>
        /// Sets the context. This context value will be attached to each log message.
        /// </summary>
        /// <param name="name">Name of the context</param>
        /// <param name="value">Value of type <see cref="bool"/></param>
        public static void SetContext(string name, bool value)
        {
            AppendToContext(name, value);
        }

        /// <summary>
        /// Sets the context. This context value will be attached to each log message.
        /// </summary>
        /// <param name="name">Name of the context</param>
        /// <param name="value">Value of type <see cref="int"/></param>
        public static void SetContext(string name, int value)
        {
            AppendToContext(name, value);
        }

        /// <summary>
        /// Sets the context. This context value will be attached to each log message.
        /// </summary>
        /// <param name="name">Name of the context</param>
        /// <param name="value">Value of type <see cref="double"/></param>
        public static void SetContext(string name, double value)
        {
            AppendToContext(name, value);
        }

        /// <summary>
        /// Sets the context. This context value will be attached to each log message.
        /// </summary>
        /// <param name="name">Name of the context</param>
        /// <param name="value">Value of type <see cref="DateTime"/></param>
        public static void SetContext(string name, DateTime value)
        {
            AppendToContext(name, value);
        }

        /// <summary>
        /// Sets the context. This context value will be attached to each log message.
        /// </summary>
        /// <param name="name">Name of the context</param>
        /// <param name="value">Value of type <see cref="Guid"/></param>
        public static void SetContext(string name, Guid value)
        {
            AppendToContext(name, value);
        }

        /// <summary>
        /// Sets the context. This context value will be attached to each log message.
        /// </summary>
        /// <param name="name">Name of the context</param>
        /// <param name="value">Value of type <see cref="string"/></param>
        public static void SetContext(string name, string value)
        {
            AppendToContext(name, value);
        }

        /// <summary>
        /// Frees a context. If context is not set it will throw exception.
        /// </summary>
        /// <param name="name">Name of the context</param>
        public static void FreeContext(string name)
        {
            name = Utility.GetContextName(name);
            name.RequireNonEmpty("name");

            if (!_contextDictionary.ContainsKey(name))
            {
                throw new InvalidOperationException("Given name is not in the context.");
            }

            try
            {
                object value;
                _contextDictionary.TryRemove(name, out value);

                string[] contexts = new string[_contextDictionary.Keys.Count];
                int i = 0;
                foreach (string key in _contextDictionary.Keys)
                {
                    object valueFromDict;
                    _contextDictionary.TryGetValue(key, out valueFromDict);
                    contexts[i++] = Utility.GetJsonRepresentation(key, valueFromDict);
                }

                lock (_contextJsonLock)
                {
                    ContextJson = Utility.Join(contexts);
                }
            }
            catch (Exception e)
            {
                Utility.Output("An exception while freeing the context. Exception: {0}", e.Message);
                throw;
            }
        }

        /// <summary>
        /// Gets a context value given name. If context doesn't exist, then returns null.
        /// </summary>
        /// <param name="name">Name of the context</param>
        /// <returns>Context value</returns>
        public static object GetContext(string name)
        {
            name = Utility.GetContextName(name);
            name.RequireNonEmpty("name");

            object value;
            _contextDictionary.TryGetValue(name, out value);
            return value;
        }

        private static ILogger Connect(ApiResponse getTableResponse)
        {
            // If logs table does not exist, then create
            if (!getTableResponse.HasTable(ApiHelper.DefaultLogsTableName))
            {
                getTableResponse = ApiHelper.CreateTableIfNotExists(Account, Key, TablesUri, SignMethod, ApiHelper.DefaultLogsTableName);
            }

            // Create required data structures
            _pendingLogMessages = new BlockingCollection<LogMessage>();
            _contextDictionary = new ConcurrentDictionary<string, object>();
            _contextJsonLock = new object();
            ContextJson = string.Empty;

            // Set error uri
            ErrorLogUri = string.Format(ErrorUrlFormat, Account);

            // Start long running log writer
            Task.Factory.StartNew(Write, TaskCreationOptions.LongRunning);

            return new Logger(_pendingLogMessages);
        }

        private static Tuple<string, string> Search(string appToken)
        {
            try
            {
                string url = string.Format(SearchUrlFormat, appToken);
                WebRequest request = CreateAuthenticatedRequest(url, appToken);

                WebResponse response = request.GetResponse();
                Stream s = response.GetResponseStream();
                StreamReader r = new StreamReader(s);
                byte[] decodedNameAndKeyByteArray = Convert.FromBase64String(r.ReadToEnd());
                string decodedNameAndKey = Encoding.UTF8.GetString(decodedNameAndKeyByteArray);
                string[] decodedNameAndKeyArray = decodedNameAndKey.Split(NameAndKeySeparator, StringSplitOptions.RemoveEmptyEntries);
                return new Tuple<string, string>(decodedNameAndKeyArray[0], decodedNameAndKeyArray[1]);
            }
            catch (Exception e)
            {
                throw new Exception("Something went wrong while validating application token.", e);
            }
        }

        private static void AppendToContext(string name, object value)
        {
            value.RequireNotNull("value");

            name = Utility.GetContextName(name);
            name.RequireNonEmpty("name");
            
            if (_contextDictionary.TryAdd(name, value))
            {
                lock (_contextJsonLock)
                {
                    if (string.IsNullOrWhiteSpace(ContextJson))
                    {
                        ContextJson = Utility.GetJsonRepresentation(name, value);
                    }
                    else
                    {
                        ContextJson = string.Format("{0}, {1}", ContextJson, Utility.GetJsonRepresentation(name, value));
                    }
                }
            }
        }

        private static void Write()
        {
            // Until the log message adding is completed, keep running
            while (!_pendingLogMessages.IsCompleted)
            {
                try
                {
                    // Take the next message and write it
                    LogMessage nextMessage = _pendingLogMessages.Take();
                    string fullMessage = nextMessage.GetJson(ContextJson);
                    ApiResponse response = ApiHelper.InsertEntity(Account, Key, LogTableUri, SignMethod, fullMessage);
                    if (!response.Succedded)
                    {
                        SaveError(AppToken, response.ErrorMessage);
                    }
                }
                catch (Exception e)
                {
                    SaveError(AppToken, e.Message);
                    Utility.Output("Log message write is failed. Exception: {0}", e.Message);
                    throw;
                }
            }
        }

        private static void SaveError(string appToken, string body)
        {
            try
            {
                WebRequest request = CreateAuthenticatedRequest(ErrorLogUri, appToken, "POST", body);
                request.GetResponse();
            }
            catch (Exception e)
            {
                Utility.Output("Log message write is failed. Exception: {0}", e.Message);
            }
        }

        private static WebRequest CreateAuthenticatedRequest(string url, string appToken, string method = "GET", string body = "")
        {
            WebRequest request = WebRequest.Create(url);
            request.Method = method;

            // Compute and set auth header
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appToken)))
            {
                string time = DateTime.UtcNow.ToString("R");
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(
                    string.Format("{0}\n{1}\n{2}", appToken, time, request.RequestUri.PathAndQuery)));
                request.Headers.Add("x-app", appToken);
                request.Headers.Add("x-datetime", time);
                request.Headers.Add("x-auth", Convert.ToBase64String(hash));
            }

            // Set the body
            if (method.Equals("post", StringComparison.InvariantCultureIgnoreCase))
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteArray.Length;
                request.GetRequestStream().Write(byteArray, 0, byteArray.Length);
            }

            return request;
        }

        private static void InitializeUri(string account, string key, string uri = "")
        {
            account.RequireNotNull("account");
            key.RequireNotNull("key");

            Account = account;
            Key = key;

            AccountUri = ApiHelper.GetAccountUri(account, uri);
            TablesUri = ApiHelper.GetUri(AccountUri, ApiHelper.DefaultTablesSegmentName);
            LogTableUri = ApiHelper.GetUri(AccountUri, ApiHelper.DefaultLogsTableName);

            SignMethod = new HMACSHA256(Convert.FromBase64String(Key));
        }
    }
}
