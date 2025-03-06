using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.GitTracker.Core.WPF.Mvvm.Models;
using SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels;
using SharpBIM.Services.UtilityServices;
using SharpBIM.UIContexts;
using SharpBIM.Utility;
using SharpBIM.Utility.Extensions;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Controls.UserControls;
using SharpBIM.WPF.Helpers.Commons;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.Views
{
    /// <summary>
    /// Interaction logic for LableSelectionView.xaml
    /// </summary>
    public partial class LableSelectionView : SharpBIMUserControl
    {
        public LableSelectionView()
        {
            InitializeComponent();
        }
    }

    public class LabelsSelectionViewModel : ModelViewBase
    {
        public LabelsSelectionViewModel()
        {
            CreateLabelCommand = new SharpBIMCommand(async (x) => await CreateLabel(x), "Create new Label", Glyphs.list_latin_big, (x) => true);
            PushButtonCommand = new SharpBIMCommand(async (x) => await PushNewLabel(x), "Push Label", Glyphs.upload, (x) => true);
        }

        public SharpBIMCommand PushButtonCommand { get; set; }

        // Add this line to the constructor

        public ObservableCollection<LabelModelView> Children { get; set; } = [];

        public async Task Init(IssueViewModel issuemv)
        {
            var loadedLables = issuemv.IssueLables;

            var getallLabelsReport = await LabelService.GetLables(issuemv.SelectedRepo.name);
            if (getallLabelsReport.IsFailed)
            {
                AppGlobals.MsgService.AlertUser(WindowHandle, "Unable to get Labels", "Couldn't fetch labels from your Repo.");
                return;
            }

            var labels = getallLabelsReport.Model;
            foreach (var lable in labels)
            {
                var labelMv = lable.ToModelView<LabelModelView>(issuemv);
                Children.Add(labelMv);
                var loadedmv = loadedLables.FirstOrDefault(o => o.Id == labelMv.Id);
                if (loadedmv != null)
                {
                    labelMv.IsAvailable = loadedmv.IsAvailable;
                }
            }
        }

        public SharpBIMCommand CreateLabelCommand { get; set; }

        // Add this line to the constructor

        public string NewLabelName
        {
            get { return GetValue<string>(nameof(NewLabelName)); }
            set { SetValue(value, nameof(NewLabelName)); }
        }

        public bool CreateNewLabel
        {
            get { return GetValue<bool>(nameof(CreateNewLabel)); }
            set { SetValue(value, nameof(CreateNewLabel)); }
        }

        public string NewLabelColor
        {
            get { return GetValue<string>(nameof(NewLabelColor)); }
            set { SetValue(value, nameof(NewLabelColor)); }
        }

        public string NewLabelDescription
        {
            get { return GetValue<string>(nameof(NewLabelDescription)); }
            set { SetValue(value, nameof(NewLabelDescription)); }
        }

        public async Task CreateLabel(object x)
        {
            try
            {
                CreateNewLabel = !CreateNewLabel;
            }
            catch (Exception ex)
            {
            }
        }

        private async Task PushNewLabel(object x)
        {
            if (string.IsNullOrEmpty(NewLabelName))
                return;

            if (Children.Any(o => o.Title.EQ(NewLabelName)))
                return;

            var labelModel = new GitLabel();
            labelModel.name = NewLabelName;
            labelModel.color = NewLabelColor.Remove("#");

            var createReport = await LabelService.CreateLable(GetParentViewModel<IssueViewModel>().SelectedRepo.name, labelModel);
            if (createReport.IsFailed)
            {
                AppGlobals.MsgService.AlertUser(WindowHandle, "Can't Create Label", createReport.ErrorMessage);
            }
        }
    }
}