using System.Text.Json;

namespace SharpBIM.GitTracker.Auth
{
    public class User
    {
        public bool IsPersonalToken { get; set; }
        public UserToken Token { get; set; }
        public InstallationModel Installation { get; set; }
        public string InstallationId => Installation?.id.ToString() ?? string.Empty;

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