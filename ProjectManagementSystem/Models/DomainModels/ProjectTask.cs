using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models.DomainModels
{
    public enum status { New, InProgress, Finished };

    public class ProjectTask
	{
        [Key]
        public int Id { get; set; }

        [StringLength(50, MinimumLength = 4, ErrorMessage = "Must be at least 4 characters long.")]
        public string Name { get; set; }

        public status Status { get; set; }

        [Required]
        public DateTime Deadline { get; set; }

        [StringLength(200, MinimumLength = 10, ErrorMessage = "Must be at least 10 characters long.")]
        public string Description { get; set; }

        public int Progress { get; set; }

        public string? DeveloperId { get; set; }

        public string? ManagerId { get; set; }

        public bool IsDeveloperAssigned { get; set; }

        public bool IsManagerAssigned { get; set; }

        public string? AdminId { get; set; }

        [Required(ErrorMessage = "Please select Project.")]
        public int ProjectId { get; set; }
    }
}

