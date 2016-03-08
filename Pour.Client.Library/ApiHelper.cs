using System;
using System.Security.Cryptography;

namespace Pour.Client.Library
{
    internal static class ApiHelper
    {
        #region Private Constants

        private const string DeleteTableUriFormat = "{0}('{1}')";

        private const string TableBodyFormat = "\"{0}\"";

        private const string AddTableBodyFormat = "{{ \"TableName\": \"{0}\" }}";

        private const string KeyFilteredTableUriFormat = "{0}(PartitionKey='{1}',RowKey='{2}')";

        private const string FilteredTableUriFormat = "{0}()?$filter={1}&$top={2}";

        private const string FilteredTableUriFormatWithContinuation = "{0}()?$filter={1}&$top={2}&NextPartitionKey={3}&NextRowKey={4}";

        private const int MaxNumberOfEntities = 100;

        private const string NotConnectedErrorMessage = "Looks like you haven't connected to your storage account yet.";

        private const string AccountUriFormat = "https://{0}.table.core.windows.net/";

        private const string CorsUrlFormat = "https://{0}.table.core.windows.net/?restype=service&comp=properties";

        private const string CorsEnableBodyFormat = @"<?xml version='1.0' encoding='utf-8'?>
            <StorageServiceProperties>
                <Cors>
                    <CorsRule>
                        <AllowedOrigins>{0}</AllowedOrigins>
                        <AllowedMethods>POST</AllowedMethods>
                        <MaxAgeInSeconds>3600</MaxAgeInSeconds>
                        <ExposedHeaders>x-ms-*</ExposedHeaders>
                        <AllowedHeaders>accept, authorization, content-type, If-Match, x-ms-*, DataServiceVersion, MaxDataServiceVersion</AllowedHeaders>
                    </CorsRule>
                </Cors>
            </StorageServiceProperties>";

        private const string CorsDisableBodyFormat = @"<?xml version='1.0' encoding='utf-8'?>
            <StorageServiceProperties></StorageServiceProperties>";

        #endregion

        internal const string DefaultTablesSegmentName = "Tables";

        internal const string DefaultLogsTableName = "Logs";

        internal static ApiResponse Validate(string account, string key, string uri)
        {
            account.RequireNonEmpty("account");
            key.RequireNonEmpty("key");
            uri.RequireNonEmpty("uri");
            
            HMACSHA256 sha256 = new HMACSHA256(Convert.FromBase64String(key));
            ApiResponse response = Request(account, key, uri, sha256, "GET");
            if (!response.Succedded)
            {
                throw new InvalidOperationException("The storage account credentials are invalid. Error: " + response.ErrorMessage);
            }

            return response;
        }

        internal static ApiResponse GetTables(string account, 
            string key, 
            string uri, 
            HMACSHA256 sign)
        {
            return Request(account, key, uri, sign, "GET");
        }

        internal static ApiResponse CreateTableIfNotExists(string account, 
            string key, 
            string uri,
            HMACSHA256 sign,
            string name)
        {
            ApiResponse response = Request(account, key, uri, sign, "POST", string.Format(AddTableBodyFormat, name));

            // If failed due to conflict (table exists already), then this is still success
            if (!response.Succedded && response.Status == System.Net.HttpStatusCode.Conflict)
            {
                return new ApiResponse { Succedded = true };
            }

            return response;
        }

        internal static bool HasTable(this ApiResponse response, string name)
        {
            return response.Succedded &&
                string.IsNullOrWhiteSpace(response.ErrorMessage) &&
                !string.IsNullOrWhiteSpace(response.ResponseBody) &&
                response.ResponseBody.Contains(string.Format(TableBodyFormat, name));
        }

        internal static ApiResponse DeleteTable(string account, 
            string key, 
            string uri,
            HMACSHA256 sign,
            string name)
        {
            return Request(account, key, string.Format(DeleteTableUriFormat, uri, name), sign, "DELETE", "", "application/atom+xml");
        }

        internal static ApiResponse InsertEntity(string account, 
            string key, 
            string uri,
            HMACSHA256 sign, 
            string body)
        {
            return Request(account, key, uri, sign, "POST", body);
        }

        internal static ApiResponse MergeEntity(string account, 
            string key, 
            string uri,
            HMACSHA256 sign, 
            string body, 
            string partitionKey, 
            string rowKey)
        {
            return Request(account, key, string.Format(KeyFilteredTableUriFormat, uri, partitionKey, rowKey), sign, "MERGE", body);
        }

        internal static ApiResponse DeleteEntity(string account, 
            string key, 
            string uri,
            HMACSHA256 sign,
            string partitionKey, 
            string rowKey)
        {
            return Request(account, key, string.Format(KeyFilteredTableUriFormat, uri, partitionKey, rowKey), sign, "DELETE", "", "application/atom+xml",  "*");
        }

        internal static ApiResponse GetEntity(string account, 
            string key, 
            string uri,
            HMACSHA256 sign, 
            string partitionKey, 
            string rowKey)
        {
            return Request(account, key,  string.Format(KeyFilteredTableUriFormat, uri, partitionKey, rowKey), sign);
        }

        internal static ApiResponse Query(string account, 
            string key, 
            string uri,
            HMACSHA256 sign, 
            string filter,
            int count = MaxNumberOfEntities,
            string nextPartitionKey = "",
            string nextRowKey = "")
        {
            string fullUri = (string.IsNullOrWhiteSpace(nextPartitionKey) || string.IsNullOrWhiteSpace(nextRowKey)) ?
                string.Format(FilteredTableUriFormat, uri, filter, count) :
                string.Format(FilteredTableUriFormatWithContinuation, uri, filter, count, nextPartitionKey, nextRowKey);
            return Request(account, key, fullUri, sign);
        }

        internal static ApiResponse EnableCors(string account,
            string key,
            string url)
        {
            HMACSHA256 sha256 = new HMACSHA256(Convert.FromBase64String(key));
            return Request(account, key, string.Format(CorsUrlFormat, account), sha256, "PUT", 
                string.Format(CorsEnableBodyFormat, url), "application/xml");
        }

        internal static ApiResponse DisableCors(string account,
            string key)
        {
            HMACSHA256 sha256 = new HMACSHA256(Convert.FromBase64String(key));
            return Request(account, key, string.Format(CorsUrlFormat, account), sha256, "PUT",
                CorsDisableBodyFormat, "application/xml");
        }

        internal static string GetAccountUri(string account, string uri = "")
        {
            return string.IsNullOrWhiteSpace(uri) ? string.Format(AccountUriFormat, account) : uri;
        }

        internal static string GetTablesUri(string account)
        {
            return GetUri(GetAccountUri(account), DefaultTablesSegmentName);
        }

        internal static string GetUri(string accountUri, string segment)
        {
            return string.Format("{0}{1}", accountUri, segment);
        }

        private static ApiResponse Request(string account, 
            string key, 
            string uri, 
            HMACSHA256 signAlgorithm,
            string method = "GET", 
            string body = "",
            string contentType = "application/json",
            string ifMatch = "") 
        {
            account.RequireNonEmpty("account", NotConnectedErrorMessage);
            key.RequireNonEmpty("key", NotConnectedErrorMessage);
            uri.RequireNonEmpty("uri", NotConnectedErrorMessage);
            return HttpHelper.Request(account, uri, method, body, contentType, signAlgorithm, ifMatch);
        }
    } 
}
