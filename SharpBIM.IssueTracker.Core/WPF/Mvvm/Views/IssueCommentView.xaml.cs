using SharpBIM.WPF.Controls.UserControls;

namespace SharpBIM.IssueTracker.Core.WPF.Mvvm.Views
{
    /// <summary>
    /// Interaction logic for IssueCommentView.xaml
    /// </summary>
    public partial class IssueCommentView : SharpBIMUserControl
    {
        public IssueCommentView()
        {
            InitializeComponent();
        }
    }

    public class IssueCommentModelView : ModelViewBase<CommentModel>
    {
        public IssueCommentModelView()
        {
            ShowOnWebCommand = new SharpBIMCommand(ShowOnWeb, "Show on web", Glyphs.hyperlink_globe, static (x) => true);
        }

        public string MarkDown
        {
            get { return GetValue<string>(nameof(MarkDown)); }
            set { SetValue(value, nameof(MarkDown)); }
        }

        public string CreatedDate
        {
            get { return GetValue<string>(nameof(CreatedDate)); }
            set { SetValue(value, nameof(CreatedDate)); }
        }

        public string ModifiedDate
        {
            get { return GetValue<string>(nameof(ModifiedDate)); }
            set { SetValue(value, nameof(ModifiedDate)); }
        }

        async public override Task Init(CommentModel dataModel)
        {
            base.Init(dataModel);
            Title = dataModel.user.login;
            MarkDown = dataModel.body;
            ModifiedDate = dataModel.updated_at.ToShortDateString();
            CreatedDate = dataModel.created_at.ToShortDateString();
        }

        public SharpBIMCommand ShowOnWebCommand { get; set; }

        // Add this line to the constructor

        public void ShowOnWeb(object x)
        {
            try
            {
                IOEx.OpenUrl(ContextData.html_url);
            }
            catch (Exception ex)
            {
            }
        }

        public override IServiceReport<bool> Save()
        {
            throw new NotImplementedException();
        }

        public override IServiceReport<bool> Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class IssueCommentViewModel : ModelViewBase<IssueModel, IssueCommentModelView>
    {
        public IssueCommentViewModel()
        {
            PushCommentCommand = new SharpBIMCommand(async (x) => await PushComment(x), "Push Comment", Glyphs.empty, (x) => true);

            ReloadIssueCommand = new SharpBIMCommand(async (x) => await ReloadIssue(x), "Reload Comments", Glyphs.reload, (x) => true);
        }

        public SharpBIMCommand ReloadIssueCommand { get; set; }

        // Add this line to the constructor

        public async Task ReloadIssue(object x)
        {
            try
            {
                await Loadcomments();
            }
            catch (Exception ex)
            {
            }
        }

        public SharpBIMCommand PushCommentCommand { get; set; }

        public async Task PushComment(object x)
        {
            try
            {
                ProgressActivity.Reset();
                ProgressActivity.IsVisible = true;
                ProgressActivity.Title = "Pushing comment";
                if (string.IsNullOrEmpty(NewComment))
                {
                    AppGlobals.MsgService.AlertUser(WindowHandle, "Push comment", "Nothing to push");
                    return;
                }

                var issueModel = GetParentViewModel<IssueViewModel>();
                var pushReport = await CommentService.PushComment(issueModel.SelectedRepo, ContextData.number, new CommentModel { body = NewComment });
                if (pushReport.IsFailed)
                {
                    AppGlobals.MsgService.AlertUser(WindowHandle, "Failed to push comment", pushReport.ErrorMessage);
                }
                else
                {
                    FillModels([pushReport.Model]);
                    NewComment = string.Empty;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ProgressActivity.Reset();
            }
        }

        public string NewComment
        {
            get { return GetValue<string>(nameof(NewComment)); }
            set { SetValue(value, nameof(NewComment)); }
        }

        public override async Task Init(IssueModel dataModel)
        {
            base.Init(dataModel);
            Title = $"Comments on #{dataModel.number}";
            await Loadcomments();
        }

        private async Task Loadcomments()
        {
            Children.Clear();
            var parernt = GetParentViewModel<IssueViewModel>();
            var commentReport = await CommentService.GetCommentsForIssue(parernt.SelectedRepo, ContextData.number);
            if (commentReport.IsFailed)
            {
                AppGlobals.MsgService.AlertUser(WindowHandle, "Couldn't Retrieve Comments", commentReport.ErrorMessage);
            }
            else
            {
                var models = commentReport.Model;
                FillModels(models);
            }
        }

        private void FillModels(IEnumerable<CommentModel> models)
        {
            foreach (var comment in models)
            {
                var cmv = comment.ToModelView<IssueCommentModelView>(this);

                Children.Add(cmv);
            }
        }

        public override IServiceReport<bool> Save()
        {
            throw new NotImplementedException();
        }

        public override IServiceReport<bool> Validate()
        {
            throw new NotImplementedException();
        }
    }
}