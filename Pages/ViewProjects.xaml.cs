using System;
using System.Collections.ObjectModel;
using System.Linq;
using ArchonPM.Objects;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ArchonPM.Pages
{
    public sealed class ProjectListItem
    {
        public int ID { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string OwnerLine { get; init; } = string.Empty;
        public string DescriptionPreview { get; init; } = string.Empty;
        public string CountsLine { get; init; } = string.Empty;
    }

    public sealed partial class ViewProjects : Page
    {
        private readonly ObservableCollection<ProjectListItem> _displayedProjects = new();

        public ViewProjects()
        {
            InitializeComponent();
            ProjectsList.ItemsSource = _displayedProjects;
            StatusFilterBox.SelectedIndex = 0;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            RefreshProjects();
        }

        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateProject));
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            RefreshProjects();
        }

        private void ProjectsList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ProjectListItem item)
            {
                Frame.Navigate(typeof(ProjectDetailsPage), item.ID);
            }
        }

        private void RefreshProjects()
        {
            _displayedProjects.Clear();

            string search = SearchBox.Text?.Trim() ?? string.Empty;
            string statusFilter = (StatusFilterBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;

            foreach (Project project in App.Current.ProjectService.GetAllProjects())
            {
                if (!string.IsNullOrEmpty(statusFilter) &&
                    !string.Equals(project.Status, statusFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(search))
                {
                    bool matches =
                        (project.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (project.Owner?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false);
                    if (!matches)
                    {
                        continue;
                    }
                }

                string description = project.Description ?? string.Empty;
                string preview = description.Length <= 120
                    ? description
                    : description.Substring(0, 117) + "...";

                int memberCount = project.StaffList?.Count ?? 0;
                int requirementCount = project.RequirmentList?.Count ?? 0;

                _displayedProjects.Add(new ProjectListItem
                {
                    ID = project.ID,
                    Name = project.Name,
                    Status = project.Status,
                    OwnerLine = $"Owner: {project.Owner}",
                    DescriptionPreview = string.IsNullOrWhiteSpace(preview) ? "No description" : preview,
                    CountsLine = $"{memberCount} members · {requirementCount} requirements"
                });
            }

            bool hasProjects = _displayedProjects.Count > 0;
            bool anyProjectsExist = App.Current.ProjectService.GetAllProjects().Count > 0;

            ProjectsList.Visibility = hasProjects ? Visibility.Visible : Visibility.Collapsed;
            EmptyStatePanel.Visibility = hasProjects ? Visibility.Collapsed : Visibility.Visible;

            if (!hasProjects && anyProjectsExist)
            {
                EmptyStatePanel.Children.Clear();
                EmptyStatePanel.Children.Add(new TextBlock
                {
                    Text = "No projects match your search or filter.",
                    FontSize = 16,
                    Opacity = 0.7,
                    TextWrapping = TextWrapping.WrapWholeWords
                });
            }
            else if (!hasProjects)
            {
                EmptyStatePanel.Children.Clear();
                EmptyStatePanel.Children.Add(new TextBlock
                {
                    Text = "No projects yet. Create your first project to get started.",
                    FontSize = 16,
                    Opacity = 0.7,
                    TextWrapping = TextWrapping.WrapWholeWords
                });
                var createButton = new Button { Content = "Create Project", HorizontalAlignment = HorizontalAlignment.Left };
                createButton.Click += CreateProject_Click;
                EmptyStatePanel.Children.Add(createButton);
            }
        }
    }
}
