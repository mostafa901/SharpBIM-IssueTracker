using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.GitTracker.Core.Auth
{
    public interface IUser : ISharpUser
    {
        public void Save();

        InstallationModel Installation { get; set; }
        bool IsPersonalToken { get; set; }
        UserToken Token { get; set; }
        Account UserAccount { get; set; }
    }
}