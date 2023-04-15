using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models.ViewModels
{
	public class UserViewModel
	{
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string RoleName { get; set; }
        [NotMapped]
        public string Fullname { get { return this.Name + " " + this.Surname; } }
    }
}

