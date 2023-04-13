using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
                    userViewModel.Role = roleName;
                }

                userList.Add(userViewModel);
            }

            return View(userList);
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
            userViewModel.Role = roleName;
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


