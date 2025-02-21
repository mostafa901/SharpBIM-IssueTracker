namespace SharpBIM.GitTracker.Auth
{
    public interface IGitConfig
    {
        string AppId { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string PrivateKey { get; set; }
        string AppName { get; }
        string UriAppName { get; }
    }
}