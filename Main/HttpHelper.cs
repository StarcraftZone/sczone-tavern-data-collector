using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SczoneTavernDataCollector.Main
{

    public class HttpHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static void Init()
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.Timeout = new TimeSpan(0, 0, 60);
        }

        public static dynamic Get(String url, List<KeyValuePair<string, string>> headers = null)
        {
            try
            {
                var random = new Random().Next(10000);
                Global.Log.Info("发送请求: " + random + ", GET: " + url);
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (client.DefaultRequestHeaders.Contains(header.Key))
                        {
                            client.DefaultRequestHeaders.Remove(header.Key);
                        }
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                var getTask = client.GetAsync(url);
                var readTask = getTask.Result.Content.ReadAsStringAsync();
                var result = readTask.Result;
                Global.Log.Info("收到返回: " + random + ", GET: " + url + ", " + getTask.Status + "," + getTask.Result.StatusCode + ", Response: " + result);
                return JObject.Parse(result);
            }
            catch (Exception ex)
            {
                Global.Log.Error(ex);
            }
            return null;
        }

        public static dynamic Post(String url, object param, List<KeyValuePair<string, string>> headers = null)
        {
            try
            {
                var random = new Random().Next(10000);
                var json = JsonConvert.SerializeObject(param);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Global.Log.Info("发送请求: " + random + ", POST: " + url + ", Request: " + json);

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        if (client.DefaultRequestHeaders.Contains(header.Key))
                        {
                            client.DefaultRequestHeaders.Remove(header.Key);
                        }
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                var postTask = client.PostAsync(url, content);
                var readTask = postTask.Result.Content.ReadAsStringAsync();
                var result = readTask.Result;

                Global.Log.Info("收到返回: " + random + ", POST: " + url + ", " + postTask.Status + "," + postTask.Result.StatusCode + ", Response: " + result);
                return JObject.Parse(result);
            }
            catch (Exception ex)
            {
                Global.Log.Error(ex);
            }
            return null;
        }
    }
}
