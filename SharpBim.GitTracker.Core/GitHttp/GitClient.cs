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
using SharpBim.GitTracker.Auth;
using SharpBIM;
using SharpBIM.Utility.Extensions;
using static System.Net.WebRequestMethods;

namespace SharpBim.GitTracker.GitHttp
{
    public static class MediaTypes
    {
        public const string MACHINEMANPREVIEWJSON = "application/vnd.github.machine-man-preview+json";
        public const string APPLICATIONJSON = "application/json";
        public const string VNDGITHUBJSON = "application/vnd.github+json";
        public const string RAWJSON = "application/vnd.github.raw+json";
        public const string TXTJSON = "application/vnd.github.text+json";
    }

    public abstract class GitClient
    {
        protected readonly HttpClient httpClient;
        protected virtual string endPoint => "";
        protected IGitConfig Config => AppGlobals.Config;
        protected User User => AppGlobals.user;
        protected InstallationModel Installation => User.Installation;
        protected Account Account => Installation.account;

        protected const string NUMBER = "{/number}";

        protected GitClient()
        {
            httpClient = new HttpClient();
        }

        protected virtual void AddHeaders(HttpRequestMessage request)
        {
        }

        protected virtual IEnumerable<T> ParseResponse<T>(string response)
        {
            return Enumerable.Empty<T>();
        }

        protected virtual async Task<string> GET(HttpRequestMessage request)
        {
            try
            {
                if (request.Headers.Authorization == null)
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", User.Token.access_token);
                request.Headers.UserAgent.ParseAdd(Config.AppName);
                AddHeaders(request);
                var response = await httpClient.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                return responseJson;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected virtual async Task<string> POST(string url, object requestBody)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, MediaTypes.APPLICATIONJSON)
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.APPLICATIONJSON));

                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();

                return responseJson;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}