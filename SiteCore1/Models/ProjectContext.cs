using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SiteCore1.Models;

namespace SiteCore1.Models
{
    public class ProjectContext : Data.DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public ProjectContext(DbContextOptions<Data.DbContext> options) : base(options) { }
        public DbSet<SiteCore1.Models.ApplicationUser> ApplicationUser { get; set; }
    }
}
