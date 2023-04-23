using System;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProjectManagementSystem.Controllers;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.DomainModels;
using ProjectManagementSystem.Models.ViewModels.AccountViewModels;
using ProjectManagementSystem.Services;

namespace ProjectManagementSystemUnitTests.ControllerTests
{
    public class TaskControllerTests
    {
        private readonly TaskController _controller;
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ITaskService> _taskServiceMock;

        public TaskControllerTests()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            _taskServiceMock = new Mock<ITaskService>();

            _controller = new TaskController(_taskServiceMock.Object, _context, _userManagerMock.Object);
        }


        [Fact]
        public void Index_ReturnsAViewResult_WithAListOfTaskViewModels()
        {
            // Arrange
            var tasks = new List<ProjectTask>
            {
                new ProjectTask
                {
                    Id = 1,
                    Name = "Task 1",
                    Status = status.New,
                    Deadline = DateTime.Now.AddDays(1),
                    Description = "Task 1 Description",
                    Progress = 0,
                    DeveloperId = null,
                    ManagerId = null,
                    IsDeveloperAssigned = false,
                    IsManagerAssigned = false,
                    AdminId = null,
                    ProjectId = 1
                },
                new ProjectTask
                {
                    Id = 2,
                    Name = "Task 2",
                    Status = status.InProgress,
                    Deadline = DateTime.Now.AddDays(2),
                    Description = "Task 2 Description",
                    Progress = 50,
                    DeveloperId = null,
                    ManagerId = null,
                    IsDeveloperAssigned = false,
                    IsManagerAssigned = false,
                    AdminId = null,
                    ProjectId = 1
                }
            };
            _context.Tasks.AddRange(tasks);
            _context.SaveChanges();

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<TaskViewModel>>(
                viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Edit_ReturnsViewResult_WhenUserIsAdmin()
        {
            // Arrange
            var task = new ProjectTask
            {
                Id = 3,
                Name = "Task 3",
                Status = status.New,
                Deadline = DateTime.Now.AddDays(1),
                Description = "Description for Task 3",
                Progress = 50,
                DeveloperId = "1",
                ManagerId = "2",
                IsDeveloperAssigned = true,
                IsManagerAssigned = true,
                ProjectId = 1
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            var taskViewModel = new TaskViewModel
            {
                Id = 3,
                Name = "Task 3 UPDATED",
                Status = status.New,
                Deadline = "12.04.2023",
                Description = "Description for Task 3 UPDATED",
                Progress = 50,
                DeveloperId = "1",
                ManagerId = "2",
                IsDeveloperAssigned = true,
                ProjectId = 1
            };

            var currentUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Jovan"),
                new Claim(ClaimTypes.Role, "Admin"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = currentUser
                }
            };

            // Act
            var result = _controller.Edit(taskViewModel.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<TaskViewModel>(viewResult.ViewData.Model);
            Assert.Equal(task.Name, model.Name);
            Assert.Equal(task.Description, model.Description);            
        }

        [Fact]
        public void Delete_ValidTask_RedirectToAction()
        {
            // Arrange
            var task = new ProjectTask
            {
                Id = 4,
                Name = "Task 4",
                Status = status.New,
                Deadline = DateTime.Now.AddDays(1),
                Description = "Description for Task 4",
                Progress = 50,
                DeveloperId = "1",
                ManagerId = "2",
                IsDeveloperAssigned = true,
                IsManagerAssigned = true,
                ProjectId = 1
            };
            _context.Tasks.Add(task);
            _context.SaveChanges();

            var project = new Project
            {
                Id = 1,
                Name = "Project 1",
                Progress = 30,
                ProjectManagerId = "1"
            };
            _context.Projects.Add(project);
            _context.SaveChanges();

            var taskViewModel = new TaskViewModel
            {
                Id = 4,
                Name = "Task 4",
                Status = status.New,
                Deadline = "12.04.2023",
                Description = "Description for Task 4",
                Progress = 50,
                DeveloperId = "1",
                ManagerId = "2",
                IsDeveloperAssigned = true,
                ProjectId = 1
            };

            // Act
            var result = _controller.Delete(taskViewModel);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectToActionResult = (RedirectToActionResult)result;
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var taskInDb = _context.Tasks.Find(taskViewModel.Id);
            Assert.Null(taskInDb);
        }

        [Fact]
        public void Delete_InvalidTask_ReturnNotFound()
        {
            // Act
            var result = _controller.Delete(new TaskViewModel { Id = 100 });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }        
    }
}

