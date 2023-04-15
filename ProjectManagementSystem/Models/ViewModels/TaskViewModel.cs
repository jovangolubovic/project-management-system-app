using System;
using ProjectManagementSystem.Models.DomainModels;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.ViewModels.AccountViewModels
{
	public class TaskViewModel
	{
        public int? Id { get; set; }
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Must be at least 4 characters long.")]

        public string Name { get; set; }

        public status Status { get; set; }

        [Required]
        public string Deadline { get; set; }

        [StringLength(200, MinimumLength = 10, ErrorMessage = "Must be at least 10 characters long.")]
        public string Description { get; set; }

        [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Progress { get; set; }

        public string? DeveloperId { get; set; }

        public string? DeveloperName { get; set; }

        public string? DeveloperUserName { get; set; }

        public bool IsDeveloperAssigned { get; set; }

        public string? ManagerId { get; set; }

        public string? ManagerName { get; set; }

        public string? ManagerUserName { get; set; }

        public string AdminId { get; set; }

        public string AdminName { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public bool isForClient { get; set; }
    }
}

