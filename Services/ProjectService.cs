using System;
using System.Collections.Generic;
using ArchonPM.Objects;

namespace ArchonPM.Services
{
    public class ProjectService : IProjectService
    {
        private readonly List<Project> _projects = new();

        public IReadOnlyList<Project> GetAllProjects()
        {
            return _projects.AsReadOnly();
        }

        public void AddProject(Project project)
        {
            ArgumentNullException.ThrowIfNull(project);
            _projects.Add(project);
        }
    }
}
