using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace Pour.Client.Library.Test.Integration
{
    [TestClass]
    public abstract class TableApiManagerTestBase
    {
        protected static string Account;

        protected static string Key;

        protected static string AccountUri;

        protected static string TablesUri;

        protected static HMACSHA256 SignMethod;

        private const string TestTableName = "TestTable";

        [TestMethod]
        public void GetTables()
        {
            ApiResponse response = ApiHelper.GetTables(Account, Key, TablesUri, SignMethod);
            ValidateSuccess(response);
        }

        [TestMethod]
        public void CreateTable_Existing_Success()
        {
            ApiResponse response = ApiHelper.GetTables(Account, Key, TablesUri, SignMethod);
            if (!response.HasTable(TestTableName))
            {
                response = ApiHelper.CreateTableIfNotExists(Account, Key, TablesUri, SignMethod, TestTableName);
                ValidateSuccess(response);
            }

            response = ApiHelper.CreateTableIfNotExists(Account, Key, TablesUri, SignMethod, TestTableName);
            ValidateSuccess(response);
        }

        [TestMethod]
        public void CreateTable_NonExisting_Success()
        {
            ApiResponse response = ApiHelper.GetTables(Account, Key, TablesUri, SignMethod);
            if (response.HasTable(TestTableName))
            {
                response = ApiHelper.DeleteTable(Account, Key, TablesUri, SignMethod, TestTableName);
                ValidateSuccess(response);
            }

            response = ApiHelper.GetTables(Account, Key, TablesUri, SignMethod);
            Assert.IsFalse(response.HasTable(TestTableName));

            response = ApiHelper.CreateTableIfNotExists(Account, Key, TablesUri, SignMethod, TestTableName);
            ValidateSuccess(response);
        }

        [TestMethod]
        public void DeleteTable_NonExisting_Failure()
        {
            ApiResponse response = ApiHelper.DeleteTable(Account, Key, TablesUri, SignMethod, "NonExistingTable");
            ValidateFailure(response);
        }

        [TestMethod]
        public void DeleteTable_Existing_Success()
        {
            ApiResponse response = ApiHelper.GetTables(Account, Key, TablesUri, SignMethod);
            if (!response.HasTable(TestTableName))
            {
                response = ApiHelper.CreateTableIfNotExists(Account, Key, TablesUri, SignMethod, TestTableName);
                ValidateSuccess(response);
            }
            
            response = ApiHelper.GetTables(Account, Key, TablesUri, SignMethod);
            Assert.IsTrue(response.HasTable(TestTableName));

            response = ApiHelper.DeleteTable(Account, Key, TablesUri, SignMethod, TestTableName);
            ValidateSuccess(response);
        }

        [TestMethod]
        public void InsertEntity_Success()
        {
            string tableName = "tabletoinsertentity";
            string tableUri = ApiHelper.GetUri(AccountUri, "tabletoinsertentity");
            ApiResponse response = ApiHelper.CreateTableIfNotExists(Account, Key, TablesUri, SignMethod, tableName);
            ValidateSuccess(response);

            LogMessage logMessage = new LogMessage("Test message", Utility.Level.Critical);
            string fullMessage = logMessage.GetJson();
            response = ApiHelper.InsertEntity(Account, Key, tableUri, SignMethod, fullMessage);
            ValidateSuccess(response);

            string rowKey = logMessage.RowKey;
            string partitionKey = Utility.GetPartitionKey(rowKey);
            response = ApiHelper.GetEntity(Account, Key, tableUri, SignMethod, partitionKey, rowKey);
            ValidateSuccess(response);

            Dictionary<string, object> jsonResult = 
                JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ResponseBody);
            Assert.IsNotNull(jsonResult);
            Assert.IsTrue(jsonResult.ContainsKey("PartitionKey"));
            Assert.AreEqual(jsonResult["PartitionKey"], partitionKey);
            Assert.IsTrue(jsonResult.ContainsKey("RowKey"));
            Assert.AreEqual(jsonResult["RowKey"], rowKey);
            Assert.IsTrue(jsonResult.ContainsKey(LogMessage.EventTimeKey));
            Assert.AreEqual(jsonResult[LogMessage.EventTimeKey], logMessage.Time);

            response = ApiHelper.DeleteTable(Account, Key, TablesUri, SignMethod, tableName);
            ValidateSuccess(response);
        }

        [TestMethod]
        public void MergeEntity_Success()
        {
            string tableName = "tabletoinsertentity";
            string tableUri = ApiHelper.GetUri(AccountUri, "tabletoinsertentity");
            ApiResponse response = ApiHelper.CreateTableIfNotExists(Account, Key, TablesUri, SignMethod, tableName);
            ValidateSuccess(response);

            LogMessage logMessage = new LogMessage("Test message", Utility.Level.Critical);
            string fullMessage = logMessage.GetJson();
            string rowKey = logMessage.RowKey;
            string partitionKey = Utility.GetPartitionKey(rowKey);
            response = ApiHelper.MergeEntity(Account, Key, tableUri, SignMethod, fullMessage, partitionKey, rowKey);
            ValidateSuccess(response);

            response = ApiHelper.GetEntity(Account, Key, tableUri, SignMethod, partitionKey, rowKey);
            ValidateSuccess(response);

            Dictionary<string, object> jsonResult =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ResponseBody);
            Assert.IsNotNull(jsonResult);
            Assert.IsTrue(jsonResult.ContainsKey("PartitionKey"));
            Assert.AreEqual(jsonResult["PartitionKey"], partitionKey);
            Assert.IsTrue(jsonResult.ContainsKey("RowKey"));
            Assert.AreEqual(jsonResult["RowKey"], rowKey);
            Assert.IsTrue(jsonResult.ContainsKey(LogMessage.EventTimeKey));
            Assert.AreEqual(jsonResult[LogMessage.EventTimeKey], logMessage.Time);

            response = ApiHelper.DeleteTable(Account, Key, TablesUri, SignMethod, tableName);
            ValidateSuccess(response);
        }

        [TestMethod]
        public void QueryEntity_Success()
        {
            string tableName = "tabletoinsertentity";
            string tableUri = ApiHelper.GetUri(AccountUri, "tabletoinsertentity");
            ApiResponse response = ApiHelper.CreateTableIfNotExists(Account, Key, TablesUri, SignMethod, tableName);
            ValidateSuccess(response);
            response.HasTable(TestTableName);

            LogMessage logMessage = new LogMessage("Test message", Utility.Level.Critical);
            string fullMessage = logMessage.GetJson();
            string rowKey = logMessage.RowKey;
            string partitionKey = Utility.GetPartitionKey(rowKey);
            response = ApiHelper.MergeEntity(Account, Key, tableUri, SignMethod, fullMessage, partitionKey, rowKey);
            ValidateSuccess(response);

            response = ApiHelper.Query(Account, Key, tableUri, SignMethod, 
                string.Format("(PartitionKey eq '{0}') and (RowKey le '{1}')", partitionKey, rowKey));
            ValidateSuccess(response);

            Dictionary<string, object> jsonResult =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(response.ResponseBody);
            Assert.IsNotNull(jsonResult);
            Assert.IsTrue(jsonResult.ContainsKey("value"));
            object valueResult = (jsonResult["value"] as Array);

            response = ApiHelper.DeleteTable(Account, Key, TablesUri, SignMethod, tableName);
            ValidateSuccess(response);
        }

        [TestMethod]
        public void EnableCors_Success()
        {
            ApiResponse response = ApiHelper.EnableCors(Account, Key, "http://localhost:6091");
            ValidateSuccess(response);
        }

        [TestMethod]
        public void DisableCors_Success()
        {
            ApiResponse response = ApiHelper.DisableCors(Account, Key);
            ValidateSuccess(response);
        }

        private static void ValidateSuccess(ApiResponse response)
        {
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Succedded);
            Assert.IsTrue(string.IsNullOrWhiteSpace(response.ErrorMessage));
        }

        private static void ValidateFailure(ApiResponse response)
        {
            Assert.IsFalse(response.Succedded);
            Assert.IsFalse(string.IsNullOrWhiteSpace(response.ErrorMessage));
            Assert.IsTrue(string.IsNullOrWhiteSpace(response.ResponseBody));
        }
    }
}
