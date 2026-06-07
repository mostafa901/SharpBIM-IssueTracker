using SharpBIM.IssueTracker.Core.Auth;
using SharpBIM.IssueTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.Services;

namespace SharpBIM.IssueTracker.Core.Auth
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