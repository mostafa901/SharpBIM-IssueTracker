using System.Text.Json;

namespace SharpBim.GitTracker.Auth
{
    public class GitConfig : IGitConfig
    {
        public string AppId { get; set; }
        public string ClientSecret { get; set; }
        public string PrivateKey { get; set; }
        public string ClientId { get; set; }

        public string AppName => "SharpBIM.IssueTracker";
        public string UriAppName => "SharpBIM-IssueTracker";

        public GitConfig()
        {
            PrivateKey = System.IO.File.ReadAllText("D:\\RevitApi\\Shared\\Study\\SharpBim.Git\\SharpBim.GitTracker.Core\\Study\\private.pem");
        }

        public static GitConfig Parse()
        {
            return JsonSerializer.Deserialize<GitConfig>(System.IO.File.ReadAllText("D:\\RevitApi\\Shared\\Study\\SharpBim.Git\\SharpBim.GitTracker.Core\\Study\\gitconfig.json"));
        }
    }
}