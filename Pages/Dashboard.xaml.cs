using System.Collections.ObjectModel;
using System.Linq;
using ArchonPM.Objects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ArchonPM.Pages
{
    public sealed partial class Dashboard : Page
    {
        private readonly ObservableCollection<ProjectListItem> _recentProjects = new();

        public Dashboard()
        {
            InitializeComponent();
            RecentProjectsList.ItemsSource = _recentProjects;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RefreshDashboard();
        }

        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateProject));
        }

        private void ViewProjects_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ViewProjects));
        }

        private void RecentProjectsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ProjectListItem item)
            {
                Frame.Navigate(typeof(ProjectDetailsPage), item.ID);
            }
        }

        private void RefreshDashboard()
        {
            var projects = App.Current.ProjectService.GetAllProjects();

            TotalCountText.Text = projects.Count.ToString();
            PlanningCountText.Text = projects.Count(p => p.Status == "Planning").ToString();
            InProgressCountText.Text = projects.Count(p => p.Status == "In Progress").ToString();
            CompletedCountText.Text = projects.Count(p => p.Status == "Completed").ToString();

            _recentProjects.Clear();
            foreach (Project project in projects.Reverse().Take(5))
            {
                _recentProjects.Add(new ProjectListItem
                {
                    ID = project.ID,
                    Name = project.Name,
                    Status = project.Status,
                    OwnerLine = $"Owner: {project.Owner}",
                    DescriptionPreview = project.Description ?? string.Empty,
                    CountsLine = string.Empty
                });
            }

            bool hasProjects = projects.Count > 0;
            EmptyStatePanel.Visibility = hasProjects ? Visibility.Collapsed : Visibility.Visible;
            RecentSection.Visibility = hasProjects ? Visibility.Visible : Visibility.Collapsed;
            StatsGrid.Visibility = Visibility.Visible;
        }
    }
}
