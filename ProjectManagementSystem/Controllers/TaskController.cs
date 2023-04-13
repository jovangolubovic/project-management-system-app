using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using ProjectManagementSystem.Data;
using ProjectManagementSystem.Models.DomainModels;
using ProjectManagementSystem.Models.ViewModels.AccountViewModels;
using ProjectManagementSystem.Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProjectManagementSystem.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        ApplicationDbContext _db;

        public TaskController(ITaskService taskService, ApplicationDbContext db)
        {
            _db = db;
            _taskService = taskService;
        }

        //INDEX
        public IActionResult Index()
        {
            ViewBag.DeveloperList = _taskService.getDeveloperList();
            ViewBag.ManagersList = _taskService.getManagerList();

            List<TaskViewModel> tasksVM = new List<TaskViewModel>();
            IEnumerable<ProjectTask> objList = _db.Tasks;
            foreach (ProjectTask t in objList)
            {
                TaskViewModel taskvm = new TaskViewModel();
                taskvm.Name = t.Name;
                taskvm.Description = t.Description;
                taskvm.Id = t.Id;
                taskvm.Status = t.Status;
                taskvm.Progress = t.Progress;
                taskvm.ProjectId = t.ProjectId;
                taskvm.IsDeveloperAssigned = t.IsDeveloperAssigned;
                taskvm.Deadline = t.Deadline.ToString().Substring(0, t.Deadline.ToString().Length - 10);
                List<String> projectname = (from p in _db.Projects
                                            where p.Id == t.ProjectId
                                            select p.Name
                                               ).ToList();
                taskvm.ProjectName = projectname.First();
                var managername = (from m in _db.Users
                                   where m.Id == t.ManagerId
                                   select m.Name
                                ).ToList();
                var developername = (from d in _db.Users
                                     where d.Id == t.DeveloperId
                                     select d.Email
                                ).ToList();
                if (managername.Count != 0)
                {
                    taskvm.ManagerName = managername.First();
                }

                string developerId = t.DeveloperId;
                int developerTaks = (from task in _db.Tasks
                                     where developerId == task.DeveloperId
                                     select task.DeveloperId
                                     ).Count();
                if (developerTaks < 3)
                {
                    taskvm.DeveloperId = developerId;

                }

                if (developername.Count != 0)
                {
                    taskvm.DeveloperName = developername.First();
                }

                tasksVM.Add(taskvm);
            }

            return View(tasksVM);
        }

        //CREATE GET
        public IActionResult Create()
        {
            ViewBag.Status = _taskService.getStatusList();
            ViewBag.Managers = _taskService.getManagerList();
            ViewBag.Developers = _taskService.getDeveloperList();
            ViewBag.Projects = _taskService.getProjectList();

            return View();
        }
        //CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskViewModel obj)
        {


            ProjectTask task = new ProjectTask();
            task.Description = obj.Description;
            task.Name = obj.Name;
            string due = obj.Deadline;
            task.Deadline = DateTime.ParseExact(due, "dd.MM.yyyy", null);
            task.ManagerId = Request.Form["managersId"];
            string statusTask = Request.Form["status"];

            //validation for developers, not more than 3 tasks
            string developerId = Request.Form["developersId"];
            int developerTaks = (from t in _db.Tasks
                                 where developerId == t.DeveloperId
                                 select t.DeveloperId
                                 ).Count();

            if (obj.IsDeveloperAssigned)
            {
                if (developerTaks < 3)
                {
                    task.DeveloperId = developerId;
                    task.IsDeveloperAssigned = true;

                }
                else
                {
                    return Ok("Can not assign more than 3 tasks to one developer!");
                }
            }
            task.Status = status.New;
            task.Progress = 0;

            //add project to task
            string projectId = Request.Form["projectsId"];
            int projectID = int.Parse(projectId);
            var projects = (from proj in _db.Projects
                            where proj.Id == projectID
                            select proj
                               ).ToList();
            Project pro = projects.First();
            task.ProjectId = pro.Id;



            _db.Add(task);
            _db.SaveChanges();

            //update project progress
            updateProjectProgress(task);
            _db.SaveChanges();

            return RedirectToAction("Index");
        }


        //EDIT GET
        public IActionResult Edit(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }

            List<ProjectTask> tasksVM = new List<ProjectTask>();
            IEnumerable<ProjectTask> objList = _db.Tasks;
            ProjectTask t = _db.Tasks.Find(id);

            if (t == null)
            {
                return NotFound();
            }

            TaskViewModel taskvm = new TaskViewModel();
            taskvm.Name = t.Name;
            taskvm.Description = t.Description;
            taskvm.Id = t.Id;
            taskvm.Status = t.Status;
            taskvm.Progress = t.Progress;
            taskvm.ProjectId = t.ProjectId;
            var managername = (from m in _db.Users
                               where m.Id == t.ManagerId
                               select m.Name
                ).ToList();

            if (managername.Count != 0)
            {
                taskvm.ManagerName = managername.First();
            }
            ViewBag.Managers = _taskService.getManagerList();
            ViewBag.Developers = _taskService.getDeveloperList();
            ViewBag.Projects = _taskService.getProjectList();

            return View(taskvm);

        }
        //EDIT POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TaskViewModel obj)
        {

            ProjectTask t = _db.Tasks.Find(obj.Id);

            var managerId = Request.Form["managersId"];
            string developerId = Request.Form["developersId"];

            //Developer evaluation
            int developerTaks = (from tasks in _db.Tasks
                                 where developerId == tasks.DeveloperId
                                 select tasks.DeveloperId
                                 ).Count();

            if (obj.IsDeveloperAssigned)
            {
                if (developerTaks < 3)
                {
                    t.DeveloperId = developerId;
                    t.IsDeveloperAssigned = true;

                }
                else
                {
                    return Ok("Can not assign more than 3 tasks to one developer!");
                }
            }

            //if Admin unasssigns developer 
            if (obj.IsDeveloperAssigned == false && obj.Name != null && obj.Deadline != null)
            {
                t.DeveloperId = null;
                t.IsDeveloperAssigned = false;
                t.Deadline = DateTime.ParseExact(obj.Deadline, "dd.MM.yyyy", null);

            }
            //if Manager unasssigns developer 
            if (obj.IsDeveloperAssigned == false && obj.Name == null && obj.Deadline != null)
            {
                t.DeveloperId = null;
                t.IsDeveloperAssigned = false;
                try
                {
                    t.Deadline = DateTime.ParseExact(obj.Deadline, "dd.MM.yyyy", null);
                }
                catch
                {
                    new Exception("Invalid date format");
                    return Ok("Invalid date format, go back and enter valid date format: DD.MM.YYYY");
                }

            }




            //In case developer is assigned, name and manager will return null,
            //so they stay unchanged.
            if (obj.Name != null)
            {
                t.Name = obj.Name;
                t.ManagerId = managerId;
                t.Deadline = DateTime.ParseExact(obj.Deadline, "dd.MM.yyyy", null);


            }
            t.Description = obj.Description;
            t.Progress = obj.Progress;
            if (t.Progress == 100)
            {
                t.Status = status.Finished;
            }
            if (t.Progress < 100 && t.Progress > 0)
            {
                t.Status = status.InProgress;

            }
            updateProjectProgress(t);
            _db.Update(t);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            var obj = _db.Tasks.Find(id);
            if (obj == null)
            {
                return NotFound();
            }

            return View(obj);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var obj = _db.Tasks.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Tasks.Remove(obj);
            _db.SaveChanges();
            updateProjectProgress(obj);
            return RedirectToAction("Index");
        }
        public void updateProjectProgress(ProjectTask t)
        {
            int taskscount = (from tasks in _db.Tasks
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
            int progressProject = sum / taskscount;
            Project project = (from p in _db.Projects
                               where p.Id == t.ProjectId
                               select p
                         ).First();
            project.Progress = progressProject;
            _db.Update(project);
            _db.SaveChanges();
        }
    }

}

