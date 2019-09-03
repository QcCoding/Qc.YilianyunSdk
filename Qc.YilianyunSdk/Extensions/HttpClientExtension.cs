using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Qc.YilianyunSdk
{
    public static class HttpClientExtension
    {

        public static T HttpPost<T>(this HttpClient client, string url, string postData = null, string contentType = "application/x-www-form-urlencoded")
        {
            postData = postData ?? string.Empty;
            var httpContent = new StringContent(postData, Encoding.UTF8);
            if (contentType != null)
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            HttpResponseMessage response = client.PostAsync(url, httpContent).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            return Utils.JsonHelper.Deserialize<T>(result);
        }
        public static T HttpPost<T>(this HttpClient client, string url, Dictionary<string, object> dicData)
        {
            string postDataStr = string.Empty;
            int i = 0;
            foreach (var item in dicData)
            {
                if (i++ > 0)
                {
                    postDataStr += "&";
                }
                postDataStr += $"{item.Key}={item.Value}";
            }
            return client.HttpPost<T>(url, postDataStr);
        }
    }
}
