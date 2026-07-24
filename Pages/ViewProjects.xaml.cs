using ArchonPM.Objects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace ArchonPM.Pages
{
    public sealed partial class ViewProjects : Page
    {
        private readonly ObservableCollection<Project> _displayedProjects = new();

        public ViewProjects()
        {
            InitializeComponent();
            ProjectsList.ItemsSource = _displayedProjects;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RefreshProjects();
        }

        private void RefreshProjects()
        {
            _displayedProjects.Clear();

            foreach (Project project in App.Current.ProjectService.GetAllProjects())
            {
                _displayedProjects.Add(project);
            }

            bool hasProjects = _displayedProjects.Count > 0;
            ProjectsList.Visibility = hasProjects ? Visibility.Visible : Visibility.Collapsed;
            EmptyStateText.Visibility = hasProjects ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
