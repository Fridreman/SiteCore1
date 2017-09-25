using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace SiteCore1.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string CountProject { get; set; }
        public string ImagePasport { get; set; }
        public DateTime DateReg { get; set; }
        public DateTime DateLog { get; set; }
        public bool Verified { get; set; }
        public bool AskVerified { get; set; }
    }
}
