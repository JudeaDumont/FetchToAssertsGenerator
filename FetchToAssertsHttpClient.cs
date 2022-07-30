using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tests.Common
{
    public static class FetchToAssertsHttpClient
    {
        public const string BASE_URL = "BASE_URL";
        public const string BASE_URL_POSTFIX = "/postfix";

        public static string BASE_URL { get; set; }
        public static string ROOT_URL { get; set; }
        
        static FetchToAssertsHttpClient()
        {
            if (( BASE_URL = Environment.GetEnvironmentVariable(BASE_URL) ) == null)
            {
                BASE_URL = "https://localhost";
            }
            ROOT_URL = BASE_URL + BASE_URL_POSTFIX;
            BASE_URL = BASE_URL;
        }
        
        public static async Task<T> Get<T>(string url, string preAPIURL = "") where T : class
        {
            using (var httpClient = GetFetchAndAssertsHttpClient())
            {
                var response = await httpClient.GetAsync(BASE_URL + preAPIURL + BASE_URL_POSTFIX + url);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }
        
        public static async Task<T> Put<T>(string url, string contentResponseBody, string preAPIURL = "") where T : class
        {
            using (var httpClient = GetFetchAndAssertsHttpClient())
            {
                var response = await httpClient.PutAsync(BASE_URL + preAPIURL + BASE_URL_POSTFIX + url,
                    new StringContent(contentResponseBody, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }

        public static async Task<T> PostFile<T>(string url, string fileName, string preAPIURL = "") where T : class
        {
            using (var httpClient = GetFetchAndAssertsHttpClient())
            {

                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);
                var multiForm = new MultipartFormDataContent();
                FileStream fs = File.OpenRead(path);
                multiForm.Add(new StreamContent(fs), "file", Path.GetFileName(path));
                var response = await httpClient.PostAsync(BASE_URL + preAPIURL + BASE_URL_POSTFIX + url, multiForm);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }

        public static async Task<T> PostMultiPartForm<T>(string url, MultipartFormDataContent content, string preAPIURL = "") where T : class
        {
            using (var httpClient = GetFetchAndAssertsHttpClient())
            {
                var response = await httpClient.PostAsync(BASE_URL + preAPIURL + BASE_URL_POSTFIX + url, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }

        public static async Task<T> Post<T>(string url, string contentResponseBody, string preAPIURL = "") where T : class
        {
            using (var httpClient = GetFetchAndAssertsHttpClient())
            {
                var response = await httpClient.PostAsync(BASE_URL + preAPIURL + BASE_URL_POSTFIX + url,
                    new StringContent(contentResponseBody, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }

        public static async Task<string> Delete(string url, string id)
        {
            using (var httpClient = GetFetchAndAssertsHttpClient())
            {
                var response = await httpClient.DeleteAsync(ROOT_URL + url + id);
                response.EnsureSuccessStatusCode();
                return response.StatusCode.ToString();
            }
        }

        public static async Task<Dictionary<string, string>> PostAndGetError(string url, string contentResponseBody)
        {
            using (var httpClient = GetFetchAndAssertsHttpClient())
            {
                var response = await httpClient.PostAsync(ROOT_URL + url,
                    new StringContent(contentResponseBody, Encoding.UTF8, "application/json"));
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
            }
        }

        public static HttpClient GetFetchAndAssertsHttpClient()
        {
            var httpClientHandler = new HttpClientHandler();

            httpClientHandler.UseDefaultCredentials = true;
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
            httpClientHandler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, certChain, policyErrors) => true;

            var httpClient = new HttpClient(httpClientHandler);

            httpClient.Timeout = TimeSpan.FromMinutes(16);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }

        public static async Task<string> PostWithCred(string username, string password, string domain, string url, string contentResponseBody)
        {
            using (var httpClient = RBACHttpClient.GetRBACHttpClient(username, password, domain))
            {
                var response = await httpClient.PostAsync(ROOT_URL + url,
                    new StringContent(contentResponseBody, Encoding.UTF8, "application/json"));
                return response.StatusCode.ToString();
            }
        }
    }
}
