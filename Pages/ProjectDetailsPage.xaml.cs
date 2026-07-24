using System;
using System.Collections.ObjectModel;
using System.Linq;
using ArchonPM.Objects;
using ArchonPM.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace ArchonPM.Pages
{
    public sealed class RequirementItem
    {
        public int ID { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public Visibility EditVisibility { get; init; }
    }

    public sealed class MemberItem
    {
        public int ID { get; init; }
        public string Name { get; init; } = string.Empty;
        public string EmailDisplay { get; init; } = string.Empty;
        public string RoleDisplay { get; init; } = string.Empty;
        public Visibility EditVisibility { get; init; }
        public Visibility RemoveVisibility { get; init; }
    }

    public sealed partial class ProjectDetailsPage : Page
    {
        private int _projectId;
        private int _actingMemberId;
        private bool _suppressActingAsChange;
        private readonly ObservableCollection<RequirementItem> _requirements = new();
        private readonly ObservableCollection<MemberItem> _members = new();

        public ProjectDetailsPage()
        {
            InitializeComponent();
            RequirementsList.ItemsSource = _requirements;
            MembersList.ItemsSource = _members;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not int projectId)
            {
                ShowError("Invalid project. Return to Projects and try again.");
                return;
            }

            _projectId = projectId;
            RefreshPage();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ViewProjects));
        }

        private void ActingAsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressActingAsChange || ActingAsBox.SelectedItem is not ComboBoxItem item || item.Tag is not int memberId)
            {
                return;
            }

            _actingMemberId = memberId;
            RefreshPage();
        }

        private void RefreshPage()
        {
            Project? project = App.Current.ProjectService.GetProjectById(_projectId);
            if (project == null)
            {
                ShowError("This project no longer exists.");
                return;
            }

            ErrorText.Visibility = Visibility.Collapsed;
            DetailsPivot.Visibility = Visibility.Visible;
            ActingAsPanel.Visibility = Visibility.Visible;
            PageTitle.Text = project.Name;

            EnsureActingMember(project);
            PopulateActingAsBox(project);

            ProjectRole actingRole = GetActingRole(project);

            OverviewName.Text = project.Name;
            OverviewOwner.Text = $"Owner: {project.Owner}";
            OverviewStatus.Text = $"Status: {project.Status}";
            OverviewDescription.Text = string.IsNullOrWhiteSpace(project.Description)
                ? "No description provided."
                : project.Description;

            EditProjectButton.Visibility = PermissionService.CanEditProject(actingRole)
                ? Visibility.Visible
                : Visibility.Collapsed;
            AddRequirementButton.Visibility = PermissionService.CanManageRequirements(actingRole)
                ? Visibility.Visible
                : Visibility.Collapsed;
            AddMemberButton.Visibility = PermissionService.CanManageMembers(actingRole)
                ? Visibility.Visible
                : Visibility.Collapsed;

            RefreshRequirements(project, actingRole);
            RefreshMembers(project, actingRole);
        }

        private void EnsureActingMember(Project project)
        {
            if (project.StaffList.Any(m => m.ID == _actingMemberId))
            {
                return;
            }

            Staff? primary = project.StaffList.FirstOrDefault(m => m.Role == ProjectRole.PrimaryAdmin)
                             ?? project.StaffList.FirstOrDefault();
            _actingMemberId = primary?.ID ?? 0;
        }

        private void PopulateActingAsBox(Project project)
        {
            _suppressActingAsChange = true;
            ActingAsBox.Items.Clear();

            foreach (Staff member in project.StaffList)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{member.Name} ({PermissionService.GetDisplayName(member.Role)})",
                    Tag = member.ID
                };
                ActingAsBox.Items.Add(item);
                if (member.ID == _actingMemberId)
                {
                    ActingAsBox.SelectedItem = item;
                }
            }

            if (ActingAsBox.SelectedItem == null && ActingAsBox.Items.Count > 0)
            {
                ActingAsBox.SelectedIndex = 0;
                if (ActingAsBox.SelectedItem is ComboBoxItem selected && selected.Tag is int id)
                {
                    _actingMemberId = id;
                }
            }

            _suppressActingAsChange = false;
        }

        private ProjectRole GetActingRole(Project project)
        {
            return project.StaffList.FirstOrDefault(m => m.ID == _actingMemberId)?.Role
                   ?? ProjectRole.Viewer;
        }

        private void RefreshRequirements(Project project, ProjectRole actingRole)
        {
            _requirements.Clear();
            bool canEdit = PermissionService.CanManageRequirements(actingRole);

            foreach (Requirement requirement in project.RequirmentList)
            {
                _requirements.Add(new RequirementItem
                {
                    ID = requirement.ID,
                    Name = requirement.Name,
                    Description = requirement.Description,
                    Category = requirement.Catagory,
                    EditVisibility = canEdit ? Visibility.Visible : Visibility.Collapsed
                });
            }

            bool hasItems = _requirements.Count > 0;
            RequirementsList.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
            RequirementsEmptyText.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RefreshMembers(Project project, ProjectRole actingRole)
        {
            _members.Clear();

            foreach (Staff member in project.StaffList)
            {
                bool canEditRole = PermissionService.CanManageMembers(actingRole)
                    && member.Role != ProjectRole.PrimaryAdmin
                    && PermissionService.CanAssignRole(actingRole, member.Role);

                bool canRemove = PermissionService.CanRemoveMember(actingRole, member.Role);

                _members.Add(new MemberItem
                {
                    ID = member.ID,
                    Name = member.Name,
                    EmailDisplay = string.IsNullOrWhiteSpace(member.Email) ? "No email" : member.Email,
                    RoleDisplay = $"Role: {PermissionService.GetDisplayName(member.Role)}",
                    EditVisibility = canEditRole ? Visibility.Visible : Visibility.Collapsed,
                    RemoveVisibility = canRemove ? Visibility.Visible : Visibility.Collapsed
                });
            }

            bool hasItems = _members.Count > 0;
            MembersList.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
            MembersEmptyText.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ShowError(string message)
        {
            DetailsPivot.Visibility = Visibility.Collapsed;
            ActingAsPanel.Visibility = Visibility.Collapsed;
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private async void EditProjectButton_Click(object sender, RoutedEventArgs e)
        {
            Project? project = App.Current.ProjectService.GetProjectById(_projectId);
            if (project == null)
            {
                ShowError("This project no longer exists.");
                return;
            }

            var nameBox = new TextBox { Header = "Project Name", Text = project.Name };
            var ownerBox = new TextBox { Header = "Owner", Text = project.Owner };
            var descriptionBox = new TextBox
            {
                Header = "Description",
                Text = project.Description,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 100
            };
            var statusBox = new ComboBox { Header = "Status" };
            foreach (string status in new[] { "Planning", "Not Started", "In Progress", "Completed" })
            {
                statusBox.Items.Add(status);
            }

            statusBox.SelectedItem = statusBox.Items.Cast<string>().FirstOrDefault(s => s == project.Status)
                                     ?? statusBox.Items[0];

            var panel = new StackPanel { Spacing = 12 };
            panel.Children.Add(nameBox);
            panel.Children.Add(ownerBox);
            panel.Children.Add(descriptionBox);
            panel.Children.Add(statusBox);

            var dialog = new ContentDialog
            {
                Title = "Edit Project",
                Content = panel,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            try
            {
                App.Current.ProjectService.UpdateProject(
                    _projectId,
                    nameBox.Text?.Trim() ?? string.Empty,
                    descriptionBox.Text?.Trim() ?? string.Empty,
                    ownerBox.Text?.Trim() ?? string.Empty,
                    statusBox.SelectedItem?.ToString() ?? project.Status,
                    _actingMemberId);
                RefreshPage();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Unable to save", ex.Message);
            }
        }

        private async void AddRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowRequirementDialogAsync(null);
        }

        private async void EditRequirementButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not int requirementId)
            {
                return;
            }

            Requirement? requirement = App.Current.ProjectService.GetRequirements(_projectId)
                .FirstOrDefault(r => r.ID == requirementId);
            if (requirement == null)
            {
                await ShowMessageAsync("Not found", "That requirement no longer exists.");
                RefreshPage();
                return;
            }

            await ShowRequirementDialogAsync(requirement);
        }

        private async System.Threading.Tasks.Task ShowRequirementDialogAsync(Requirement? existing)
        {
            var nameBox = new TextBox { Header = "Requirement Name", Text = existing?.Name ?? string.Empty };
            var descriptionBox = new TextBox
            {
                Header = "Description",
                Text = existing?.Description ?? string.Empty,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 100
            };
            var categoryBox = new ComboBox { Header = "Type" };
            categoryBox.Items.Add("Functional");
            categoryBox.Items.Add("Non-Functional");
            categoryBox.SelectedItem = existing?.Catagory == "Non-Functional" ? "Non-Functional" : "Functional";

            var panel = new StackPanel { Spacing = 12 };
            panel.Children.Add(nameBox);
            panel.Children.Add(descriptionBox);
            panel.Children.Add(categoryBox);

            var dialog = new ContentDialog
            {
                Title = existing == null ? "Add Requirement" : "Edit Requirement",
                Content = panel,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            try
            {
                string name = nameBox.Text?.Trim() ?? string.Empty;
                string description = descriptionBox.Text?.Trim() ?? string.Empty;
                string category = categoryBox.SelectedItem?.ToString() ?? "Functional";

                if (existing == null)
                {
                    App.Current.ProjectService.AddRequirement(_projectId, name, description, category, _actingMemberId);
                }
                else
                {
                    App.Current.ProjectService.UpdateRequirement(
                        _projectId, existing.ID, name, description, category, _actingMemberId);
                }

                RefreshPage();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Unable to save requirement", ex.Message);
            }
        }

        private async void AddMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isSubmittingMember)
            {
                return;
            }

            var nameBox = new TextBox { Header = "Name" };
            var emailBox = new TextBox { Header = "Email (optional)" };
            var roleBox = CreateAssignableRoleBox(excludePrimary: true);

            var panel = new StackPanel { Spacing = 12 };
            panel.Children.Add(nameBox);
            panel.Children.Add(emailBox);
            panel.Children.Add(roleBox);

            var dialog = new ContentDialog
            {
                Title = "Add Person",
                Content = panel,
                PrimaryButtonText = "Add",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            _isSubmittingMember = true;
            try
            {
                ProjectRole role = roleBox.SelectedItem is ComboBoxItem item && item.Tag is ProjectRole selectedRole
                    ? selectedRole
                    : ProjectRole.Viewer;

                App.Current.ProjectService.AddProjectMember(
                    _projectId,
                    nameBox.Text?.Trim() ?? string.Empty,
                    emailBox.Text,
                    role,
                    _actingMemberId);
                RefreshPage();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Unable to add person", ex.Message);
            }
            finally
            {
                _isSubmittingMember = false;
            }
        }

        private bool _isSubmittingMember;

        private async void EditRoleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not int memberId)
            {
                return;
            }

            Staff? member = App.Current.ProjectService.GetProjectMembers(_projectId)
                .FirstOrDefault(m => m.ID == memberId);
            if (member == null)
            {
                await ShowMessageAsync("Not found", "That member no longer exists.");
                RefreshPage();
                return;
            }

            ComboBox roleBox = CreateAssignableRoleBox(excludePrimary: true);
            foreach (ComboBoxItem item in roleBox.Items.OfType<ComboBoxItem>())
            {
                if (item.Tag is ProjectRole role && role == member.Role)
                {
                    roleBox.SelectedItem = item;
                    break;
                }
            }

            var dialog = new ContentDialog
            {
                Title = $"Edit Role — {member.Name}",
                Content = roleBox,
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            try
            {
                ProjectRole newRole = roleBox.SelectedItem is ComboBoxItem item && item.Tag is ProjectRole selectedRole
                    ? selectedRole
                    : member.Role;

                App.Current.ProjectService.UpdateMemberRole(_projectId, memberId, newRole, _actingMemberId);
                RefreshPage();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Unable to update role", ex.Message);
            }
        }

        private async void RemoveMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not int memberId)
            {
                return;
            }

            Staff? member = App.Current.ProjectService.GetProjectMembers(_projectId)
                .FirstOrDefault(m => m.ID == memberId);
            if (member == null)
            {
                RefreshPage();
                return;
            }

            var confirm = new ContentDialog
            {
                Title = "Remove Person",
                Content = $"Remove {member.Name} from this project?",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };

            if (await confirm.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            try
            {
                App.Current.ProjectService.RemoveProjectMember(_projectId, memberId, _actingMemberId);
                RefreshPage();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Unable to remove person", ex.Message);
            }
        }

        private ComboBox CreateAssignableRoleBox(bool excludePrimary)
        {
            var roleBox = new ComboBox { Header = "Role", MinWidth = 260 };
            foreach (ProjectRole role in Enum.GetValues<ProjectRole>())
            {
                if (excludePrimary && role == ProjectRole.PrimaryAdmin)
                {
                    continue;
                }

                roleBox.Items.Add(new ComboBoxItem
                {
                    Content = PermissionService.GetDisplayName(role),
                    Tag = role
                });
            }

            roleBox.SelectedIndex = Math.Max(0, roleBox.Items.Count - 1);
            return roleBox;
        }

        private async System.Threading.Tasks.Task ShowMessageAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
