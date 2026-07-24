using System.Collections.Generic;
using ArchonPM.Objects;

namespace ArchonPM.Services
{
    public interface IProjectService
    {
        IReadOnlyList<Project> GetAllProjects();
        void AddProject(Project project);
    }
}
