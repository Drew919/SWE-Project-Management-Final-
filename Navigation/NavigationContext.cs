namespace ArchonPM.Navigation
{
    public enum AppPageKind
    {
        Dashboard,
        Projects,
        CreateProject,
        ProjectDetails
    }

    public enum ProjectSection
    {
        Overview,
        Requirements,
        PeopleAndRoles,
        Risks,
        TimeLog
    }

    public sealed class NavigationContext
    {
        public AppPageKind CurrentPage { get; set; } = AppPageKind.Dashboard;
        public int? CurrentProjectId { get; set; }
        public string CurrentProjectName { get; set; } = string.Empty;
        public ProjectSection CurrentSection { get; set; } = ProjectSection.Overview;

        public void ClearProject()
        {
            CurrentProjectId = null;
            CurrentProjectName = string.Empty;
            CurrentSection = ProjectSection.Overview;
        }

        public string GetContextLabel()
        {
            return CurrentPage switch
            {
                AppPageKind.Dashboard => string.Empty,
                AppPageKind.Projects => "Projects",
                AppPageKind.CreateProject => "New Project",
                AppPageKind.ProjectDetails when !string.IsNullOrWhiteSpace(CurrentProjectName) =>
                    $"{CurrentProjectName} · {GetSectionDisplayName(CurrentSection)}",
                AppPageKind.ProjectDetails => "Project Details",
                _ => string.Empty
            };
        }

        public static string GetSectionDisplayName(ProjectSection section) => section switch
        {
            ProjectSection.Overview => "Overview",
            ProjectSection.Requirements => "Requirements",
            ProjectSection.PeopleAndRoles => "People and Roles",
            ProjectSection.Risks => "Risks",
            ProjectSection.TimeLog => "Time Log",
            _ => section.ToString()
        };
    }
}
