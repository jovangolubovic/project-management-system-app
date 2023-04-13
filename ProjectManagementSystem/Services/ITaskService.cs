using System;
using ProjectManagementSystem.Models.DomainModels;
using ProjectManagementSystem.Models.ViewModels;

namespace ProjectManagementSystem.Services
{
	public interface ITaskService
	{
        public List<ManagerViewModel> getManagerList();
        public List<DeveloperViewModel> getDeveloperList();
        public List<UserViewModel> getUserList();
        public List<Project> getProjectList();
        public List<String> getStatusList();
    }
}

