using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE80;

using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Abstracts;
using SharpBIM.ServiceContracts.Interfaces;
#if WINDOWS
using SharpBIM.WPF.Services;
#endif

namespace SharpBIM.GitTracker.Core
{
    public class SharpGitService : Service, ISharpGitService
    {
        GitTrackerGlobals gitGlobals = new GitTrackerGlobals();

        public string PersonalAccessCode { get; }

        public SharpGitService(string personalAccessCode, IConfig config) : base(config)
        {
            PersonalAccessCode = personalAccessCode;
        }

#if WINDOWS
        public async Task<IServiceReport<string>> PublishFeedbackUI(string repoName)
        {
            IServiceReport<string> report = new ServiceReport<string>();

            if (AppGlobals.MsgService == null)
            {
                report.Failed("MsgService not initialized");
                return report;
            }
            var feedbackReport = AppGlobals.MsgService.FeedBack(AppGlobals.MainWindowHandle, "Feedback", "Your comment");
            if (feedbackReport.IsFailed)
            {
                report.Failed(feedbackReport.ErrorMessage);
                return report;
            }

            report = await PublishFeedback(feedbackReport.Model.Item1,
                                                          feedbackReport.Model.Item2,
                                                          repoName);

            AppGlobals.MsgService.AlertUser(AppGlobals.MainWindowHandle, "Feedback", report.ErrorMessage);
            return report;
        }
#endif

        public async Task<IServiceReport<IEnumerable<RepoModel>>> GetRepoModelsAsync(string repoFullName)
        {
            var report = new ServiceReport<IEnumerable<RepoModel>>();
            var loginReport = await AuthService.LoginByPersonalToken(PersonalAccessCode);
            if (loginReport.IsFailed)
            {
                report.Failed(loginReport.ErrorMessage);
                return report;
            }
            var repoModelsReport = await ReposSerivce.GetRepos(repoFullName);
            if (repoModelsReport.IsFailed)
            {
                report.Failed(repoModelsReport.ErrorMessage);
                return report;
            }
            var models = repoModelsReport.Model.ToList();
            report.Model = models;

            return report;
        }
        public async Task<IServiceReport<string>> PublishFeedback(string title, string body, string repoName)
        {
            var report = new ServiceReport<string>();
            try
            {
                var gitRepoService = new GitRepos(AppGlobals);
                var gitService = new GitIssues(AppGlobals);
                GitClient.Config = (await new GitAuth(AppGlobals).GetGitConfigAsync()).Model;

                gitService.User.Token = new SharpToken { access_token = GitAuth.Config.PToken };
                gitService.User.Name = "mostafa901";
                gitService.UpdateOwnerAccount(gitService.User.Name);

                var repoModel = (await gitRepoService.GetRepos()).Model.FirstOrDefault(x => x.name.EQ(repoName));
                var response = await gitService.CreateIssue(repoModel,
                    new IssueModel
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

        async public Task<IServiceReport<IEnumerable<IssueModel>>> GetIssueModelsAsync(RepoModel repoModel)
        {
            var report = new ServiceReport<IEnumerable<IssueModel>>();
            var loginReport = await AuthService.LoginByPersonalToken(PersonalAccessCode);
            if (loginReport.IsFailed)
            {
                report.Failed(loginReport.ErrorMessage);
                return report;
            }
            var repoModelsReport = await IssuesService.GetIssues(repoModel, 0, IssueState.all);
            if (repoModelsReport.IsFailed)
            {
                report.Failed(repoModelsReport.ErrorMessage);
                return report;
            }
            var models = repoModelsReport.Model.ToList();
            report.Model = models;

            return report;
        }

        async public Task<IServiceReport<IEnumerable<IssueModel>>> GetSubIssueModelsAsync(RepoModel repoModel, int issueNumber)
        {
            var report = new ServiceReport<IEnumerable<IssueModel>>();
            var loginReport = await AuthService.LoginByPersonalToken(PersonalAccessCode);
            if (loginReport.IsFailed)
            {
                report.Failed(loginReport.ErrorMessage);
                return report;
            }
            var repoModelsReport = await IssuesService.GetSubIssues(repoModel, issueNumber);
            if (repoModelsReport.IsFailed)
            {
                report.Failed(repoModelsReport.ErrorMessage);
                return report;
            }
            var models = repoModelsReport.Model.ToList();
            report.Model = models;

            return report;
        }


    }
}