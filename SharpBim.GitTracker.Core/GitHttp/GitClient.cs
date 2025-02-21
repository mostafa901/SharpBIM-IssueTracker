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
        protected IGitConfig Config => AppGlobals.Config;
        protected User User => AppGlobals.user;
        protected InstallationModel Installation => User.Installation;
        protected Account Account => Installation?.account;

        protected const string NUMBER = "{/number}";

        protected GitClient()
        {
            httpClient ??= new HttpClient();
        }

        protected virtual void AddHeaders(HttpRequestMessage request)
        {
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

            if (result.Any() == false)
                return null;
            return result;
        }

        protected virtual async Task<IServiceReport<string>> GET(HttpRequestMessage request)
        {
            var report = new ServiceReport<string>();
            try
            {
                LoadHeaders(request);
                //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.APPLICATIONJSON));
                var response = await httpClient.SendAsync(request);

                report = await EvaluateResponse(response);
            }
            catch (Exception ex)
            {
                report.Failed(ex.Message);
            }
            return report;
        }

        private void LoadHeaders(HttpRequestMessage request)
        {
            if (request.Headers.Authorization == null)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", User.Token.access_token);
            request.Headers.UserAgent.ParseAdd(Config.AppName);
            AddHeaders(request);
        }

        protected virtual JsonSerializerOptions GetPostOptions()
        {
            return new JsonSerializerOptions();
        }

        protected virtual async Task<IServiceReport<string>> POST(string url, object requestBody)
        {
            var report = new ServiceReport<string>();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(JsonSerializer.Serialize(requestBody, GetPostOptions()), Encoding.UTF8, MediaTypes.VNDGITHUBJSON)
                };
                LoadHeaders(request);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.APPLICATIONJSON));

                var response = await httpClient.SendAsync(request);
                report = await EvaluateResponse(response);
            }
            catch (Exception ex)
            {
                report.Failed(ex.Message);
            }
            return report;
        }

        protected async Task<bool> AreWeAuthorized()
        {
            var report = await AuthService.Login();
            return !report.IsFailed;
        }

        protected virtual async Task<IServiceReport<string>> PUT(string url, object requestBody)
        {
            if (!await AreWeAuthorized())
                return new ServiceReport<string>().Failed("Not Authorized");
            var report = new ServiceReport<string>();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = new StringContent(JsonSerializer.Serialize(requestBody, GetPostOptions()), Encoding.UTF8, MediaTypes.VNDGITHUBJSON)
                };
                LoadHeaders(request);
                var response = await httpClient.SendAsync(request);
                report = await EvaluateResponse(response);
            }
            catch (Exception ex)
            {
                report.Failed(ex.Message);
            }

            return report;
        }

        protected virtual async Task<IServiceReport<string>> PATCH(string url, object requestBody)
        {
            if (!await AreWeAuthorized())
                return new ServiceReport<string>().Failed("Not Authorized");
            var report = new ServiceReport<string>();
            try
            {
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = new StringContent(JsonSerializer.Serialize(requestBody, GetPostOptions()), Encoding.UTF8, MediaTypes.FULLJSON)
                };
                LoadHeaders(request);
                var response = await httpClient.SendAsync(request);
                report = await EvaluateResponse(response);
            }
            catch (Exception ex)
            {
                report.Failed(ex.Message);
            }

            return report;
        }

        private static async Task<ServiceReport<string>> EvaluateResponse(HttpResponseMessage response)
        {
            var report = new ServiceReport<string>();

            var responseJson = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false)
            {
                report.Failed(ResponseMessages.GetMessageToUser(responseJson));
            }
            else
                report.Model = responseJson;

            return report;
        }
    }
}

public static class ResponseMessages
{
    private const string ResponseDefault = "Unknown Error.";
    private const string ResponseMissingPermission = "You do not have permission to perform this action. You need to reinstall the application.";
    private const string MissingGitPermission = "Resource not accessible by integration";

    public static string GetMessageToUser(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);
        var msg = doc.RootElement.GetProperty("message").GetString();

        if (MissingGitPermission.Contains(msg))
            return ResponseMissingPermission;
        else
            return $"{ResponseDefault}{Environment.NewLine}{msg}";
    }
}