using System.Collections.Generic;
using ArchonPM.Objects;

namespace ArchonPM.Services
{
    public interface IProjectService
    {
        IReadOnlyList<Project> GetAllProjects();
        Project? GetProjectById(int projectId);
        Project? GetProjectByName(string name);
        void AddProject(Project project);
        bool DeleteProject(int projectId);
        void UpdateProject(int projectId, string name, string description, string owner, string status, int actorMemberId);

        IReadOnlyList<Staff> GetProjectMembers(int projectId);
        void AddProjectMember(int projectId, string name, string? email, ProjectRole role, int actorMemberId);
        void UpdateMemberRole(int projectId, int memberId, ProjectRole newRole, int actorMemberId);
        void RemoveProjectMember(int projectId, int memberId, int actorMemberId);

        IReadOnlyList<Requirement> GetRequirements(int projectId);
        void AddRequirement(int projectId, string name, string description, string category, int actorMemberId);
        void UpdateRequirement(int projectId, int requirementId, string name, string description, string category, int actorMemberId);
    }
}
