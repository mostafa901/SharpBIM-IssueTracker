using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Abstracts;
using SharpBIM.ServiceContracts.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBIM.GitTracker.Core
{
    public class SharpGitService : Service, ISharpGitService
    {
        public SharpGitService(IConfig config) : base(config)
        {
        }

        public async Task<IServiceReport<string>> PublishFeedback(string title, string body, string repoName)
        {
            var report = new ServiceReport<string>();
            try
            {
                GitAuth.Config = (await new SharpBIMHTTP(AppGlobals).GetGitConfigAsync("SharpBim@gmail.com")).Model;


                AppGlobals.SharpUser.Token = new SharpToken { access_token = GitAuth.Config.PToken };
                AppGlobals.SharpUser.Name = "mostafa901";

                var gitService = new GitIssues(AppGlobals);
                gitService.UpdateOwnerAccount(AppGlobals.SharpUser.Name);
                var response = await gitService.CreateIssue(repoName,
                    new SharpBIM.GitTracker.Core.GitHttp.Models.IssueModel
                    {
                        Title = title,
                        body = body,
                    });

                if (response.IsFailed)
                {
                    report.Failed(response.ErrorMessage);
                }
                else
                {
                    report.AddSuccess("Feedback issued.");
                }
            }
            catch (Exception ex)
            {
                report.Failed(ex);
            }
            return report;
        }

        public async Task<IServiceReport<string>> PublishFeedback2(string title, string body, string personalToken, string secret)
        {
            var report = new ServiceReport<string>();
            var authService = new GitAuth(AppGlobals);
            GitAuth.Config = (await (new SharpBIMHTTP(AppGlobals)).GetGitConfigAsync("SharpBim@gmail.com")).Model;
            new GitRepos(AppGlobals).UpdateOwnerAccount("mostafa901");
            var result = await authService.LoginByPersonalToken(personalToken, secret);

            if (!result.IsFailed)
            {
                var git = await new GitIssues(AppGlobals).CreateIssue("SharpBIM.Invoice",
                    new GitTracker.Core.GitHttp.Models.IssueModel
                    {
                        body = body,
                        Title = title,
                    });
            }
            if (result.IsFailed)
            {
                report.Merge(result.ErrorMessage);
                return report;
            }
            return report;
        }
    }
}