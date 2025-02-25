using SharpBIM.GitTracker.Core.Auth;

namespace SharpBIM.GitTracker.Core.Auth
{
    public interface IUser
    {
        public void Save();

        InstallationModel Installation { get; set; }
        bool IsPersonalToken { get; set; }
        UserToken Token { get; set; }
        Account UserAccount { get; set; }
    }
}