using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.DomainModels;
using ProjectManagementSystem.Models.ViewModels.AccountViewModels;
using ProjectManagementSystem.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectManagementSystem.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _iTaskService;
        private readonly UserManager<ApplicationUser> _userManager;
        ApplicationDbContext _db;

        public TaskController(ITaskService taskService, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _iTaskService = taskService;
            _userManager = userManager;
        }

        // GET: 
        // Returning View with List of Tasks
        public IActionResult Index()
        {
            ViewBag.DeveloperList = _iTaskService.getDeveloperList();
            ViewBag.ManagerList = _iTaskService.getManagerList();

            List<TaskViewModel> taskViewModelList = new List<TaskViewModel>();
            List<ProjectTask> taskList = _db.Tasks.ToList();
            foreach (ProjectTask t in taskList)
            {
                TaskViewModel taskViewModel = new TaskViewModel();

                taskViewModel.Id = t.Id;
                taskViewModel.Name = t.Name;
                taskViewModel.Status = t.Status;
                taskViewModel.Deadline = t.Deadline.ToString().Substring(0, t.Deadline.ToString().Length - 10);
                taskViewModel.Description = t.Description;
                taskViewModel.Progress = t.Progress;
                taskViewModel.ProjectId = t.ProjectId;
                taskViewModel.ManagerId = t.ManagerId;
                taskViewModel.DeveloperId = t.DeveloperId;

                var projectName = (from p in _db.Projects
                                   where p.Id == t.ProjectId
                                   select p.Name
                                   ).ToList();

                taskViewModel.ProjectName = projectName.First();

                var managerName = (from m in _db.Users
                                   where m.Id == t.ManagerId
                                   select m.Name + " " + m.Surname
                                  ).ToList();

                var managerUserName = (from m in _db.Users
                                       where m.Id == t.ManagerId
                                       select m.Email
                                      ).ToList();

                if (managerName.Count != 0)
                {
                    taskViewModel.ManagerName = managerName.First();
                }

                if (managerUserName.Count != 0)
                {
                    taskViewModel.ManagerUserName = managerUserName.First();
                }

                var developerName = (from d in _db.Users
                                     where d.Id == t.DeveloperId
                                     select d.Name + " " + d.Surname
                                    ).ToList();

                var developerUserName = (from d in _db.Users
                                         where d.Id == t.DeveloperId
                                         select d.Email
                                        ).ToList();

                if (developerName.Count != 0)
                {
                    taskViewModel.DeveloperName = developerName.First();
                }

                if (developerUserName.Count != 0)
                {
                    taskViewModel.DeveloperUserName = developerUserName.First();
                }

                taskViewModelList.Add(taskViewModel);
            }

            return View(taskViewModelList);
        }

        // GET: 
        // Returning View for Creation of Task
        public IActionResult Create()
        {
            ViewBag.Status = _iTaskService.getStatusList();
            ViewBag.ManagerList = _iTaskService.getManagerList();
            ViewBag.DeveloperList = _iTaskService.getDeveloperList();
            ViewBag.ProjectList = _iTaskService.getProjectList();

            return View();
        }

        // POST:
        // Creating New Task and adding it to database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskViewModel taskViewModel)
        {
            ProjectTask task = new ProjectTask();

            task.Name = taskViewModel.Name;
            task.Description = taskViewModel.Description;
            task.AdminId = _userManager.GetUserId(User);
            string deadline = taskViewModel.Deadline;
            task.Deadline = DateTime.ParseExact(deadline, "dd.MM.yyyy", null);
            task.ManagerId = Request.Form["managerId"];
            task.IsManagerAssigned = true;
            task.IsDeveloperAssigned = true;

            if (string.IsNullOrEmpty(task.ManagerId))
            {
                task.ManagerId = null;
                task.IsManagerAssigned = false;
            }

            string statusTask = Request.Form["status"];

            // Not more than 3 Tasks per Developer validation
            task.DeveloperId = Request.Form["developerId"];

            if (string.IsNullOrEmpty(task.DeveloperId))
            {
                task.DeveloperId = null;
                task.IsDeveloperAssigned = false;
            }

            int developerTask = (from t in _db.Tasks
                                 where task.DeveloperId == t.DeveloperId
                                 select t.DeveloperId
                                ).Count();

            if (developerTask > 3 && task.DeveloperId != null)
            {
                TempData["ErrorMessageForDeveloperTasks"] = "Cannot assign more than 3 Tasks to one Developer!";
                return RedirectToAction("Create");
            }

            task.Status = status.New;
            task.Progress = 0;

            // Add Project to Task
            string projectId = Request.Form["projectId"];
            int pId = int.Parse(projectId);
            var projects = (from p in _db.Projects
                            where p.Id == pId
                            select p
                           ).ToList();

            Project project = projects.First();
            task.ProjectId = project.Id;

            _db.Add(task);
            _db.SaveChanges();

            // Update project progress
            updateProjectProgress(task);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET:
        // Returning View for Editing of an existing Task
        public IActionResult Edit(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }

            List<ProjectTask> taskViewModelList = new List<ProjectTask>();
            IEnumerable<ProjectTask> taskList = _db.Tasks;
            ProjectTask t = _db.Tasks.Find(id);

            if (t == null)
            {
                return NotFound();
            }

            TaskViewModel taskViewModel = new TaskViewModel();

            taskViewModel.Id = t.Id;
            taskViewModel.Name = t.Name;
            taskViewModel.Status = t.Status;
            taskViewModel.Deadline = t.Deadline.ToString().Substring(0, t.Deadline.ToString().Length - 10);//
            taskViewModel.Description = t.Description;
            taskViewModel.Progress = t.Progress;
            taskViewModel.ProjectId = t.ProjectId;

            var managerName = (from m in _db.Users
                               where m.Id == t.ManagerId
                               select m.Name
                              ).ToList();

            if (managerName.Count != 0)
            {
                taskViewModel.ManagerName = managerName.First();
            }

            ViewBag.ManagerList = _iTaskService.getManagerList();
            ViewBag.DeveloperList = _iTaskService.getDeveloperList();
            ViewBag.ProjectList = _iTaskService.getProjectList();
            ViewBag.ProjectManagerId = t.ManagerId;
            ViewBag.DeveloperId = t.DeveloperId;

            return View(taskViewModel);
        }

        // POST:
        // Updating Existing Task and Saving to Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TaskViewModel taskViewModel)
        {
            var currentUser = HttpContext.User;
            var roleName = currentUser.FindFirst(ClaimTypes.Role)?.Value;

            ProjectTask t = _db.Tasks.Find(taskViewModel.Id);

            if (roleName == "Admin")
            {
                if (!string.IsNullOrEmpty(Request.Form["managerId"]))
                {
                    t.ManagerId = Request.Form["managerId"];
                    t.IsManagerAssigned = true;
                }
                else
                {
                    t.ManagerId = null;
                    t.IsManagerAssigned = false;
                }

                if (!string.IsNullOrEmpty(Request.Form["developerId"]))
                {
                    t.DeveloperId = Request.Form["developerId"];
                    t.IsDeveloperAssigned = true;
                }
                else
                {
                    t.DeveloperId = null;
                    t.IsDeveloperAssigned = false;
                }
            }

            if (roleName == "Project Manager")
            {

                if (!string.IsNullOrEmpty(Request.Form["developerId"]))
                {
                    t.DeveloperId = Request.Form["developerId"];
                    t.IsDeveloperAssigned = true;
                }
                else
                {
                    t.DeveloperId = null;
                    t.IsDeveloperAssigned = false;
                }
            }

            // Not more than 3 Tasks per Developer validation
            int developerTask = (from tasks in _db.Tasks
                                 where t.DeveloperId == tasks.DeveloperId
                                 select tasks.DeveloperId
                                ).Count();

            if (developerTask > 3 && t.DeveloperId != null)
            {
                TempData["ErrorMessageForDeveloperTasks"] = "Cannot assign more than 3 Tasks to one Developer!";
                return RedirectToAction("Edit");
            }

            t.Description = taskViewModel.Description;
            t.Progress = taskViewModel.Progress;

            if (t.Progress == 100)
            {
                t.Status = status.Finished;
            }
            if (t.Progress < 100 && t.Progress > 0)
            {
                t.Status = status.InProgress;
            }
            if (t.Progress == 0)
            {
                t.Status = status.New;
            }

            updateProjectProgress(t);
            _db.Update(t);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET:
        // Returning View for Deleting a Task
        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var task = _db.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            TaskViewModel taskViewModel = new TaskViewModel();
            taskViewModel.Id = task.Id;
            taskViewModel.Name = task.Name;
            var projectName = (from p in _db.Projects
                               where p.Id == task.ProjectId
                               select p.Name
                              ).ToList();

            taskViewModel.ProjectName = projectName.First();

            var developerName = (from d in _db.Users
                                 where d.Id == task.DeveloperId
                                 select d.Name + " " + d.Surname
                                ).ToList();

            if (developerName != null)
            {
                taskViewModel.DeveloperName = developerName.FirstOrDefault();
            }

            var managerName = (from m in _db.Users
                               where m.Id == task.ManagerId
                               select m.Name + " " + m.Surname
                              ).ToList();

            if (managerName != null)
            {
                taskViewModel.ManagerName = managerName.FirstOrDefault();
            }

            return View(taskViewModel);
        }

        // POST:
        // Deleting Task from Database by Task's Id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var task = _db.Tasks.Find(id);
            if (task == null)
            {
                return NotFound();
            }

            _db.Tasks.Remove(task);
            _db.SaveChanges();
            updateProjectProgress(task);
            return RedirectToAction("Index");
        }

        // Method for Updating progress of Project, when new Tasks are added
        public void updateProjectProgress(ProjectTask t)
        {
            int tasksCount = (from tasks in _db.Tasks
                              where tasks.ProjectId == t.ProjectId
                              select tasks.Id
                             ).Count();
            int sum = 0;
            foreach (ProjectTask task in _db.Tasks)
            {
                if (task.ProjectId == t.ProjectId)
                {
                    sum += task.Progress;
                }
            }

            Project project = (from p in _db.Projects
                               where p.Id == t.ProjectId
                               select p
                              ).First();
            if (sum != 0)
            {
                int progressOfProject = sum / tasksCount;
                project.Progress = progressOfProject;
            }
            else
            {
                project.Progress = 0;
            }

            _db.Update(project);
            _db.SaveChanges();
        }
    }
}

