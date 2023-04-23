using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using ProjectManagementSystem.Controllers;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystemUnitTests.ControllerTests
{
	public class UserControllerTests
	{
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserControllerTests()
        {
            // Create a new instance of ApplicationDbContext for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _db = new ApplicationDbContext(options);
        }
   
        [Fact]
        public void DeletePost_ValidId_RemovesUserFromDatabaseAndRedirectsToIndex()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "1",
                Name = "Jovan",
                Surname = "Golubovic"
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            var userController = new UserController(_db, null, null);

            // Act
            var result = userController.DeletePost(user.Id) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);

            var deletedUser = _db.Users.Find(user.Id);
            Assert.Null(deletedUser);
        }

        [Fact]
        public void DeletePost_ReturnsNotFound_WhenIdIsNull()
        {
            // Arrange
            var controller = new UserController(_db, null, null);

            // Act
            var result = controller.DeletePost(null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}

