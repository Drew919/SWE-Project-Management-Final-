using ArchonPM.Objects;

namespace ArchonPM.Services
{
    public static class PermissionService
    {
        public static string GetDisplayName(ProjectRole role) => role switch
        {
            ProjectRole.PrimaryAdmin => "Primary Admin",
            ProjectRole.SecondaryAdmin => "Secondary Admin",
            ProjectRole.ProjectManager => "Project Manager",
            ProjectRole.SoftwareEngineer => "Software Engineer",
            ProjectRole.Viewer => "Viewer",
            _ => role.ToString()
        };

        public static bool CanViewProject(ProjectRole role) => true;

        public static bool CanEditProject(ProjectRole role) =>
            role is ProjectRole.PrimaryAdmin or ProjectRole.SecondaryAdmin or ProjectRole.ProjectManager;

        public static bool CanViewRequirements(ProjectRole role) => true;

        public static bool CanManageRequirements(ProjectRole role) =>
            role is ProjectRole.PrimaryAdmin
                or ProjectRole.SecondaryAdmin
                or ProjectRole.ProjectManager
                or ProjectRole.SoftwareEngineer;

        public static bool CanViewMembers(ProjectRole role) => true;

        public static bool CanManageMembers(ProjectRole role) =>
            role is ProjectRole.PrimaryAdmin or ProjectRole.SecondaryAdmin;

        public static bool CanAssignRole(ProjectRole actorRole, ProjectRole targetRole)
        {
            if (actorRole == ProjectRole.PrimaryAdmin)
            {
                return true;
            }

            if (actorRole == ProjectRole.SecondaryAdmin)
            {
                return targetRole is not ProjectRole.PrimaryAdmin and not ProjectRole.SecondaryAdmin;
            }

            return false;
        }

        public static bool CanRemoveMember(ProjectRole actorRole, ProjectRole targetRole)
        {
            if (targetRole == ProjectRole.PrimaryAdmin)
            {
                return false;
            }

            if (actorRole == ProjectRole.PrimaryAdmin)
            {
                return true;
            }

            if (actorRole == ProjectRole.SecondaryAdmin)
            {
                return targetRole != ProjectRole.PrimaryAdmin;
            }

            return false;
        }

        public static bool CanTransferPrimaryAdmin(ProjectRole actorRole) =>
            actorRole == ProjectRole.PrimaryAdmin;
    }
}
