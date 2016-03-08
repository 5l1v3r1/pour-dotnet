using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Pour.Client.Library.Test.Integration;

namespace Pour.Client.Library.Test
{
    [TestClass]
    public class LogManagerTest
    {
        private static LogMessage SampleLogMessage = new LogMessage("some message", Utility.Level.Info);

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            AzureStorageEmulatorManager.Start();
            LogManager.Connect();
        }

        [ClassCleanup]
        public static void CleanUp()
        {
            AzureStorageEmulatorManager.Stop();
        }

        [TestMethod]
        public void GetContext_NonExisting()
        {
            string name = "Some Name" + Guid.NewGuid();
            object value = LogManager.GetContext(name);
            Assert.IsNull(value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetContext_Null()
        {
            string name = "Some Name" + Guid.NewGuid();
            LogManager.SetContext(name, null);
            Assert.AreEqual(null, LogManager.GetContext(name));
        }

        [TestMethod]
        public void SetContext_Bool_True()
        {
            string name = "Some Name" + Guid.NewGuid();
            bool value = true;
            LogManager.SetContext(name, value);
            Validate(name, value);
        }

        [TestMethod]
        public void SetContext_Bool_False()
        {
            string name = "Some Name" + Guid.NewGuid();
            bool value = false;
            LogManager.SetContext(name, value);
            Validate(name, value);
        }

        [TestMethod]
        public void SetContext_Int()
        {
            string name = "Some Name" + Guid.NewGuid();
            int value = new Random().Next(int.MinValue, int.MaxValue);
            LogManager.SetContext(name, value);
            Validate(name, value);
        }

        [TestMethod]
        public void SetContext_Double()
        {
            string name = "Some Name" + Guid.NewGuid();
            double value = new Random().NextDouble();
            LogManager.SetContext(name, value);
            Validate(name, value);
        }

        [TestMethod]
        public void SetContext_DateTime()
        {
            string name = "Some Name" + Guid.NewGuid();
            DateTime value = DateTime.UtcNow;
            LogManager.SetContext(name, value);
            Validate(name, value);
        }

        [TestMethod]
        public void SetContext_Guid()
        {
            string name = "Some Name" + Guid.NewGuid();
            Guid value = Guid.NewGuid();
            LogManager.SetContext(name, value);
            Validate(name, value);
        }

        [TestMethod]
        public void SetContext_String()
        {
            string name = "Some Name" + Guid.NewGuid();
            string value = "Some value";
            LogManager.SetContext(name, value);
            Validate(name, value);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FreeContext_NonExisting()
        {
            string name = "Some Name" + Guid.NewGuid();
            LogManager.FreeContext(name);
        }

        [TestMethod]
        public void FreeContext_Existing()
        {
            string name = "Some Name" + Guid.NewGuid();

            object value = LogManager.GetContext(name);
            Assert.IsNull(value);

            string newValue = "Some value";
            LogManager.SetContext(name, newValue);

            value = LogManager.GetContext(name);
            Assert.IsNotNull(value);

            LogManager.FreeContext(name);
            value = LogManager.GetContext(name);
            Assert.IsNull(value);
        }

        private static void Validate(string name, object value)
        {
            Assert.AreEqual(value, LogManager.GetContext(name));

            string contextJson = LogManager.ContextJson;
            string contextName = Utility.GetContextName(name);
            Assert.IsNotNull(contextJson);
            Assert.IsTrue(contextJson.Length > 0);
            Assert.IsTrue(contextJson.Contains(Utility.GetJsonRepresentation(contextName, value)));

            Dictionary<string, object> jsonResult = JsonConvert.DeserializeObject<Dictionary<string, object>>(SampleLogMessage.GetJson(contextJson));
            Assert.IsNotNull(jsonResult);
            Assert.IsTrue(jsonResult.ContainsKey(contextName));
            Assert.AreEqual(jsonResult[contextName], value.ToString());
            
            if (!(value is string))
            {
                Assert.IsTrue(jsonResult.ContainsKey(contextName + Utility.OdataTypeKeySuffix));
                Assert.AreEqual(jsonResult[contextName + Utility.OdataTypeKeySuffix],
                    Utility.OdataTypeValuePrefix + value.GetType().Name);
            }
        }
    }
}
