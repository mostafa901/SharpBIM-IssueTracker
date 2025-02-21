namespace SharpBIM.GitTracker.Auth
{
    public class UserToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public DateTime ExpireTime { get; set; }
        public string refresh_token { get; set; }
        public int refresh_token_expires_in { get; set; }
        public DateTime RefreshExpireTime { get; set; }
        public string token_type { get; set; } // always bearer
        public string scope { get; set; } // will always be empty
    }
}