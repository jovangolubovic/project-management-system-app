using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectManagementSystem.Helper
{
    public static class Helper
    {
        public static string Admin = "Admin";
        public static string Developer = "Developer";
        public static string ProjectManager = "Project Manager";

        public static string New = "New";
        public static string InProgress = "In Progress";
        public static string Finished = "Finished";

        public static List<SelectListItem> GetRolesForDropDown()
        {
            return new List<SelectListItem>
            {
                new SelectListItem{Value=Helper.Admin, Text=Helper.Admin },
                new SelectListItem{Value=Helper.Developer, Text=Helper.Developer},
                new SelectListItem{Value=Helper.ProjectManager, Text=Helper.ProjectManager }

            };
        }

        public static List<SelectListItem> GetTaskStatusForDropdown()
        {
            return new List<SelectListItem>
            {
                new SelectListItem{Value=Helper.New, Text=Helper.New },
                new SelectListItem{Value=Helper.InProgress, Text=Helper.InProgress},
                new SelectListItem{Value=Helper.Finished, Text=Helper.Finished }

            };
        }
    }
}

