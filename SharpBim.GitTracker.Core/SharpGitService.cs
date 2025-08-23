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
                var gitService = new GitIssues(AppGlobals);
                GitClient.Config =   (await new GitAuth(AppGlobals).GetGitConfigAsync()).Model;

                gitService.User.Token = new SharpToken { access_token = GitAuth.Config.PToken };
                gitService.User.Name = "mostafa901";
                gitService.UpdateOwnerAccount(gitService.User.Name);

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


    }
}