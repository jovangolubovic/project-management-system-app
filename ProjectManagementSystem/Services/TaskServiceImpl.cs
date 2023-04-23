using System;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models.DomainModels;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Services
{
    public class TaskServiceImpl : ITaskService
    {
        private readonly ApplicationDbContext _db;
        public TaskServiceImpl(ApplicationDbContext db)
        {
            _db = db;
        }

        //Returning All Developers
        public List<DeveloperViewModel> getDeveloperList()
        {
            var developers = (from user in _db.Users
                              join userRoles in _db.UserRoles
                              on user.Id equals userRoles.UserId
                              join roles in _db.Roles.Where(x => x.Name == Helper.Helper.Developer)
                              on userRoles.RoleId equals roles.Id
                              select new DeveloperViewModel
                              {
                                  Id = user.Id,
                                  Name = user.Name,
                                  Surname = user.Surname
                              }
                              ).ToList();

            return developers;
        }

        //Returning All Managers
        public List<ManagerViewModel> getManagerList()
        {
            var managers = (from user in _db.Users
                            join userRoles in _db.UserRoles
                            on user.Id equals userRoles.UserId
                            join roles in _db.Roles.Where(x => x.Name == Helper.Helper.ProjectManager)
                            on userRoles.RoleId equals roles.Id
                            select new ManagerViewModel
                            {
                                Id = user.Id,
                                Name = user.Name,
                                Surname = user.Surname                            
                            }
                            ).ToList();

            return managers;
        }

        //Returning All Users Expect Admin
        public List<UserViewModel> getUserList()
        {
            var users = (from user in _db.Users
                         join userRoles in _db.UserRoles
                         on user.Id equals userRoles.UserId
                         join roles in _db.Roles
                         on userRoles.RoleId equals roles.Id
                         select new UserViewModel
                         {
                             Id = user.Id,
                             Name = user.Name,
                             Surname = user.Surname,
                             RoleName = roles.Name
                         }
                        ).ToList();

            return users;
        }

        //Returning All Projects
        public List<Project> getProjectList()
        {
            var projects = (from project in _db.Projects
                            select new Project
                            {
                                Id = project.Id,
                                Name = project.Name
                            }
                            ).ToList();

            return projects;
        }

        //Returning List with Statuses
        public List<string> getStatusList()
        {
            List<string> listStatus = new List<string>();
            listStatus.Add(Helper.Helper.New);
            listStatus.Add(Helper.Helper.InProgress);
            listStatus.Add(Helper.Helper.Finished);
            return listStatus;
        }
    }
}

