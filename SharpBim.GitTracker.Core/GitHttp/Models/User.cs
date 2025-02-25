using System.Text.Json;
using SharpBIM.GitTracker.Core.Auth;

namespace SharpBIM.GitTracker.Core.GitHttp.Models
{
    internal class User : IUser
    {
        public bool IsPersonalToken { get; set; }
        public UserToken Token { get; set; }
        public InstallationModel Installation { get; set; }
        public Account UserAccount { get; set; }

        public User()
        {
            Token = new UserToken();
        }

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

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}