using System;
using System.Collections.Generic;
using ArchonPM.Objects;
using ArchonPM.Services;
using Xunit;

namespace ArchonPM.Tests
{
    public class ProjectServiceTests
    {
        [Fact]
        public void AddProject_ValidProject_IsReturnedByGetAllProjects()
        {
            var service = new ProjectService();
            var project = new Project
            {
                Name = "Alpha",
                Owner = "Ada",
                Description = "First project",
                Status = "Planning"
            };

            service.AddProject(project);

            IReadOnlyList<Project> projects = service.GetAllProjects();
            Assert.Single(projects);
            Assert.Same(project, projects[0]);
        }

        [Fact]
        public void AddProject_MultipleProjects_PreservesInsertionOrder()
        {
            var service = new ProjectService();
            var first = new Project { Name = "First", Owner = "A", Description = "", Status = "Planning" };
            var second = new Project { Name = "Second", Owner = "B", Description = "", Status = "In Progress" };

            service.AddProject(first);
            service.AddProject(second);

            IReadOnlyList<Project> projects = service.GetAllProjects();
            Assert.Equal(2, projects.Count);
            Assert.Same(first, projects[0]);
            Assert.Same(second, projects[1]);
        }

        [Fact]
        public void AddProject_Null_ThrowsArgumentNullException()
        {
            var service = new ProjectService();
            Assert.Throws<ArgumentNullException>(() => service.AddProject(null!));
        }

        [Fact]
        public void GetAllProjects_ReturnedCollection_IsNotDirectlyMutable()
        {
            var service = new ProjectService();
            service.AddProject(new Project
            {
                Name = "Alpha",
                Owner = "Ada",
                Description = "",
                Status = "Planning"
            });

            IReadOnlyList<Project> projects = service.GetAllProjects();
            Assert.IsAssignableFrom<IReadOnlyList<Project>>(projects);
            Assert.Throws<NotSupportedException>(() => ((IList<Project>)projects).Add(new Project
            {
                Name = "Hack",
                Owner = "X",
                Description = "",
                Status = "Planning"
            }));
            Assert.Single(service.GetAllProjects());
        }
    }
}
