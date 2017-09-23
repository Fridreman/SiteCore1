using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SiteCore1.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using SiteCore1.Data;
using HtmlAgilityPack;
using SiteCore1.Controllers;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http.Headers;
using System.Collections;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using System.Diagnostics;
using Imgur.API.Models;
using Imgur.API;

namespace SiteCore1.Controllers
{
    public class HomeController : Controller
    {
        public IHostingEnvironment _environment;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly SignInManager<ApplicationUser> signInManager;
        public readonly ILogger _logger;
        private ProjectContext _context;
        public SkrollController skrollController = new SkrollController();

        public HomeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ProjectContext context,
            ILogger<AccountController> logger, IHostingEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            this.signInManager = signInManager;
            _logger = logger;
            _environment = environment;
        }
        
        public IActionResult Index()
        {
            var AllProjects = _context.Projects.ToList();
            List<Project> EnableProjects = new List<Project>();
            List<Project> ViewProjects = new List<Project>();
            foreach (Project project in AllProjects)
                if(project.Enable == true)
                    EnableProjects.Add(project);
            ViewProjects = EnableProjects.GetRange(EnableProjects.Count - 4, 4);

            for(int TempCountProject = 0; TempCountProject < 4; TempCountProject++)
            {
                for(int TempCountNewProject = 0; TempCountNewProject < EnableProjects.Count; TempCountNewProject++)
                {
                    
                }
            }

            return View(ViewProjects);
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> About(IList<IFormFile> files)
        {
            var uploads = Path.Combine(_environment.WebRootPath, "files");
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(uploads, file.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
            }
            return View();
        }


        public IActionResult ChangeTheme()
        {
            if (Request.Cookies["theme"] == null)
            {
                Response.Cookies.Append("theme", "dark");
            }
            else
            {
                if (Request.Cookies["theme"] == "dark")
                {
                    Response.Cookies.Append("theme", "light");
                }
                else if (Request.Cookies["theme"] == "light")
                {
                    Response.Cookies.Append("theme", "dark");
                }
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [HttpPost]
        public void About_post(HtmlText model)
        {

            var asd = model;
            //return View(model);
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult List()
        {

            ViewData["Message"] = "List user.";

            ViewData["TempUsers"] = _userManager.Users.ToList();
            return View();
        }

        public IActionResult Search(int? id)
        {
            ViewData["Message"] = "Search radio-elements.";
            int page = id ?? 0;
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax)
            {
                if (id != 100)
                    return PartialView("_Search", skrollController.GetItemsPage(page));
                else
                    return PartialView("_Search", skrollController.GetItemsPageOne(1));
            }
            return View(skrollController.GetItemsPage(page));
        }

        public IActionResult _Search()
        {
            ViewBag.Message = "Частичное представление.";
            return _Search();
        }

        [HttpPost]
        public async Task<ActionResult> NewServiceAsync(Content data)
        {
            if (data.Value == "Del")
                await DeleteConfirmedAsync(data.Name);
            else
                await BanConfirmedAsync(data.Name);
            return Ok("True");
        }

        public async Task<IActionResult> DeleteConfirmedAsync(string IdForUser)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(IdForUser);
            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> BanConfirmedAsync(string IdForUser)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(IdForUser);
            await _userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddDays(10));
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}