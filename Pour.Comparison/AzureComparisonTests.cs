using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Pour.Client.Library;

namespace Pour.Comparison
{
    [TestClass]
    public class AzureComparisonTests
    {
        private static int _id = 0;

        private const int Count = 250;

        public class SampleEntity : TableEntity
        {
            public SampleEntity(string message, string someString, int someInt)
            {
                RowKey = string.Format("{0}-{1}", DateTime.MaxValue.Subtract(DateTime.UtcNow).Ticks, Interlocked.Increment(ref _id));
                PartitionKey = Utility.GetPartitionKey(RowKey);
                Message = message;
                SomeStringValue = someString;
                SomeIntValue = someInt;
            }

            public SampleEntity() { }

            public string Message { get; set; }

            public string SomeStringValue { get; set; }

            public int SomeIntValue { get; set; }
        }

        [TestMethod]
        public void Comparison_Write()
        {
            string message = "Sample log message";
            Utility.Level someLevel = Utility.Level.Info;

            Stopwatch sw = Stopwatch.StartNew();

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["Azure.ConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference("comparisonazure");
            table.CreateIfNotExists();

            sw.Stop();
            Console.WriteLine("Azure - Setup: " + sw.ElapsedMilliseconds);
            sw.Restart();

            // Create a new customer entity.
            for (int i = 0; i < Count; i++)
            {
                SampleEntity sampleEntity = new SampleEntity(message, "SomeStringValue", (int)someLevel);
                TableOperation insertOperation = TableOperation.Insert(sampleEntity);
                table.Execute(insertOperation);
            }

            sw.Stop();
            Debug.WriteLine("Azure - Insert: " + sw.ElapsedMilliseconds);
            sw.Restart();

            // Connect
            LogManager.Connect(ConfigurationManager.AppSettings["Pour.Token"]);
            LogManager.SetContext("SomeStringValue", "SomeStringValue");
            //string tableUri = ApiHelper.GetUri(LogManager.AccountUri, "comparisonpour");

            sw.Stop();
            Debug.WriteLine("Pour - Setup: " + sw.ElapsedMilliseconds);
            sw.Restart();

            for (int i = 0; i < Count; i++)
            {
                LogMessage nextMessage = new LogMessage(message, someLevel, Interlocked.Increment(ref _id));
                string fullMessage = nextMessage.GetJson(LogManager.ContextJson);
                ApiResponse response = ApiHelper.InsertEntity(LogManager.Account,
                    LogManager.Key,
                    LogManager.LogTableUri,
                    LogManager.SignMethod,
                    fullMessage);
            }

            sw.Stop();
            Debug.WriteLine("Pour - Insert: " + sw.ElapsedMilliseconds);
        }
    }
}
