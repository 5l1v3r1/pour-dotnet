using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Pour.Client.Library;
using System;
using System.Diagnostics;
using System.Threading;

namespace Pour.Comparison
{
    [TestClass]
    public class AzureComparisonTests
    {
        private static int _id = 0;

        private const int Count = 100;

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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=pourtest;AccountKey=4iYhBQC8le0aXjUBDTXd61JaL+532uTwCGrrhXMtk4cr4+B0naa9B8VPSjQpvat9UQnaSlFILABuWtrdgAHvpw==");
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
            LogManager.Connect("28a96c5f85ee2e2c912f9d5fc97818075loFm05qhhyDXj9OfzZBpBllEFzNU4Dx");
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
