using System;
using System.Collections.Generic;
using System.Linq;
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

            var userId = _userManager.GetUserId(User);
            var nameOfUser = (from u in _db.Users
                              where u.Id == userId
                              select u.Name + " " + u.Surname).ToList().First();

            ViewBag.NameOfUser = nameOfUser;

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
                taskViewModel.IsDeveloperAssigned = t.IsDeveloperAssigned;
               
                List<String> projectName = (from p in _db.Projects
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

                string developerId = t.DeveloperId;

                int developerTask = (from task in _db.Tasks
                                     where developerId == task.DeveloperId
                                     select task.DeveloperId
                                    ).Count();

                if (developerTask < 3)
                {
                    taskViewModel.DeveloperId = developerId;
                }

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
            
            string statusTask = Request.Form["status"];

            // Validation for developers, not more than 3 tasks
            string developerId = Request.Form["developerId"];
            int developerTask = (from t in _db.Tasks
                                 where developerId == t.DeveloperId
                                 select t.DeveloperId
                                ).Count();

            if (taskViewModel.IsDeveloperAssigned)
            {
                if (developerTask < 3)
                {
                    task.DeveloperId = developerId;
                    task.IsDeveloperAssigned = true;
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot assign more than 3 Tasks to one Developer!";
                    return RedirectToAction("Create"); 
                }

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

            //update project progress
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

            return View(taskViewModel);
        }

        // POST:
        // Updating Existing Task and Saving to Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TaskViewModel taskViewModel)
        {

            ProjectTask t = _db.Tasks.Find(taskViewModel.Id);

            var managerId = Request.Form["managerId"];
            string developerId = Request.Form["developerId"];

            //Developer evaluation
            int developerTask = (from tasks in _db.Tasks
                                 where developerId == tasks.DeveloperId
                                 select tasks.DeveloperId
                                ).Count();

            if (taskViewModel.IsDeveloperAssigned)
            {
                if (developerTask < 3)
                {
                    t.DeveloperId = developerId;
                    t.IsDeveloperAssigned = true;
                }
                else
                {
                    TempData["ErrorMessage"] = "Cannot assign more than 3 Tasks to one Developer!";
                    return RedirectToAction("Edit");
                }
            }

            // If User unassigns Developer from Task 
            if (taskViewModel.IsDeveloperAssigned == false && taskViewModel.Name != null && taskViewModel.Deadline != null)
            {
                t.DeveloperId = null;
                t.IsDeveloperAssigned = false;
                t.Deadline = DateTime.ParseExact(taskViewModel.Deadline, "dd.MM.yyyy", null);
            }

            //if Manager unassigns developer from Task
            if (taskViewModel.IsDeveloperAssigned == false && taskViewModel.Name == null && taskViewModel.Deadline != null)
            {
                t.DeveloperId = null;
                t.IsDeveloperAssigned = false;
                try
                {
                    t.Deadline = DateTime.ParseExact(taskViewModel.Deadline, "dd.MM.yyyy", null);
                }
                catch
                {
                    new Exception("Invalid Date format");
                    return Ok("Invalid Date format, go back and enter valid Date format: DD.MM.YYYY");
                }
            }

            //In case developer is assigned, name and manager will return null,
            //so they stay unchanged.
            if (taskViewModel.Name != null)
            {
                t.Name = taskViewModel.Name;
                t.ManagerId = managerId;
                t.Deadline = DateTime.ParseExact(taskViewModel.Deadline, "dd.MM.yyyy", null);
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
            if(t.Progress == 0)
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
            List<String> projectName = (from p in _db.Projects
                                        where p.Id == task.ProjectId
                                        select p.Name
                                       ).ToList();

            taskViewModel.ProjectName = projectName.First();

            var developerName = (from d in _db.Users
                                     where d.Id == task.DeveloperId
                                     select d.Name + " " + d.Surname
                                    ).ToList();

            if(developerName != null)
            {
                taskViewModel.DeveloperName = developerName.FirstOrDefault();
            }            

            var managerName = (from m in _db.Users
                               where m.Id == task.ManagerId
                               select m.Name + " " + m.Surname
                              ).ToList();

            taskViewModel.ManagerName = managerName.First();

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

