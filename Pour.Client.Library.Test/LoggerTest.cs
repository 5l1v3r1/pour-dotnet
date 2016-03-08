using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Pour.Client.Library.Test
{
    [TestClass]
    public class LoggerTest
    {
        private readonly IList<string> _sampleMessages = new[]
        {
            string.Empty,
            "some message",
            new string('*', LogMessage.MaxMessageLength / 2) 
        };

        private static readonly string LongMessage = new string('*', LogMessage.MaxMessageLength + 1);

        private static BlockingCollection<LogMessage> _collection; 

        private static Logger _logger;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _collection = new BlockingCollection<LogMessage>();
            _logger = new Logger(_collection);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_Critical_Null()
        {
            _logger.Critical(null);
        }

        public void Log_Critical()
        {
            foreach (string message in _sampleMessages)
            {
                _logger.Critical(message);
                Validate(message, Utility.Level.Critical);
            }
        }

        [TestMethod]
        public void Log_Critical_Long()
        {
            _logger.Critical(LongMessage);
            Validate(LongMessage, Utility.Level.Critical);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void Log_Error_Null()
        {
            _logger.Error(null);
        }

        [TestMethod]
        public void Log_Error()
        {
            foreach (string message in _sampleMessages)
            {
                _logger.Error(message);
                Validate(message, Utility.Level.Error);
            }
        }

        [TestMethod]
        public void Log_Error_Long()
        {
            _logger.Error(LongMessage);
            Validate(LongMessage, Utility.Level.Error);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_Warning_Null()
        {
            _logger.Warning(null);
        }

        [TestMethod]
        public void Log_Warning()
        {
            foreach (string message in _sampleMessages)
            {
                _logger.Warning(message);
                Validate(message, Utility.Level.Warning);
            }
        }

        [TestMethod]
        public void Log_Warning_Long()
        {
            _logger.Warning(LongMessage);
            Validate(LongMessage, Utility.Level.Warning);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_Info_Null()
        {
            _logger.Info(null);
        }

        [TestMethod]
        public void Log_Info()
        {
            foreach (string message in _sampleMessages)
            {
                _logger.Info(message);
                Validate(message, Utility.Level.Info);
            }
        }

        [TestMethod]
        public void Log_Info_Long()
        {
            _logger.Info(LongMessage);
            Validate(LongMessage, Utility.Level.Info);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Log_Verbose_Null()
        {
            _logger.Verbose(null);
        }

        [TestMethod]
        public void Log_Verbose()
        {
            foreach (string message in _sampleMessages)
            {
                _logger.Verbose(message);
                Validate(message, Utility.Level.Verbose);
            }
        }

        [TestMethod]
        public void Log_Verbose_Long()
        {
            _logger.Verbose(LongMessage);
            Validate(LongMessage, Utility.Level.Verbose);
        }

        private static void Validate(string message, Utility.Level level)
        {
            if (message.Length > LogMessage.MaxMessageLength)
            {
                message = message.Substring(0, LogMessage.MaxMessageLength);
            }

            int levelInt = (int) level;

            LogMessage lastMessage = _collection.Take();

            Assert.IsNotNull(lastMessage);
            Assert.IsInstanceOfType(lastMessage.Time, typeof(DateTime));
            Assert.IsTrue(lastMessage.Time.Subtract(DateTime.UtcNow).TotalMinutes < 1);

            Assert.IsTrue(lastMessage.MessageJson.Length > 0);
            Assert.IsTrue(lastMessage.MessageJson.Contains(Utility.GetJsonRepresentation(LogMessage.MessageKey, message)));
            Assert.IsTrue(lastMessage.MessageJson.Contains(Utility.GetJsonRepresentation(LogMessage.LevelKey, levelInt)));

            Dictionary<string, object> jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(lastMessage.GetJson());
            Assert.IsNotNull(jsonResult);
            Assert.IsTrue(jsonResult.ContainsKey(LogMessage.MessageKey));
            Assert.AreEqual(jsonResult[LogMessage.MessageKey], message);
            Assert.IsTrue(jsonResult.ContainsKey(LogMessage.LevelKey));
            Assert.AreEqual(jsonResult[LogMessage.LevelKey], levelInt.ToString());
            Assert.IsTrue(jsonResult.ContainsKey(LogMessage.LevelKey + Utility.OdataTypeKeySuffix));
            Assert.AreEqual(jsonResult[LogMessage.LevelKey + Utility.OdataTypeKeySuffix],
                Utility.OdataTypeValuePrefix + levelInt.GetType().Name);
        }
    }
}