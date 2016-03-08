using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Pour.Client.Library
{
    internal static class HttpHelper
    {
        #region Private Constants

        private const string AcceptHeaderValue = "application/json;odata=nometadata";

        private const string AcceptCharsetHeaderKey = "Accept-Charset";

        private const string AcceptCharsetHeaderValue = "UTF-8";

        private const string AuthorizationHeaderKey = "Authorization";

        private const string AuthorizationHeaderRawValueFormat = "{0}\n\n{1}\n{2}\n{3}";

        private const string AuthorizationHeaderValueFormat = "SharedKey {0}:{1}";

        private const string MsHeaderKeyPrefix = "x-ms-";

        private const string MsDateHeaderKey = "x-ms-date";

        private const string MsVersionHeaderKey = "x-ms-version";

        private const string MsVersionHeaderValue = "2013-08-15";

        private const string DataServiceVersionHeaderKey = "DataServiceVersion";

        private const string MaxDataServiceVersionHeaderKey = "MaxDataServiceVersion";

        private const string DataServiceVersionHeaderValue = "3.0;NetFx";

        #endregion

        #region Settings

        internal static bool RetrieveHeaders { get; set; }

        #endregion

        internal static ApiResponse Request(string account,
            string uri,
            string method,
            string body,
            string contentType,
            HMACSHA256 encryption,
            string ifMatch)
        {
            ApiResponse apiResponse = new ApiResponse();

            try
            {
                ServicePointManager.DefaultConnectionLimit = 4;
                HttpWebRequest request = CreateRestRequest(account, Uri.EscapeUriString(uri), method, body, contentType, encryption, ifMatch);
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();

                // Copy custom key (the ones start with x-ms-*)
                if (RetrieveHeaders && response != null && response.Headers != null && response.Headers.Keys != null)
                {
                    apiResponse.Headers = new Dictionary<string, string>();

                    for (int i = 0; i < response.Headers.AllKeys.Length; i++)
                    {
                        if (response.Headers.AllKeys[i].StartsWith(MsHeaderKeyPrefix))
                        {
                            apiResponse.Headers.Add(response.Headers.AllKeys[i], response.Headers[response.Headers.AllKeys[i]]);
                        }
                    }
                }
                
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    apiResponse.ResponseBody = reader.ReadToEnd();
                }

                response.Close();
                apiResponse.Succedded = true;
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    apiResponse.Status = ((HttpWebResponse)e.Response).StatusCode;

                    using (StreamReader reader = new StreamReader(e.Response.GetResponseStream()))
                    {
                        apiResponse.ErrorMessage = reader.ReadToEnd();
                    }
                }
                else
                {
                    apiResponse.ErrorMessage = e.Message;
                }
            }
            catch (Exception e)
            {
                apiResponse.ErrorMessage = e.Message;
            }

            return apiResponse;
        }

        private static HttpWebRequest CreateRestRequest(string account,
            string uri,
            string method,
            string body,
            string contentType,
            HMACSHA256 encryption,
            string ifMatch)
        {
            // Create the request and set proxy to null
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(uri);
            request.Proxy = null;

            // Set the method and content type
            request.Method = method;
            request.ContentType = contentType;

            // Let this match with anything
            if (!string.IsNullOrWhiteSpace(ifMatch))
            {
                request.Headers.Add("If-Match", "*");
            }

            // Set the headers
            request.Accept = AcceptHeaderValue;
            request.Headers.Add(MsDateHeaderKey, DateTime.UtcNow.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            request.Headers.Add(MsVersionHeaderKey, MsVersionHeaderValue);
            request.Headers.Add(DataServiceVersionHeaderKey, DataServiceVersionHeaderValue);
            request.Headers.Add(MaxDataServiceVersionHeaderKey, DataServiceVersionHeaderValue);

            // Set authorization header
            if (encryption != null)
            {
                string cannonicalResource = GetCannonicalResource(account, request.RequestUri);
                string stringToSign = string.Format(AuthorizationHeaderRawValueFormat, 
                    request.Method, request.ContentType, request.Headers[MsDateHeaderKey], cannonicalResource);
                request.Headers.Add(AuthorizationHeaderKey,
                    string.Format(AuthorizationHeaderValueFormat, account, Encrypt(stringToSign, encryption)));
            }

            // Set the body and content-length if needed
            bool hasBody = !string.IsNullOrWhiteSpace(body);
            if (hasBody)
            {
                request.Headers.Add(AcceptCharsetHeaderKey, AcceptCharsetHeaderValue);
                byte[] byteArray = Encoding.UTF8.GetBytes(body);
                request.ContentLength = byteArray.Length;
                Stream bodyDataStream = request.GetRequestStream();
                bodyDataStream.Write(byteArray, 0, byteArray.Length);
                bodyDataStream.Close();
            }

            ServicePointManager.Expect100Continue = hasBody;

            return request;
        }

        private static string GetCannonicalResource(string account, Uri uri)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("/{0}{1}", account, uri.AbsolutePath);

            if (uri.Query.Contains("comp"))
            {
                NameValueCollection parameters = HttpUtility.ParseQueryString(uri.Query);
                builder.AppendFormat("?comp={0}", parameters["comp"]);
            }

            return builder.ToString();
        }

        private static string Encrypt(string stringToSign, HMACSHA256 sha256)
        {
            byte[] signatureBytes = Encoding.UTF8.GetBytes(stringToSign);
            return Convert.ToBase64String(sha256.ComputeHash(signatureBytes));
        }
    }
}
