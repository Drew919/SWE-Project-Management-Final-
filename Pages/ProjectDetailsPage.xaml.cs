using System;
using System.Collections.ObjectModel;
using System.Linq;
using ArchonPM.Navigation;
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

    public sealed class RiskItem
    {
        public int ID { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string PriorityDisplay { get; init; } = string.Empty;
        public string StatusDisplay { get; init; } = string.Empty;
        public Visibility EditVisibility { get; init; }
    }

    public sealed class TimeEntryItem
    {
        public string HeaderLine { get; init; } = string.Empty;
        public string DetailLine { get; init; } = string.Empty;
    }

    public sealed partial class ProjectDetailsPage : Page
    {
        private int _projectId;
        private int _actingMemberId;
        private bool _suppressActingAsChange;
        private bool _suppressPivotChange;
        private readonly ObservableCollection<RequirementItem> _requirements = new();
        private readonly ObservableCollection<MemberItem> _members = new();
        private readonly ObservableCollection<RiskItem> _risks = new();
        private readonly ObservableCollection<TimeEntryItem> _timeEntries = new();

        public ProjectDetailsPage()
        {
            InitializeComponent();
            RequirementsList.ItemsSource = _requirements;
            MembersList.ItemsSource = _members;
            RisksList.ItemsSource = _risks;
            TimeEntriesList.ItemsSource = _timeEntries;
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
            ApplySectionFromContext();
            App.Current.MainWindow?.RefreshNavigationChrome();
        }

        private void DetailsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressPivotChange || DetailsPivot.SelectedIndex < 0)
            {
                return;
            }

            App.Current.Navigation.CurrentSection = DetailsPivot.SelectedIndex switch
            {
                1 => ProjectSection.Requirements,
                2 => ProjectSection.PeopleAndRoles,
                3 => ProjectSection.Risks,
                4 => ProjectSection.TimeLog,
                _ => ProjectSection.Overview
            };
            App.Current.MainWindow?.RefreshNavigationChrome();
        }

        private void ApplySectionFromContext()
        {
            int index = App.Current.Navigation.CurrentSection switch
            {
                ProjectSection.Requirements => 1,
                ProjectSection.PeopleAndRoles => 2,
                ProjectSection.Risks => 3,
                ProjectSection.TimeLog => 4,
                _ => 0
            };

            _suppressPivotChange = true;
            if (DetailsPivot.Items.Count > index)
            {
                DetailsPivot.SelectedIndex = index;
            }
            _suppressPivotChange = false;
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
            App.Current.Navigation.CurrentProjectId = project.ID;
            App.Current.Navigation.CurrentProjectName = project.Name;
            App.Current.Navigation.CurrentPage = AppPageKind.ProjectDetails;

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
            AddRiskButton.Visibility = PermissionService.CanManageRisks(actingRole)
                ? Visibility.Visible
                : Visibility.Collapsed;
            LogTimeButton.Visibility = PermissionService.CanLogTime(actingRole)
                ? Visibility.Visible
                : Visibility.Collapsed;

            RefreshRequirements(project, actingRole);
            RefreshMembers(project, actingRole);
            RefreshRisks(project, actingRole);
            RefreshTimeEntries(project);
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

        private void RefreshRisks(Project project, ProjectRole actingRole)
        {
            _risks.Clear();
            bool canEdit = PermissionService.CanManageRisks(actingRole);

            foreach (Risk risk in project.RiskList)
            {
                _risks.Add(new RiskItem
                {
                    ID = risk.ID,
                    Name = risk.Name,
                    Description = risk.Description,
                    PriorityDisplay = $"Severity: {risk.Priority}",
                    StatusDisplay = $"Status: {risk.Status}",
                    EditVisibility = canEdit ? Visibility.Visible : Visibility.Collapsed
                });
            }

            bool hasItems = _risks.Count > 0;
            RisksList.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
            RisksEmptyText.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;
        }

        private void RefreshTimeEntries(Project project)
        {
            _timeEntries.Clear();

            foreach (TimeEntry entry in project.TimeEntryList)
            {
                string requirementName = entry.RequirementId.HasValue
                    ? project.RequirmentList.FirstOrDefault(r => r.ID == entry.RequirementId.Value)?.Name ?? "Removed requirement"
                    : "General";

                _timeEntries.Add(new TimeEntryItem
                {
                    HeaderLine = $"{entry.Phase} - {entry.Hours:0.##} h",
                    DetailLine = $"{requirementName}: {entry.StaffName}: {entry.Date:M/d/yyyy}"
                });
            }

            bool hasItems = _timeEntries.Count > 0;
            TimeEntriesList.Visibility = hasItems ? Visibility.Visible : Visibility.Collapsed;
            TimeEmptyText.Visibility = hasItems ? Visibility.Collapsed : Visibility.Visible;

            if (hasItems)
            {
                var totals = ProjectService.EffortPhases
                    .Select(phase => new
                    {
                        Phase = phase,
                        Hours = project.TimeEntryList.Where(t => t.Phase == phase).Sum(t => t.Hours)
                    })
                    .Where(t => t.Hours > 0)
                    .Select(t => $"{t.Phase}: {t.Hours:0.##} h");

                double total = project.TimeEntryList.Sum(t => t.Hours);
                TimeTotalsText.Text = $"Totals - {string.Join(", ", totals)}, all phases: {total:0.##} h";
                TimeTotalsText.Visibility = Visibility.Visible;
            }
            else
            {
                TimeTotalsText.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowError(string message)
        {
            DetailsPivot.Visibility = Visibility.Collapsed;
            ActingAsPanel.Visibility = Visibility.Collapsed;
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
            App.Current.Navigation.ClearProject();
            App.Current.MainWindow?.RefreshNavigationChrome();
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
                App.Current.MainWindow?.RefreshNavigationChrome();
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
                Title = $"Edit Role - {member.Name}",
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

        private async void AddRiskButton_Click(object sender, RoutedEventArgs e)
        {
            var nameBox = new TextBox { Header = "Risk Name" };
            var descriptionBox = new TextBox
            {
                Header = "Description",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 100
            };
            var priorityBox = new ComboBox { Header = "Severity" };
            foreach (RiskPriority priority in Enum.GetValues<RiskPriority>())
            {
                priorityBox.Items.Add(new ComboBoxItem { Content = priority.ToString(), Tag = priority });
            }
            priorityBox.SelectedIndex = 1;

            var statusBox = new ComboBox { Header = "Status" };
            foreach (string status in ProjectService.RiskStatuses)
            {
                statusBox.Items.Add(status);
            }
            statusBox.SelectedIndex = 0;

            var panel = new StackPanel { Spacing = 12 };
            panel.Children.Add(nameBox);
            panel.Children.Add(descriptionBox);
            panel.Children.Add(priorityBox);
            panel.Children.Add(statusBox);

            var dialog = new ContentDialog
            {
                Title = "Add Risk",
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

            try
            {
                RiskPriority priority = priorityBox.SelectedItem is ComboBoxItem item && item.Tag is RiskPriority selected
                    ? selected
                    : RiskPriority.Medium;

                App.Current.ProjectService.AddRisk(
                    _projectId,
                    nameBox.Text?.Trim() ?? string.Empty,
                    descriptionBox.Text?.Trim() ?? string.Empty,
                    priority,
                    statusBox.SelectedItem?.ToString() ?? "Open",
                    _actingMemberId);
                RefreshPage();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Unable to add risk", ex.Message);
            }
        }

        private async void UpdateRiskStatusButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not int riskId)
            {
                return;
            }

            Risk? risk = App.Current.ProjectService.GetRisks(_projectId).FirstOrDefault(r => r.ID == riskId);
            if (risk == null)
            {
                await ShowMessageAsync("Not found", "That risk no longer exists.");
                RefreshPage();
                return;
            }

            var statusBox = new ComboBox { Header = "Status", MinWidth = 260 };
            foreach (string status in ProjectService.RiskStatuses)
            {
                statusBox.Items.Add(status);
            }
            statusBox.SelectedItem = ProjectService.RiskStatuses.FirstOrDefault(s => s == risk.Status)
                                     ?? ProjectService.RiskStatuses[0];

            var dialog = new ContentDialog
            {
                Title = $"Update Status - {risk.Name}",
                Content = statusBox,
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
                App.Current.ProjectService.UpdateRiskStatus(
                    _projectId, riskId, statusBox.SelectedItem?.ToString() ?? risk.Status, _actingMemberId);
                RefreshPage();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Unable to update risk", ex.Message);
            }
        }

        private async void LogTimeButton_Click(object sender, RoutedEventArgs e)
        {
            var phaseBox = new ComboBox { Header = "Phase" };
            foreach (string phase in ProjectService.EffortPhases)
            {
                phaseBox.Items.Add(phase);
            }
            phaseBox.SelectedIndex = 0;

            var hoursBox = new TextBox { Header = "Hours", PlaceholderText = "e.g. 2.5" };

            var requirementBox = new ComboBox { Header = "Requirement (optional)" };
            requirementBox.Items.Add(new ComboBoxItem { Content = "General / none", Tag = null });
            foreach (Requirement requirement in App.Current.ProjectService.GetRequirements(_projectId))
            {
                requirementBox.Items.Add(new ComboBoxItem { Content = requirement.Name, Tag = requirement.ID });
            }
            requirementBox.SelectedIndex = 0;

            var panel = new StackPanel { Spacing = 12 };
            panel.Children.Add(phaseBox);
            panel.Children.Add(hoursBox);
            panel.Children.Add(requirementBox);

            var dialog = new ContentDialog
            {
                Title = "Log Time",
                Content = panel,
                PrimaryButtonText = "Log",
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
                if (!double.TryParse(hoursBox.Text?.Trim(), out double hours))
                {
                    await ShowMessageAsync("Invalid hours", "Enter a number of hours, like 2 or 2.5.");
                    return;
                }

                int? requirementId = requirementBox.SelectedItem is ComboBoxItem item && item.Tag is int id
                    ? id
                    : null;

                App.Current.ProjectService.AddTimeEntry(
                    _projectId,
                    phaseBox.SelectedItem?.ToString() ?? string.Empty,
                    hours,
                    requirementId,
                    _actingMemberId);
                RefreshPage();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync("Unable to log time", ex.Message);
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
