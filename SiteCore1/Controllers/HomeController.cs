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
using Hangfire;
using SiteCore1.Services;

namespace SiteCore1.Controllers
{
    public class HomeController : Controller
    {
        public IHostingEnvironment _environment;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly SignInManager<ApplicationUser> signInManager;
        public readonly ILogger _logger;
        private ProjectContext _context;
        public SkrollController skrollController;
        private readonly IRecurringJobManager _recurringJob;

        public HomeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ProjectContext context,
            ILogger<AccountController> logger, IHostingEnvironment environment, IRecurringJobManager recurringJob)
        {
            _recurringJob = recurringJob;
            _context = context;
            _userManager = userManager;
            this.signInManager = signInManager;
            _logger = logger;
            _environment = environment;
            skrollController = new SkrollController(_context);
        }

        public IActionResult Index()
        {
            RecurringJob.AddOrUpdate(() => CheckProjectsAsync(), Cron.Daily(), TimeZoneInfo.Local);
            var AllProjects = _context.Projects.ToList();
            List<Project> EnableProjects = new List<Project>();
            foreach (Project project in AllProjects)
                if (project.Enable == true)
                {
                    EnableProjects.Add(project);
                }
            EnableProjects.Sort((a, b) => DateTime.Compare(a.DateStart, b.DateStart));
            var ViewProject = EnableProjects.GetRange(EnableProjects.Count - 4, 4);
            EnableProjects.Sort((a, b) => a.Money.CompareTo(b.Money));
            ViewProject.AddRange(EnableProjects.GetRange(EnableProjects.Count - 4, 4));
            ViewProject.AddRange(EnableProjects);
            return View(ViewProject);
        }

        public async Task CheckProjectsAsync()
        {
            var projects = _context.Projects.ToList();
            foreach (var project in projects)
                if (DateTime.Today == project.DateEnd && project.Enable == true)
                    await TheEndProject(project);
        }

        private async Task TheEndProject(Project project)
        {
            var Athor = project.Name_owner;
            var users = project.Users;
            ApplicationUser _applicationUser = await _userManager.FindByNameAsync(Athor);
            EmailService emailService = new EmailService();
            await emailService.SendEmailAsync(_applicationUser.Email, "Ваш проект(" + project.Title + ") закончился.", $"<a href='https://localhost:44318/'>Просмотреть проект и спонсоров.</a>");
            var Count = users.Count(c => c.ToString() == ";");
            for (int a = 0; a < Count; a++)
            {
                var user = await _userManager.FindByEmailAsync(users.Substring(0, users.IndexOf(";")));
                await emailService.SendEmailAsync(user.Email, "Проект(" + project.Title + ") закончился.", $"<a href='https://localhost:44318/'>Просмотреть проект.</a>");
                users.Remove(0, users.IndexOf(";"));
            }
            await _context.SaveChangesAsync();
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
        [Authorize(Roles = "Admin")]
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

        public IActionResult ViewAllProject(int? id)
        {
            ViewData["Message"] = "All project.";
            int page = id ?? 0;
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax)
            {
                if (id != 100)
                    return PartialView("_ViewAllProject", GetProjectPage(page));
                else
                    return PartialView("_ViewAllProject", GetProjectPage(1));
            }
            return View(GetProjectPage(page));
        }

        public IActionResult _ViewAllProject()
        {
            ViewBag.Message = "Частичное представление.";
            return _ViewAllProject();
        }

        public List<Project> GetProjectPage(int page)
        {
            var AllProjects = _context.Projects.ToList();
            List<Project> EnableProjects = new List<Project>();
            foreach (Project project in AllProjects)
                if (project.Enable == true)
                {
                    EnableProjects.Add(project);
                }
            return EnableProjects.GetRange(page, 5);
        }

        [HttpPost]
        public PartialViewResult GetProjectPage(Where data)
        {
            var AllProjects = _context.Projects.ToList();
            var Count = 5;
            List<Project> EnableProjects = new List<Project>();
            foreach (Project project in AllProjects)
                if (project.Enable == true)
                {
                    EnableProjects.Add(project);
                }
            if (EnableProjects.Count - (Count + data.Page) < 0)
                Count = EnableProjects.Count - data.Page;
            var Res = PartialView("~/Views/Home/_ViewAllProject.cshtml", EnableProjects.GetRange(data.Page, Count));
            return Res;
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
                if (data.Value == "Ban")
                await BanConfirmedAsync(data.Name);
            else
                await UnBanConfirmedAsync(data.Name);
            return Ok("True");
        }

        public async Task<IActionResult> DeleteConfirmedAsync(string Email)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(Email);
            await _userManager.DeleteAsync(user);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> BanConfirmedAsync(string Email)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(Email);
            await _userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddYears(10));
            await _userManager.SetLockoutEnabledAsync(user, true);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> UnBanConfirmedAsync(string Email)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(Email);
            await _userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddYears(10));
            await _userManager.SetLockoutEnabledAsync(user, false);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}