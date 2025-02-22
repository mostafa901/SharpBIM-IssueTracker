﻿using System.IdentityModel.Tokens.Jwt;
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

        private const string ResponseMissingPermission = "You do not have permission to perform this action. You need to reinstall the application.";
        private const string MissingGitPermission = "Resource not accessible by integration";

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);
            string jwtToken = GenerateJwtToken();
            request.Headers.Authorization = new AuthenticationHeaderValue(QueryString.BEARER, jwtToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.MACHINEMANPREVIEWJSON));
        }

        public async Task<InstallationModel> GetInstallationIdAsync()
        {
            int trial = 5;
            InstallationModel installModel = null;
            while (trial > 0)
            {
                trial--;

                var report = await GET(endPoint);

                if (!report.IsFailed)
                {
                    var installModels = ParseResponse<InstallationModel>(report.Model);
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
            var response = await GET(LimitURL);
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