namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitAssignees : GitClient
    {
        protected override string GetEndPoint(params object[] values)
        {
            var vs = values.ToList();
            vs.Insert(1, "assignees");
            var url = base.GetEndPoint(vs.ToArray());
            return url;
        }
        public GitAssignees(IConfig appGlobals) : base(appGlobals)
        {
        }

 
        public async Task<IServiceReport<IEnumerable<Account>>> GetAssigneesAsync(RepoModel repoModel)
        {
            var accountReport = new ServiceReport<IEnumerable<Account>>();
            // "https://api.github.com/repos/{owner}/{repo}/assignees"
            var url = GetEndPoint(repoModel);
            
            var res = await GET(url);
            if (res.IsFailed)
            {
                accountReport.Merge(res);
                return accountReport;
            }
            var accounts = ParseResponse<Account>(res.Model);

            accountReport.Model = accounts;
            return accountReport;
        }
    }
}