using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Web.WebView2.Core;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SharpBim.GitTracker.Auth;
using SharpBim.GitTracker.Auth.BrowseOptions;
using SharpBIM;
using static System.Net.WebRequestMethods;

namespace SharpBim.GitTracker.GitHttp
{
    public abstract class GitClient
    {
        protected readonly HttpClient httpClient;
        protected virtual string endPoint { get; set; }
        protected IGitConfig Config => AppGlobals.Config;
        protected User User => AppGlobals.user;

        public GitClient()
        {
            httpClient = new HttpClient();
        }

        public virtual async Task<string> GET(HttpRequestMessage request)
        {
            try
            {
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

        public virtual async Task<string> POST(string url, object requestBody)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
                };

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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

    public class GitToken : GitClient
    {
        private UserToken token;

        public GitToken()
        {
            this.token = token;
        }

        public async Task<string> AuthorizeApp()
        {
            // check if the app already authorized
            var brw = new SystemBrowser();
            var gitOps = new GitLoginOptions();

            var res = await brw.InvokeAsync(gitOps);
            return gitOps.Code;
        }

        protected override string endPoint => $"https://github.com/login/oauth/access_token";

        public async Task<bool> RequestUserToken(string accessCode)
        {
            var requestBody = new
            {
                client_id = Config.ClientId,
                client_secret = Config.ClientSecret,
                code = accessCode
            };

            return await RequestToken(endPoint, requestBody);
        }

        public async Task<bool> RefreshToken()
        {
            var requestBody = new
            {
                client_id = Config.ClientId,
                client_secret = Config.ClientSecret,
                grant_type = QueryString.REFRESHTOKEN,
                refresh_token = token.refresh_token
            };

            return await RequestToken(endPoint, requestBody);
        }

        private async Task<bool> RequestToken(string url, object requestBody)
        {
            var responseJson = await POST(url, requestBody);
            if (responseJson == null)
                return false;
            using var doc = JsonDocument.Parse(responseJson);
            token = JsonSerializer.Deserialize<UserToken>(responseJson);
            return true;
        }
    }

    public class GitInstallation : GitClient
    {
        protected override string endPoint => "https://api.github.com/app/installations";

        private string GenerateJwtToken()
        {
            var securityKey = new RsaSecurityKey(LoadRsaPrivateKey());
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            var header = new JwtHeader(credentials);
            var payload = new JwtPayload
        {
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }, // Issued at time
            { "exp", DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds() }, // Expiration time
            { "iss", Config.ClientId } // GitHub App's client ID
        };

            var token = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RSA LoadRsaPrivateKey()
        {
            using var reader = new StringReader(Config.PrivateKey);
            var pemReader = new PemReader(reader);
            var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;

            if (keyPair == null)
                throw new Exception("Invalid RSA private key format.");

            var privateKey = keyPair.Private as Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters;
            return privateKey != null ? DotNetUtilities.ToRSA(privateKey) : throw new Exception("Failed to load RSA key.");
        }

        public GitInstallation()
        {
        }

        public async Task<string> GetInstallationIdAsync()
        {
            string jwtToken = GenerateJwtToken();
            int trial = 5;
            string response = null;
            while (trial > 0)
            {
                trial--;
                var request = new HttpRequestMessage(HttpMethod.Get, endPoint);
                request.Headers.Authorization = new AuthenticationHeaderValue(QueryString.BEARER, jwtToken);
                request.Headers.UserAgent.ParseAdd(Config.AppName);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.machine-man-preview+json"));

                response = await GET(request);
            }
            if (response == null)
                return null;
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            string installationId = null;
            if (root.GetArrayLength() > 0)
            {
                installationId = root[0].GetProperty("id").GetInt32().ToString();
            }

            return installationId;
        }

        public async Task<bool> RequestInstalling()
        {
            // check if the app already authorized
            var brw = new SystemBrowser();
            var gitOps = new GitInstallOptions();
            var res = await brw.InvokeAsync(gitOps);
            User.InstallationId = gitOps.InstallationId;
            return !string.IsNullOrEmpty(User.InstallationId);
        }
    }
}