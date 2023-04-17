using System;
namespace ProjectManagementSystem.Models.ViewModels
{
	public class ProjectViewModel
	{
		public string Name { get; set; }
		public IEnumerable<ManagerViewModel> ManagerList { get; set; }
	}
}

