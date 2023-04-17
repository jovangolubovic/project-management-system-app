using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Models;
using ProjectManagementSystem.Models.DomainModels;

namespace ProjectManagementSystem.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser?>
	{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<Project> Projects { get; set; }
    }
}

