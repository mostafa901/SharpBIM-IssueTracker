using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using SharpBIM.GitTracker.Auth;
using SharpBIM;
using SharpBIM.Utility.Extensions;
using static System.Net.WebRequestMethods;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts;
using Org.BouncyCastle.Asn1.Crmf;
using System.Web;

namespace SharpBIM.GitTracker.GitHttp
{
    public static class MediaTypes
    {
        public const string MACHINEMANPREVIEWJSON = "application/vnd.github.machine-man-preview+json";
        public const string APPLICATIONJSON = "application/json";
        public const string VNDGITHUBJSON = "application/vnd.github+json";
        public const string RAWJSON = "application/vnd.github.raw+json";
        public const string TXTJSON = "application/vnd.github.text+json";
        public const string FULLJSON = "application/vnd.github.full+json";
    }

    public abstract class GitClient
    {
        protected static HttpClient httpClient;
        protected virtual string endPoint => "";

        protected virtual string GetEndPoint(RepoModel repo) => endPoint.Replace("REPO", repo.name);

        protected IGitConfig Config => AppGlobals.Config;
        public User User => AppGlobals.user;
        protected InstallationModel Installation => User.Installation;
        protected Account Account => Installation?.account;

        protected const string NUMBER = "{/number}";

        protected GitClient()
        {
            httpClient ??= new HttpClient();
        }

        protected virtual void AddHeaders(HttpRequestMessage request)
        {
            //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.APPLICATIONJSON));
        }

        protected virtual IEnumerable<T> ParseResponse<T>(string response)
        {
            if (response == null)
                return null;
            List<T> result = new List<T>();
            var jop = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            if (response.StartsWith("["))
                result.AddRange(JsonSerializer.Deserialize<IEnumerable<T>>(response, jop));
            else
                result.Add(JsonSerializer.Deserialize<T>(response, jop));

            return result;
        }

        private async Task<IServiceReport<string>> SEND(HttpMethod method, string url, object requestBody)
        {
            if (!await AreWeAuthorized())
                return new ServiceReport<string>().Failed("Not Authorized");
            var report = new ServiceReport<string>();
            try
            {
                var request = new HttpRequestMessage(method, url)
                {
                    Content = GetStringContent(requestBody),
                };
                AddDefaultHeaders(request);

                var response = await httpClient.SendAsync(request);
                report = await EvaluateResponse(response);
            }
            catch (Exception ex)
            {
                report.Failed(ex.Message);
            }
            return report;
        }

        protected virtual async Task<IServiceReport<string>> GET(string url, object requestBody = null)
        {
            var report = await SEND(HttpMethod.Get, url, requestBody);

            return report;
        }

        protected virtual AuthenticationHeaderValue RequestAuth { get; set; }

        protected virtual AuthenticationHeaderValue GetAuthentication()
        {
            var auth = RequestAuth ?? new AuthenticationHeaderValue("Bearer", User.Token.access_token);
            return auth;
        }

        private void AddDefaultHeaders(HttpRequestMessage request)
        {
            request.Headers.Authorization = GetAuthentication();
            request.Headers.UserAgent.ParseAdd(Config.AppName);
            AddHeaders(request);
        }

        protected virtual JsonSerializerOptions GetPostOptions()
        {
            return new JsonSerializerOptions();
        }

        protected virtual StringContent GetStringContent(object requestBody)
        {
            if (requestBody != null)
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody, GetPostOptions()), Encoding.UTF8, MediaTypes.VNDGITHUBJSON);
                return content;
            }
            return null;
        }

        protected virtual async Task<IServiceReport<string>> POST(string url, object requestBody)
        {
            var report = await SEND(HttpMethod.Post, url, requestBody);

            return report;
        }

        protected virtual async Task<IServiceReport<string>> DELETE(string url, object requestBody)
        {
            var report = await SEND(HttpMethod.Delete, url, requestBody);

            return report;
        }

        protected virtual async Task<bool> AreWeAuthorized()
        {
            var report = await AuthService.Login();
            return !report.IsFailed;
        }

        protected virtual async Task<IServiceReport<string>> PUT(string url, object requestBody)
        {
            var report = await SEND(HttpMethod.Put, url, requestBody);

            return report;
        }

        protected virtual async Task<IServiceReport<string>> PATCH(string url, object requestBody)
        {
            var report = await SEND(new HttpMethod("PATCH"), url, requestBody);
#if false
            try
            {
                {
                    Content = new StringContent(JsonSerializer.Serialize(requestBody, GetPostOptions()), Encoding.UTF8, MediaTypes.FULLJSON)
                }
                ;
                LoadHeaders(request);
                var response = await httpClient.SendAsync(request);
                report = await EvaluateResponse(response);
            }
            catch (Exception ex)
            {
                report.Failed(ex.Message);
            }
#endif

            return report;
        }

        protected virtual async Task<ServiceReport<string>> EvaluateResponse(HttpResponseMessage response)
        {
            var report = new ServiceReport<string>();

            var responseJson = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false || responseJson.Contains("error_description"))
            {
                report.Failed(ResponseMessages.GetMessageToUser(responseJson));
            }
            else
                report.Model = responseJson;

            return report;
        }

        public string EmbedParams(string url, object body)
        {
            var builder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(builder.Query);
            var js = body.JSerialize();
            string x;
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(js);

            foreach (var kvp in dictionary)
            {
                if (kvp.Value != null)
                {
                    query[kvp.Key] = kvp.Value.ToString();
                }
            }

            builder.Query = query.ToString();
            var requestUri = builder.ToString();
            return requestUri;
        }
    }
}

public static class ResponseMessages
{
    public static string GetMessageToUser(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            string msg = responseJson;
            if (doc.RootElement.TryGetProperty("message", out JsonElement jse))
            {
                msg = jse.GetString();
            }
            else if (doc.RootElement.TryGetProperty("error_description", out jse))
            {
                msg = jse.GetString();
            }
            return $"{msg}";
        }
        catch (Exception)
        {
            // the reponse is not a json response
            return responseJson;
        }
    }

    public static class Token
    {
    }
}