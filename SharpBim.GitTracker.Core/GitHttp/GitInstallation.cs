using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SharpBIM.GitTracker.Auth;
using SharpBIM.GitTracker.Auth.BrowseOptions;

namespace SharpBIM.GitTracker.GitHttp
{
    internal class GitInstallation : GitClient
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

        public async Task<InstallationModel> GetInstallationIdAsync()
        {
            string jwtToken = GenerateJwtToken();
            int trial = 5;
            InstallationModel installModel = null;
            while (trial > 0)
            {
                trial--;
                var request = new HttpRequestMessage(HttpMethod.Get, endPoint);
                request.Headers.Authorization = new AuthenticationHeaderValue(QueryString.BEARER, jwtToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.MACHINEMANPREVIEWJSON));

                string response = await GET(request);
                if (response != null)
                {
                    var installModels = JsonSerializer.Deserialize<IEnumerable<InstallationModel>>(response);
                    installModel = installModels?.FirstOrDefault();
                    break;
                }
            }
            if (installModel == null)
                return null;

            return installModel;
        }

        private string LimitURL = "https://api.github.com/rate_limit";

        public async Task GetLimits()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, LimitURL);
            request.Headers.UserAgent.ParseAdd(Config.UriAppName);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJwtToken());

            var response = await GET(request);
        }

        public async Task<bool> RequestInstalling()
        {
            // check if the app already authorized
            var brw = new SystemBrowser();
            var gitOps = new GitInstallOptions();
            var res = await brw.InvokeAsync(gitOps);
            return !string.IsNullOrEmpty(User.InstallationId);
        }
    }
}