using System.Text.Json;

namespace SharpBim.GitTracker.Auth
{
    public class User
    {
        public UserToken Token { get; set; }
        public string InstallationId { get; set; }

        public static User? Parse(string jsonstring)
        {
            try
            {
                return JsonSerializer.Deserialize<User>(jsonstring);
            }
            catch (Exception)
            {
                return new User();
            }
        }
    }
}