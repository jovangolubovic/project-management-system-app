using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models.ViewModels
{
	public class ManagerViewModel
	{
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        [NotMapped]
        public string Fullname { get { return this.Name + " " + this.Surname; } }
    }
}

