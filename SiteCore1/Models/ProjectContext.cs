using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SiteCore1.Models
{
    public class ProjectContext : Data.DbContext
    {
        public DbSet<Project> Projects { get; set; }
        public ProjectContext(DbContextOptions<Data.DbContext> options) : base(options) { }
    }
}
