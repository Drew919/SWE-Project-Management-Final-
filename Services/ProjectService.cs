using System;
using System.Collections.Generic;
using System.Linq;
using ArchonPM.Objects;

namespace ArchonPM.Services
{
    public class ProjectService : IProjectService
    {
        private readonly List<Project> _projects = new();
        private int _nextProjectId = 1;
        private int _nextMemberId = 1;
        private int _nextRequirementId = 1;
        private int _nextRiskId = 1;
        private int _nextTimeEntryId = 1;

        public static readonly string[] EffortPhases =
        {
            "Requirements Analysis", "Designing", "Coding", "Testing", "Project Management"
        };

        public static readonly string[] RiskStatuses = { "Open", "Mitigating", "Resolved" };

        public IReadOnlyList<Project> GetAllProjects() => _projects.AsReadOnly();

        public Project? GetProjectById(int projectId)
        {
            return _projects.FirstOrDefault(p => p.ID == projectId);
        }

        public Project? GetProjectByName(string name)
        {
            return _projects.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public bool DeleteProject(int projectId)
        {
            Project? project = GetProjectById(projectId);
            return project != null && _projects.Remove(project);
        }

        public void AddProject(Project project)
        {
            ArgumentNullException.ThrowIfNull(project);

            if (string.IsNullOrWhiteSpace(project.Name))
            {
                throw new ArgumentException("Project name is required.", nameof(project));
            }

            if (string.IsNullOrWhiteSpace(project.Owner))
            {
                throw new ArgumentException("Project owner is required.", nameof(project));
            }

            project.StaffList ??= new LinkedList<Staff>();
            project.RiskList ??= new LinkedList<Risk>();
            project.RequirmentList ??= new LinkedList<Requirement>();
            project.DeliverableList ??= new LinkedList<Deliverable>();

            if (project.ID == 0)
            {
                project.ID = _nextProjectId++;
            }

            if (!project.StaffList.Any(m => m.Role == ProjectRole.PrimaryAdmin))
            {
                project.StaffList.AddLast(new Staff
                {
                    ID = _nextMemberId++,
                    Name = project.Owner.Trim(),
                    Role = ProjectRole.PrimaryAdmin
                });
            }

            _projects.Add(project);
        }

        public void UpdateProject(int projectId, string name, string description, string owner, string status, int actorMemberId)
        {
            Project project = GetRequiredProject(projectId);
            Staff actor = GetRequiredMember(project, actorMemberId);

            if (!PermissionService.CanEditProject(actor.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to edit this project.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Project name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentException("Project owner is required.", nameof(owner));
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Project status is required.", nameof(status));
            }

            string trimmedOwner = owner.Trim();
            bool ownerChanged = !string.Equals(project.Owner, trimmedOwner, StringComparison.Ordinal);

            if (ownerChanged)
            {
                if (!PermissionService.CanTransferPrimaryAdmin(actor.Role))
                {
                    throw new UnauthorizedAccessException("Only the Primary Admin can transfer project ownership.");
                }

                TransferPrimaryAdmin(project, trimmedOwner);
            }

            project.Name = name.Trim();
            project.Description = description?.Trim() ?? string.Empty;
            project.Owner = trimmedOwner;
            project.Status = status.Trim();
        }

        public IReadOnlyList<Staff> GetProjectMembers(int projectId)
        {
            Project project = GetRequiredProject(projectId);
            return project.StaffList.ToList().AsReadOnly();
        }

        public void AddProjectMember(int projectId, string name, string? email, ProjectRole role, int actorMemberId)
        {
            Project project = GetRequiredProject(projectId);
            Staff actor = GetRequiredMember(project, actorMemberId);

            if (!PermissionService.CanManageMembers(actor.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to add project members.");
            }

            if (!PermissionService.CanAssignRole(actor.Role, role))
            {
                throw new UnauthorizedAccessException("You do not have permission to assign that role.");
            }

            if (role == ProjectRole.PrimaryAdmin)
            {
                throw new InvalidOperationException("Use ownership transfer to assign Primary Admin.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Member name is required.", nameof(name));
            }

            string trimmedName = name.Trim();
            string? trimmedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

            if (trimmedEmail != null &&
                project.StaffList.Any(m =>
                    m.Email != null &&
                    string.Equals(m.Email, trimmedEmail, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A member with that email already exists on this project.");
            }

            if (project.StaffList.Any(m =>
                    string.Equals(m.Name, trimmedName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(m.Email ?? string.Empty, trimmedEmail ?? string.Empty, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("That member is already on this project.");
            }

            project.StaffList.AddLast(new Staff
            {
                ID = _nextMemberId++,
                Name = trimmedName,
                Email = trimmedEmail,
                Role = role
            });
        }

        public void UpdateMemberRole(int projectId, int memberId, ProjectRole newRole, int actorMemberId)
        {
            Project project = GetRequiredProject(projectId);
            Staff actor = GetRequiredMember(project, actorMemberId);
            Staff target = GetRequiredMember(project, memberId);

            if (!PermissionService.CanManageMembers(actor.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to change member roles.");
            }

            if (target.Role == ProjectRole.PrimaryAdmin || newRole == ProjectRole.PrimaryAdmin)
            {
                throw new InvalidOperationException("Primary Admin can only be changed by transferring ownership.");
            }

            if (!PermissionService.CanAssignRole(actor.Role, newRole) ||
                !PermissionService.CanAssignRole(actor.Role, target.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to assign that role.");
            }

            if (actor.Role == ProjectRole.SecondaryAdmin && newRole == ProjectRole.SecondaryAdmin)
            {
                throw new UnauthorizedAccessException("Secondary Admins cannot assign Secondary Admin.");
            }

            target.Role = newRole;
        }

        public void RemoveProjectMember(int projectId, int memberId, int actorMemberId)
        {
            Project project = GetRequiredProject(projectId);
            Staff actor = GetRequiredMember(project, actorMemberId);
            Staff target = GetRequiredMember(project, memberId);

            if (!PermissionService.CanManageMembers(actor.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to remove members.");
            }

            if (!PermissionService.CanRemoveMember(actor.Role, target.Role))
            {
                throw new UnauthorizedAccessException("That member cannot be removed with your role.");
            }

            if (target.Role == ProjectRole.PrimaryAdmin)
            {
                throw new InvalidOperationException("Primary Admin cannot be removed. Transfer ownership first.");
            }

            project.StaffList.Remove(target);
        }

        public IReadOnlyList<Requirement> GetRequirements(int projectId)
        {
            Project project = GetRequiredProject(projectId);
            return project.RequirmentList.ToList().AsReadOnly();
        }

        public void AddRequirement(int projectId, string name, string description, string category, int actorMemberId)
        {
            Project project = GetRequiredProject(projectId);
            Staff actor = GetRequiredMember(project, actorMemberId);

            if (!PermissionService.CanManageRequirements(actor.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to add requirements.");
            }

            ValidateRequirementFields(name, category);

            project.RequirmentList.AddLast(new Requirement
            {
                ID = _nextRequirementId++,
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Catagory = NormalizeCategory(category)
            });
        }

        public void UpdateRequirement(int projectId, int requirementId, string name, string description, string category, int actorMemberId)
        {
            Project project = GetRequiredProject(projectId);
            Staff actor = GetRequiredMember(project, actorMemberId);

            if (!PermissionService.CanManageRequirements(actor.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to edit requirements.");
            }

            Requirement? requirement = project.RequirmentList.FirstOrDefault(r => r.ID == requirementId);
            if (requirement == null)
            {
                throw new KeyNotFoundException($"Requirement {requirementId} was not found.");
            }

            ValidateRequirementFields(name, category);

            requirement.Name = name.Trim();
            requirement.Description = description?.Trim() ?? string.Empty;
            requirement.Catagory = NormalizeCategory(category);
        }

        public IReadOnlyList<Risk> GetRisks(int projectId)
        {
            Project project = GetRequiredProject(projectId);
            return project.RiskList.ToList().AsReadOnly();
        }

        public void AddRisk(int projectId, string name, string description, RiskPriority priority, string status, int actorMemberId)
        {
            Project project = GetRequiredProject(projectId);
            Staff actor = GetRequiredMember(project, actorMemberId);

            if (!PermissionService.CanManageRisks(actor.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to add risks.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Risk name is required.", nameof(name));
            }

            ValidateRiskStatus(status);

            project.RiskList.AddLast(new Risk
            {
                ID = _nextRiskId++,
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Priority = priority,
                Status = status.Trim()
            });
        }

        public void UpdateRiskStatus(int projectId, int riskId, string status, int actorMemberId)
        {
            Project project = GetRequiredProject(projectId);
            Staff actor = GetRequiredMember(project, actorMemberId);

            if (!PermissionService.CanManageRisks(actor.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to update risks.");
            }

            Risk? risk = project.RiskList.FirstOrDefault(r => r.ID == riskId);
            if (risk == null)
            {
                throw new KeyNotFoundException($"Risk {riskId} was not found.");
            }

            ValidateRiskStatus(status);
            risk.Status = status.Trim();
        }

        public IReadOnlyList<TimeEntry> GetTimeEntries(int projectId)
        {
            Project project = GetRequiredProject(projectId);
            return project.TimeEntryList.ToList().AsReadOnly();
        }

        public void AddTimeEntry(int projectId, string phase, double hours, int? requirementId, int actorMemberId)
        {
            Project project = GetRequiredProject(projectId);
            Staff actor = GetRequiredMember(project, actorMemberId);

            if (!PermissionService.CanLogTime(actor.Role))
            {
                throw new UnauthorizedAccessException("You do not have permission to log time.");
            }

            if (hours <= 0)
            {
                throw new ArgumentException("Hours must be greater than zero.", nameof(hours));
            }

            string trimmedPhase = phase?.Trim() ?? string.Empty;
            if (!EffortPhases.Any(p => p.Equals(trimmedPhase, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Phase must be one of the tracked effort phases.", nameof(phase));
            }

            if (requirementId.HasValue && !project.RequirmentList.Any(r => r.ID == requirementId.Value))
            {
                throw new KeyNotFoundException($"Requirement {requirementId.Value} was not found.");
            }

            project.TimeEntryList.AddLast(new TimeEntry
            {
                ID = _nextTimeEntryId++,
                StaffName = actor.Name,
                Hours = hours,
                Date = DateTime.Now,
                Phase = EffortPhases.First(p => p.Equals(trimmedPhase, StringComparison.OrdinalIgnoreCase)),
                RequirementId = requirementId
            });
        }

        private static void ValidateRiskStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status) ||
                !RiskStatuses.Any(s => s.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Status must be Open, Mitigating, or Resolved.", nameof(status));
            }
        }

        private void TransferPrimaryAdmin(Project project, string newOwnerName)
        {
            Staff? currentPrimary = project.StaffList.FirstOrDefault(m => m.Role == ProjectRole.PrimaryAdmin);
            Staff? matchingMember = project.StaffList.FirstOrDefault(m =>
                string.Equals(m.Name, newOwnerName, StringComparison.OrdinalIgnoreCase));

            if (matchingMember != null)
            {
                if (currentPrimary != null && currentPrimary.ID != matchingMember.ID)
                {
                    currentPrimary.Role = ProjectRole.SecondaryAdmin;
                }

                matchingMember.Role = ProjectRole.PrimaryAdmin;
            }
            else if (currentPrimary != null)
            {
                currentPrimary.Name = newOwnerName;
                currentPrimary.Role = ProjectRole.PrimaryAdmin;
            }
            else
            {
                project.StaffList.AddLast(new Staff
                {
                    ID = _nextMemberId++,
                    Name = newOwnerName,
                    Role = ProjectRole.PrimaryAdmin
                });
            }

            EnsureSinglePrimaryAdmin(project);
        }

        private static void EnsureSinglePrimaryAdmin(Project project)
        {
            var primaries = project.StaffList.Where(m => m.Role == ProjectRole.PrimaryAdmin).ToList();
            if (primaries.Count <= 1)
            {
                return;
            }

            for (int i = 1; i < primaries.Count; i++)
            {
                primaries[i].Role = ProjectRole.SecondaryAdmin;
            }
        }

        private Project GetRequiredProject(int projectId)
        {
            Project? project = GetProjectById(projectId);
            if (project == null)
            {
                throw new KeyNotFoundException($"Project {projectId} was not found.");
            }

            return project;
        }

        private static Staff GetRequiredMember(Project project, int memberId)
        {
            Staff? member = project.StaffList.FirstOrDefault(m => m.ID == memberId);
            if (member == null)
            {
                throw new KeyNotFoundException($"Member {memberId} was not found on this project.");
            }

            return member;
        }

        private static void ValidateRequirementFields(string name, string category)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Requirement name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Requirement category is required.", nameof(category));
            }

            string normalized = NormalizeCategory(category);
            if (normalized is not ("Functional" or "Non-Functional"))
            {
                throw new ArgumentException("Category must be Functional or Non-Functional.", nameof(category));
            }
        }

        private static string NormalizeCategory(string category)
        {
            string trimmed = category.Trim();
            if (trimmed.Equals("NonFunctional", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("Non-Functional", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals("Non Functional", StringComparison.OrdinalIgnoreCase))
            {
                return "Non-Functional";
            }

            if (trimmed.Equals("Functional", StringComparison.OrdinalIgnoreCase))
            {
                return "Functional";
            }

            return trimmed;
        }
    }
}
