using System;
using System.Collections.Generic;
using System.Linq;
using ArchonPM.Objects;
using ArchonPM.Services;
using Xunit;

namespace ArchonPM.Tests
{
    public class ProjectServiceTests
    {
        private static ProjectService CreateServiceWithProject(out Project project, out int primaryId)
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
            primaryId = service.GetProjectMembers(project.ID).Single(m => m.Role == ProjectRole.PrimaryAdmin).ID;
            return service;
        }

        [Fact]
        public void AddProject_ValidProject_IsReturnedByGetAllProjects()
        {
            var service = CreateServiceWithProject(out Project project, out _);

            IReadOnlyList<Project> projects = service.GetAllProjects();
            Assert.Single(projects);
            Assert.Same(project, projects[0]);
            Assert.True(project.ID > 0);
        }

        [Fact]
        public void GetProjectById_ReturnsMatchingProject()
        {
            var service = CreateServiceWithProject(out Project project, out _);
            Project? found = service.GetProjectById(project.ID);
            Assert.Same(project, found);
            Assert.Null(service.GetProjectById(999));
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
            var service = CreateServiceWithProject(out _, out _);
            IReadOnlyList<Project> projects = service.GetAllProjects();
            Assert.Throws<NotSupportedException>(() => ((IList<Project>)projects).Add(new Project
            {
                Name = "Hack",
                Owner = "X",
                Description = "",
                Status = "Planning"
            }));
        }

        [Fact]
        public void AddProject_CreatesPrimaryAdminFromOwner()
        {
            var service = CreateServiceWithProject(out Project project, out int primaryId);
            Staff primary = service.GetProjectMembers(project.ID).Single(m => m.ID == primaryId);
            Assert.Equal("Ada", primary.Name);
            Assert.Equal(ProjectRole.PrimaryAdmin, primary.Role);
            Assert.Single(service.GetProjectMembers(project.ID).Where(m => m.Role == ProjectRole.PrimaryAdmin));
        }

        [Fact]
        public void UpdateProject_PreservesIdMembersAndRequirements()
        {
            var service = CreateServiceWithProject(out Project project, out int primaryId);
            service.AddRequirement(project.ID, "Login", "Users can log in", "Functional", primaryId);
            service.AddProjectMember(project.ID, "Bob", "bob@example.com", ProjectRole.SoftwareEngineer, primaryId);

            int originalId = project.ID;
            service.UpdateProject(project.ID, "Alpha Renamed", "Updated", "Ada", "In Progress", primaryId);

            Project? updated = service.GetProjectById(originalId);
            Assert.NotNull(updated);
            Assert.Equal(originalId, updated!.ID);
            Assert.Equal("Alpha Renamed", updated.Name);
            Assert.Equal("In Progress", updated.Status);
            Assert.Single(service.GetRequirements(originalId));
            Assert.Equal(2, service.GetProjectMembers(originalId).Count);
        }

        [Fact]
        public void UpdateProject_UnauthorizedRole_Throws()
        {
            var service = CreateServiceWithProject(out Project project, out int primaryId);
            service.AddProjectMember(project.ID, "Vie", null, ProjectRole.Viewer, primaryId);
            int viewerId = service.GetProjectMembers(project.ID).Single(m => m.Role == ProjectRole.Viewer).ID;

            Assert.Throws<UnauthorizedAccessException>(() =>
                service.UpdateProject(project.ID, "X", "Y", "Ada", "Planning", viewerId));
        }

        [Fact]
        public void Members_AddAndRejectDuplicateEmail()
        {
            var service = CreateServiceWithProject(out Project project, out int primaryId);
            service.AddProjectMember(project.ID, "Bob", "bob@example.com", ProjectRole.SoftwareEngineer, primaryId);

            Assert.Throws<InvalidOperationException>(() =>
                service.AddProjectMember(project.ID, "Robert", "bob@example.com", ProjectRole.Viewer, primaryId));
        }

        [Fact]
        public void Members_RoleUpdateAndUnauthorizedAssignPrimary()
        {
            var service = CreateServiceWithProject(out Project project, out int primaryId);
            service.AddProjectMember(project.ID, "Sam", null, ProjectRole.SoftwareEngineer, primaryId);
            int engineerId = service.GetProjectMembers(project.ID).Single(m => m.Name == "Sam").ID;

            service.UpdateMemberRole(project.ID, engineerId, ProjectRole.ProjectManager, primaryId);
            Assert.Equal(ProjectRole.ProjectManager,
                service.GetProjectMembers(project.ID).Single(m => m.ID == engineerId).Role);

            Assert.Throws<InvalidOperationException>(() =>
                service.UpdateMemberRole(project.ID, engineerId, ProjectRole.PrimaryAdmin, primaryId));
        }

        [Fact]
        public void Members_SecondaryAdminCannotAssignPrimaryOrSecondary()
        {
            var service = CreateServiceWithProject(out Project project, out int primaryId);
            service.AddProjectMember(project.ID, "Sec", null, ProjectRole.SecondaryAdmin, primaryId);
            service.AddProjectMember(project.ID, "Eng", null, ProjectRole.SoftwareEngineer, primaryId);
            int secondaryId = service.GetProjectMembers(project.ID).Single(m => m.Name == "Sec").ID;
            int engineerId = service.GetProjectMembers(project.ID).Single(m => m.Name == "Eng").ID;

            Assert.Throws<UnauthorizedAccessException>(() =>
                service.AddProjectMember(project.ID, "Other", null, ProjectRole.SecondaryAdmin, secondaryId));

            Assert.Throws<InvalidOperationException>(() =>
                service.UpdateMemberRole(project.ID, engineerId, ProjectRole.PrimaryAdmin, secondaryId));
        }

        [Fact]
        public void Members_CannotRemovePrimaryAdminDirectly()
        {
            var service = CreateServiceWithProject(out Project project, out int primaryId);
            Assert.Throws<UnauthorizedAccessException>(() =>
                service.RemoveProjectMember(project.ID, primaryId, primaryId));
        }

        [Fact]
        public void Members_RemoveOnlyAffectsSelectedProject()
        {
            var service = new ProjectService();
            var first = new Project { Name = "One", Owner = "Ada", Description = "", Status = "Planning" };
            var second = new Project { Name = "Two", Owner = "Ada", Description = "", Status = "Planning" };
            service.AddProject(first);
            service.AddProject(second);

            int primary1 = service.GetProjectMembers(first.ID).Single(m => m.Role == ProjectRole.PrimaryAdmin).ID;
            service.AddProjectMember(first.ID, "Bob", "bob@example.com", ProjectRole.Viewer, primary1);
            int bobId = service.GetProjectMembers(first.ID).Single(m => m.Name == "Bob").ID;

            int primary2 = service.GetProjectMembers(second.ID).Single(m => m.Role == ProjectRole.PrimaryAdmin).ID;
            service.AddProjectMember(second.ID, "Bob", "bob@example.com", ProjectRole.Viewer, primary2);

            service.RemoveProjectMember(first.ID, bobId, primary1);

            Assert.DoesNotContain(service.GetProjectMembers(first.ID), m => m.Name == "Bob");
            Assert.Contains(service.GetProjectMembers(second.ID), m => m.Name == "Bob");
        }

        [Fact]
        public void Requirements_AddAndUpdatePreserveId()
        {
            var service = CreateServiceWithProject(out Project project, out int primaryId);
            service.AddRequirement(project.ID, "Auth", "Login flow", "Functional", primaryId);
            Requirement req = service.GetRequirements(project.ID).Single();
            int reqId = req.ID;

            service.UpdateRequirement(project.ID, reqId, "Auth v2", "Updated", "Non-Functional", primaryId);
            Requirement updated = service.GetRequirements(project.ID).Single();
            Assert.Equal(reqId, updated.ID);
            Assert.Equal("Auth v2", updated.Name);
            Assert.Equal("Non-Functional", updated.Catagory);
        }

        [Fact]
        public void Requirements_UnauthorizedAndInvalidRejected()
        {
            var service = CreateServiceWithProject(out Project project, out int primaryId);
            service.AddProjectMember(project.ID, "Vie", null, ProjectRole.Viewer, primaryId);
            int viewerId = service.GetProjectMembers(project.ID).Single(m => m.Role == ProjectRole.Viewer).ID;

            Assert.Throws<UnauthorizedAccessException>(() =>
                service.AddRequirement(project.ID, "X", "Y", "Functional", viewerId));

            Assert.Throws<ArgumentException>(() =>
                service.AddRequirement(project.ID, "", "Y", "Functional", primaryId));
        }

        [Fact]
        public void Requirements_DoNotAffectOtherProjects()
        {
            var service = new ProjectService();
            var first = new Project { Name = "One", Owner = "Ada", Description = "", Status = "Planning" };
            var second = new Project { Name = "Two", Owner = "Ada", Description = "", Status = "Planning" };
            service.AddProject(first);
            service.AddProject(second);
            int primary1 = service.GetProjectMembers(first.ID).Single(m => m.Role == ProjectRole.PrimaryAdmin).ID;

            service.AddRequirement(first.ID, "Only First", "Desc", "Functional", primary1);

            Assert.Single(service.GetRequirements(first.ID));
            Assert.Empty(service.GetRequirements(second.ID));
        }

        [Fact]
        public void MissingProjectId_ThrowsKeyNotFound()
        {
            var service = new ProjectService();
            Assert.Throws<KeyNotFoundException>(() => service.GetRequirements(42));
        }
    }
}
