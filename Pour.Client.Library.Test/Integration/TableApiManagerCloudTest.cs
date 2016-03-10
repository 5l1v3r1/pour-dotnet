using System;
using System.Configuration;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pour.Client.Library.Test.Integration
{
    [TestClass]
    public class TableApiManagerCloudTest : TableApiManagerTestBase
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Account = ConfigurationManager.AppSettings["AzureStorage.Account"];
            Key = ConfigurationManager.AppSettings["AzureStorage.Key"];
            AccountUri = ApiHelper.GetAccountUri(Account);
            TablesUri = ApiHelper.GetTablesUri(Account);
            SignMethod = new HMACSHA256(Convert.FromBase64String(Key));

            ApiHelper.Validate(Account, Key, TablesUri);
        }
    }
}
