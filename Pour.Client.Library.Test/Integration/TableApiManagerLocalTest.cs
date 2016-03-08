using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;

namespace Pour.Client.Library.Test.Integration
{
    [TestClass]
    public class TableApiManagerLocalTest : TableApiManagerTestBase
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Account = LogManager.EmulatorAccount;
            Key = LogManager.EmulatorKey;
            AccountUri = ApiHelper.GetAccountUri(Account, LogManager.EmulatorUri);
            TablesUri = ApiHelper.GetUri(AccountUri, "Tables");
            SignMethod = new HMACSHA256(Convert.FromBase64String(Key));

            AzureStorageEmulatorManager.Start();

            ApiHelper.Validate(Account, Key, TablesUri);
        }

        [ClassCleanup]
        public static void CleanUp()
        {
            AzureStorageEmulatorManager.Stop();
        }
    }
}
