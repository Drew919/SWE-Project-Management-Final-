using System;
using System.Linq;
using ArchonPM.Navigation;
using ArchonPM.Objects;
using ArchonPM.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ArchonPM;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        rootFrame.Navigated += RootFrame_Navigated;
        NavigateToDashboard();
    }

    public void NavigateToDashboard()
    {
        App.Current.Navigation.CurrentPage = AppPageKind.Dashboard;
        App.Current.Navigation.ClearProject();
        rootFrame.Navigate(typeof(Dashboard));
        RefreshNavigationChrome();
    }

    public void NavigateToProjects()
    {
        App.Current.Navigation.CurrentPage = AppPageKind.Projects;
        App.Current.Navigation.ClearProject();
        rootFrame.Navigate(typeof(ViewProjects));
        RefreshNavigationChrome();
    }

    public void NavigateToCreateProject()
    {
        App.Current.Navigation.CurrentPage = AppPageKind.CreateProject;
        App.Current.Navigation.ClearProject();
        rootFrame.Navigate(typeof(CreateProject));
        RefreshNavigationChrome();
    }

    public void NavigateToProject(int projectId, ProjectSection? preferredSection = null)
    {
        Project? project = App.Current.ProjectService.GetProjectById(projectId);
        if (project == null)
        {
            App.Current.Navigation.ClearProject();
            RefreshNavigationChrome();
            NavigateToProjects();
            return;
        }

        if (App.Current.Navigation.CurrentPage == AppPageKind.ProjectDetails &&
            App.Current.Navigation.CurrentProjectId == projectId &&
            rootFrame.Content is ProjectDetailsPage)
        {
            RefreshNavigationChrome();
            return;
        }

        if (preferredSection.HasValue)
        {
            App.Current.Navigation.CurrentSection = preferredSection.Value;
        }

        App.Current.Navigation.CurrentPage = AppPageKind.ProjectDetails;
        App.Current.Navigation.CurrentProjectId = project.ID;
        App.Current.Navigation.CurrentProjectName = project.Name;
        rootFrame.Navigate(typeof(ProjectDetailsPage), project.ID);
        RefreshNavigationChrome();
    }

    public void RefreshNavigationChrome()
    {
        NavigationContext nav = App.Current.Navigation;
        RebuildProjectSelectorBox();
        ContextLabel.Text = nav.GetContextLabel();
    }

    private void RebuildProjectSelectorBox()
    {
        var projects = App.Current.ProjectService.GetAllProjects();
        int? currentId = App.Current.Navigation.CurrentProjectId;
        string currentName = App.Current.Navigation.CurrentProjectName;

        ProjectSelectorBox.Items.Clear();

        int selectedIndex = -1;

        foreach (Project project in projects)
        {
            var item = new ComboBoxItem
            {
                Content = project.Name,
                Tag = project.ID
            };
            ProjectSelectorBox.Items.Add(item);
            if (currentId == project.ID)
            {
                selectedIndex = ProjectSelectorBox.Items.Count - 1;
            }
        }

        if (selectedIndex >= 0)
        {
            _suppressProjectSelectorChange = true;
            ProjectSelectorBox.SelectedIndex = selectedIndex;
            _suppressProjectSelectorChange = false;
        }
        else
        {
            var placeholderItem = new ComboBoxItem { Content = "Select project" };
            ProjectSelectorBox.Items.Insert(0, placeholderItem);
            ProjectSelectorBox.SelectedIndex = 0;
        }
    }

    private bool _suppressProjectSelectorChange = false;

    private void ProjectSelectorBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressProjectSelectorChange || ProjectSelectorBox.SelectedItem is not ComboBoxItem item)
        {
            return;
        }

        if (item.Tag is int projectId)
        {
            NavigateToProject(projectId, App.Current.Navigation.CurrentSection);
        }
    }

    private void DashboardButton_Click(object sender, RoutedEventArgs e)
    {
        NavigateToDashboard();
    }



    private void RootFrame_Navigated(object sender, NavigationEventArgs e)
    {
        Type? pageType = e.SourcePageType;
        NavigationContext nav = App.Current.Navigation;

        if (pageType == typeof(Dashboard))
        {
            nav.CurrentPage = AppPageKind.Dashboard;
            nav.ClearProject();
        }
        else if (pageType == typeof(ViewProjects))
        {
            nav.CurrentPage = AppPageKind.Projects;
            nav.ClearProject();
        }
        else if (pageType == typeof(CreateProject))
        {
            nav.CurrentPage = AppPageKind.CreateProject;
            nav.ClearProject();
        }
        else if (pageType == typeof(ProjectDetailsPage))
        {
            nav.CurrentPage = AppPageKind.ProjectDetails;
            if (e.Parameter is int projectId)
            {
                Project? project = App.Current.ProjectService.GetProjectById(projectId);
                if (project != null)
                {
                    nav.CurrentProjectId = project.ID;
                    nav.CurrentProjectName = project.Name;
                }
                else
                {
                    nav.ClearProject();
                }
            }
        }

        RefreshNavigationChrome();
    }


}
