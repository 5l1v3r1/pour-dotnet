using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System;

namespace Pour.Client.Library.Test.Integration
{
    [TestClass]
    public class TableApiManagerCloudTest : TableApiManagerTestBase
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Account = "pourtest";
            Key = "4iYhBQC8le0aXjUBDTXd61JaL+532uTwCGrrhXMtk4cr4+B0naa9B8VPSjQpvat9UQnaSlFILABuWtrdgAHvpw==";
            AccountUri = ApiHelper.GetAccountUri(Account);
            TablesUri = ApiHelper.GetTablesUri(Account);
            SignMethod = new HMACSHA256(Convert.FromBase64String(Key));

            ApiHelper.Validate(Account, Key, TablesUri);
        }
    }
}
