using ArchonPM.Objects;
using ArchonPM.Services;
using Xunit;

namespace ArchonPM.Tests
{
    public class ProjectServiceDeleteSearchTests
    {
        private static ProjectService CreateServiceWithProject(out Project project)
        {
            var service = new ProjectService();
            project = new Project
            {
                Name = "Alpha",
                Owner = "Ada",
                Description = "First project",
                Status = "Planning"
            };
            service.AddProject(project);
            return service;
        }

        [Fact]
        public void DeleteProject_ExistingProject_RemovesIt()
        {
            var service = CreateServiceWithProject(out Project project);

            Assert.True(service.DeleteProject(project.ID));
            Assert.Empty(service.GetAllProjects());
        }

        [Fact]
        public void GetProjectByName_MatchesIgnoringCase()
        {
            var service = CreateServiceWithProject(out Project project);

            Assert.Same(project, service.GetProjectByName("alpha"));
        }
    }
}
