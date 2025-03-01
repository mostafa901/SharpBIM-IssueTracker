using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpBIM.UIContexts;
using SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels;
using SharpBIM.GitTracker.Core.GitHttp.Models;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels
{
    public class SubIssueListViewModel : IssueListViewModel
    {
        public override RepoModel SelectedRepo
        {
            get { return GetParentViewModel<IssueListViewModel>().SelectedRepo; }
            set { SetValue(value, nameof(SelectedRepo)); }
        }

        public SubIssueListViewModel()
        {
            Title = "List of all SUB - issues";
        }

        public override async Task LoadIssuesAsync(object x)
        {
            var parentISsue = ParentModelView as IssueViewModel;
            List<IssueViewModel> subIssuesMv = [];
            if (parentISsue != null)
            {
                if (parentISsue.Children.Any())
                {
                    // issues are already loaded before, how to refresh?
                    subIssuesMv.AddRange(parentISsue.Children);
                }
                else
                if (parentISsue.ContextData.sub_issues_summary.total > 0)
                {
                    // this will get the only 100 sub issues
                    var subIssues = await IssuesService.GetSubIssues(SelectedRepo.name, parentISsue.ContextData.number, 1);
                    if (subIssues.IsFailed)
                    {
                    }
                    else
                    {
                        var subids = subIssues.Model.Select(o => (long)o.number);

                        subIssuesMv = subIssues.Model.Select(o => o.ToModelView<IssueViewModel>(parentISsue)).ToList();
                    }
                }
            }
            if (subIssuesMv.Any())
                await AddItemsAsync(subIssuesMv, Token);
        }
    }
}