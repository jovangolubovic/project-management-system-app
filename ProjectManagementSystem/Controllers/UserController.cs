using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectManagementSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET:
        // Returning View with a List of Users
        public IActionResult Index()
        {
            var users = (from u in _db.Users
                         select u
                         ).ToList();

            List<UserViewModel> userList = new List<UserViewModel>();

            foreach (ApplicationUser au in users)
            {
                var userRoles = _db.UserRoles.ToList();
                UserViewModel userViewModel = new UserViewModel();
                userViewModel.Id = au.Id;
                userViewModel.Name = au.Name;
                userViewModel.Surname = au.Surname;
                var roleId = (from ur in _db.UserRoles
                              where ur.UserId == au.Id
                              select ur.RoleId
                              ).FirstOrDefault();

                if (roleId != null)
                {
                    var roleName = (from r in _db.Roles
                                    where r.Id == roleId
                                    select r.Name
                                    ).First().ToString();
                    userViewModel.RoleName = roleName;
                }

                userList.Add(userViewModel);
            }

            return View(userList);
        }

        // GET:
        // EDIT VIEW
        public IActionResult Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = _db.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            var userRoles = _db.UserRoles.ToList();
            var roleId = (from ur in _db.UserRoles
                          where ur.UserId == user.Id
                          select ur.RoleId
                         ).FirstOrDefault();

            var roleName = (from r in _db.Roles
                            where r.Id == roleId
                            select r.Name
                           ).First().ToString();

            UserViewModel userViewModel = new UserViewModel();
            userViewModel.Name = user.Name;
            userViewModel.Surname = user.Surname;
            userViewModel.RoleName = roleName;

            return View(userViewModel);
        }

        //POST:
        //Update edited user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPost(string? id)
        {
            var user = _db.Users.Find(id);

            if (user == null)
            {
                // If the user doesn't exist, return a not found error
                return NotFound();
            }

            // Get the role name from the form data
            string roleName = Request.Form["RoleName"][0];

            if (string.IsNullOrEmpty(roleName))
            {
                // If the role name is not provided, return a bad request error
                return BadRequest();
            }

            // Find the role in the database
            var role = _db.Roles.SingleOrDefault(r => r.Name == roleName);

            if (role == null)
            {
                // If the role doesn't exist, return a not found error
                return NotFound();
            }

            // Find the user's current role
            var userRole = _db.UserRoles
                .SingleOrDefault(ur => ur.UserId == id);

            
                // Update the user's role
                userRole.RoleId = role.Id;
            

            // Save changes to the database
             _db.SaveChanges();

            // Redirect to the user details page
            return RedirectToAction("User", new { id = id });
        }

        // GET:
        // Returning View for Deleting User
        public IActionResult Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = _db.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }

            var userRoles = _db.UserRoles.ToList();
            var roleId = (from ur in _db.UserRoles
                          where ur.UserId == user.Id
                          select ur.RoleId
                         ).FirstOrDefault();

            var roleName = (from r in _db.Roles
                            where r.Id == roleId
                            select r.Name
                           ).First().ToString();

            UserViewModel userViewModel = new UserViewModel();
            userViewModel.Name = user.Name;
            userViewModel.Surname = user.Surname;
            userViewModel.RoleName = roleName;
            return View(userViewModel);
        }

        // POST:
        // Deleting User from Database by User's Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(string? id)
        {
            var user = _db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            _db.Users.Remove(user);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}


