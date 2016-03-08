using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;

namespace Pour.Core.Library.Test
{
    [TestClass]
    public class LibraryComparisonTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            LogManager.SetContext("SomeFieldBool", true);
            LogManager.SetContext("SomeFieldInt", 0);
            LogManager.SetContext("SomeFieldString", "SomeValue");

            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference("people");
            table.CreateIfNotExists();
        }
    }
}