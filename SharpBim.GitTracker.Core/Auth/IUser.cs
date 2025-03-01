using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.Services;

namespace SharpBIM.GitTracker.Core.Auth
{
    public interface IUser : ISharpUser<SharpToken>
    {
        public void Save();

        InstallationModel Installation { get; set; }
        bool IsPersonalToken { get; set; }
        SharpToken Token { get; set; }
        Account UserAccount { get; set; }
    }
}