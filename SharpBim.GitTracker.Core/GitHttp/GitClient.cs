using SharpBIM.Utility.Helpers.Events;

using System.Web;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public abstract class GitClient : SharpBIMHTTP
    {
        public static IGitConfig Config { get; set; }


        protected override string GetEndPoint(params object[] values)
        {
            RepoModel repoModel = values[0] as RepoModel;
            var listValues = values.ToList();
            listValues.Insert(0, repoModel.url);
            var url = base.GetEndPoint(listValues.ToArray());
             
            return url;
        }

#if WINDOWS
        internal GitUser User => GitTrackerGlobals.AppGlobals.User;
#else
        internal ISharpUser<SharpToken> User => AppGlobals.SharpUser; 
#endif
        protected static string Owner { get; private set; }

        protected GitClient(IConfig appGlobals) : base(appGlobals)
        {
        }

        public void UpdateOwnerAccount(string newOwner)
        {
            Owner = newOwner;
        }
     
        protected override async Task<IServiceReport<string>> SEND(HttpMethod method, string url, object requestBody)
        {
            if (!await AreWeAuthorized())
            {
                var authReport = new ServiceReport<string>();
                authReport.Failed("Not Authorized");
                return authReport;
            }
            //if (RemainingCalls == 0)
            //{
            //    return new ServiceReport<string>().Failed($"Tokens credits depleted. Credits will be refilled with in {TimeToReset}");
            //}
            IServiceReport<string> report = new ServiceReport<string>();
            try
            {
                var request = new HttpRequestMessage(method, url)
                {
                    Content = GetStringContent(requestBody),
                };
                await AddDefaultHeaders(request);

                var response = await httpClient.SendAsync(request);
                report = await EvaluateResponse(response);

                var callev = new CallEventArgs(method, url, requestBody?.JSerialize() ?? "Null body", response, report.Model);
                ExecuteOnRequestedEvent(callev);

                if (response.Headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string> RemainingCallstring))
                {
                    RemainingCalls = int.Parse(RemainingCallstring.First());
                    if (RemainingCalls == 0)
                    {
                        TimeToReset = TimeSpan.FromSeconds(int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First()));
                    }
                }
            }
            catch (Exception ex)
            {
                report.Failed(ex.Message);
            }
            finally { NeedAuthentication = true; }
            return report;
        }

        async protected override Task<AuthenticationHeaderValue> GetAuthentication()
        {
            if (!string.IsNullOrEmpty(User.Token.access_token))
            {
                var auth = new AuthenticationHeaderValue(SharpBIM.Statics.BEARER, User.Token.access_token);
                return auth;
            }
            return null;
        }

        private async Task AddDefaultHeaders(HttpRequestMessage request)
        {
            request.Headers.Authorization = await GetAuthentication();
            request.Headers.UserAgent.ParseAdd(Config?.AppName ?? AppGlobals.CompanyName);
            AddHeaders(request);
        }

        protected override StringContent GetStringContent(object requestBody)
        {
            if (requestBody != null)
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody, GetJsonOptions(null)), Encoding.UTF8, MediaTypes.VNDGITHUBJSON);
                return content;
            }
            return null;
        }

        protected override async Task<IServiceReport<string>> POST(string url, object requestBody)
        {
            var report = await SEND(HttpMethod.Post, url, requestBody);

            return report;
        }

        protected override async Task<IServiceReport<string>> DELETE(string url, object requestBody)
        {
            var report = await SEND(HttpMethod.Delete, url, requestBody);

            return report;
        }

        protected override async Task<bool> AreWeAuthorized()
        {
            if (NeedAuthentication)
            {
                if (string.IsNullOrEmpty(AppGlobals.SharpUser.Token.access_token))
                {
                    return false;
                }
            }
            return true;
        }

        protected override async Task<IServiceReport<string>> PUT(string url, object requestBody)
        {
            var report = await SEND(HttpMethod.Put, url, requestBody);

            return report;
        }

        protected override async Task<IServiceReport<string>> PATCH(string url, object requestBody)
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

        protected override async Task<IServiceReport<string>> EvaluateResponse(HttpResponseMessage response)
        {
            var report = new ServiceReport<string>();

            var responseJson = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode == false || responseJson.Contains("error_description"))
            {
                report.Model = response.StatusCode.ToString();
                report.Failed($"{ResponseMessages.GetMessageToUser(responseJson)}");
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