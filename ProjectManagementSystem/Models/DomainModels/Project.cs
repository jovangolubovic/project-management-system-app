using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models.DomainModels
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50, MinimumLength = 4, ErrorMessage = "Must be at least 4 characters long.")]
        public string Name { get; set; }

        [Range(10, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Progress { get; set; }

        [ForeignKey("Users")]
        [Required(ErrorMessage = "Please select a Project Manager.")]
        public string? ProjectManagerId { get; set; }

        public ICollection<ProjectTask> Tasks { get; set; }
    }
}

