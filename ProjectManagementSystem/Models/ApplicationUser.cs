using System;
using Microsoft.AspNetCore.Identity;

namespace ProjectManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}

