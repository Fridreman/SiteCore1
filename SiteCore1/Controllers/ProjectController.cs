using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Net.Http.Headers;
using SiteCore1.Models;
using Microsoft.AspNetCore.Hosting.Server;
using SiteCore1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System.Diagnostics;
using Imgur.API;
using Imgur.API.Models.Impl;

namespace SiteCore1.Controllers
{
    public class ProjectController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private IHostingEnvironment _environment;
        private ProjectContext _context;
        private DbContext _contextDb;
        ApplicationUser _applicationUser;
        public Project project = new Project();
        public string ProjectId;

        public ProjectController(IHostingEnvironment environment, ProjectContext context, UserManager<ApplicationUser> userManager, DbContext contextDb)
        {
            _contextDb = contextDb;
            _userManager = userManager;
            _environment = environment;
            _context = context;
        }

        public ActionResult Index(string Id)
        {
            var projectTemp = _context.Projects.FirstOrDefault(c => c.Id == Id.ToString());
            ViewData["Title"] = projectTemp.Title;
            ViewData["Image"] = projectTemp.ImageTitle;
            ViewData["Content"] = System.Net.WebUtility.HtmlDecode(projectTemp.Text);
            ViewData["Lots"] = projectTemp.Lots;
            ViewData["Money"] = projectTemp.Money;
            ViewData["Price"] = projectTemp.Price;
            ViewData["DataEnd"] = (projectTemp.DateEnd.Day + "." + projectTemp.DateEnd.Month + "." + projectTemp.DateEnd.Year);
            ViewData["DataStart"] = (projectTemp.DateStart.Day + "." + projectTemp.DateStart.Month + "." + projectTemp.DateStart.Year);
            ViewData["StatusProject"] = GetStatusProject(projectTemp);
            if (!projectTemp.Enable)
                return RedirectToAction("Index", "Home");
            return View();
        }
        
        public string GetStatusProject(Project projectTemp)
        {
            if (DateTime.Now <= projectTemp.DateEnd)
                if (Int32.Parse(projectTemp.Money) >= Int32.Parse(projectTemp.Price))
                    return "WorkAndFull";
            
                else
                    return "WorkAndNotFull"; 
            else
                if (Int32.Parse(projectTemp.Money) >= Int32.Parse(projectTemp.Price))
                    return "NotWorkAndFull"; 
                else
                    return "NotWorkAndNotFull";          
        }
        
        public async Task<ActionResult> Create(int id)
        {
            _applicationUser = await _userManager.FindByNameAsync(User.Identity.Name);
            ProjectId = _applicationUser.Id + "_" + id.ToString();
            project.Id = ProjectId;

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            if (_applicationUser.CountProject == null)
                _applicationUser.CountProject = "0";
            _applicationUser.CountProject = (Int32.Parse(_applicationUser.CountProject) + 1).ToString();

            await _contextDb.SaveChangesAsync();
            await _userManager.UpdateAsync(_applicationUser);

            ViewData["ProjectId"] = ProjectId;

            return View();
        }

         [HttpPost]
         [ValidateAntiForgeryToken]
         public async Task<ActionResult> CreateAsync(ProjectSetup model)
         {
            if (ModelState.IsValid)
            {
                await GetProjectIdAsync();
                var projectTemp = _context.Projects.FirstOrDefault(c => c.Id == ProjectId);
                projectTemp.Title = model.Title;
                projectTemp.DateEnd = model.Date;
                projectTemp.DateStart = DateTime.Today;
                projectTemp.Price = model.Price;
                projectTemp.Money = "0";
                _context.SaveChanges();
                return RedirectToAction("CreateContent", "Project");
            }
            else
                return View(Request.Headers["Referer"]);
        }

        public IActionResult CreateContent()
        {
            return View();
        }

        /*[HttpPost]
        public async Task<IActionResult> Upload(IList<IFormFile> files)
        {
            return View();
        }*/

        [HttpPost]
        public async Task UploadImageAsync(IList<IFormFile> files)
        {
            await GetProjectIdAsync();
            try
            {
                var client = new ImgurClient("01bd44056654677", "8a9f05a8ce2dc6321cb64fe735cbc41cdbca02da");
                var endpoint = new ImageEndpoint(client);
                IImage image;
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        using (var fileStream = file.OpenReadStream())
                        using (var ms = new MemoryStream())
                        {
                            fileStream.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            string s = Convert.ToBase64String(fileBytes);
                            image = await endpoint.UploadImageBinaryAsync(fileBytes);
                        }
                        Debug.Write("Image uploaded. Image Url: " + image.Link);
                        Change_ImageTitle(image.Link);
                    }
                }
            }
            catch (ImgurException imgurEx)
            {
                Debug.Write("An error occurred uploading an image to Imgur.");
                Debug.Write(imgurEx.Message);
            }
        }

        public async Task GetImage(string imageId)
        {
            try
            {
                var client = new ImgurClient("4d5ac665bcb6d0e", "16fc948803ebe3dd614cb08ec074d90179f477c4");
                var endpoint = new ImageEndpoint(client);
                var image = await endpoint.GetImageAsync(imageId);
                Debug.Write("Image retrieved. Image Url: " + image.Link);
            }
            catch (ImgurException imgurEx)
            {
                Debug.Write("An error occurred getting an image from Imgur.");
                Debug.Write(imgurEx.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditContent(HtmlText model)
        {
            _applicationUser = await _userManager.FindByNameAsync(User.Identity.Name);
            GetProjectId((Int32.Parse(_applicationUser.CountProject) - 1).ToString());
            var projectTemp = _context.Projects.FirstOrDefault(c => c.Id == ProjectId);

            if(projectTemp.Enable)
                return RedirectToAction("Index", "Home");
            projectTemp.Text = model.HtmlContent;
            _context.SaveChanges();
            return RedirectToAction("CreateLots");
        }

        public void Change_ImageTitle(string url)
        {
            var projectTemp = _context.Projects.FirstOrDefault(c => c.Id == ProjectId);
            projectTemp.ImageTitle = url;
            _context.SaveChanges();
        }

        public async Task GetProjectIdAsync()
        {
            _applicationUser = await _userManager.FindByNameAsync(User.Identity.Name);
            ProjectId = Request.Headers["Referer"].ToString();
            ProjectId = _applicationUser.Id + "_" + ProjectId.Remove(0, ProjectId.IndexOf("=") + 1);
        }

        public async Task GetProjectIdAsyncByJustBackRequest()
        {
            ProjectId = Request.Headers["Referer"].ToString();
            ProjectId = ProjectId.Remove(0, ProjectId.IndexOf("=") + 1);
        }

        public void GetProjectId(string id)
        {
            ProjectId = _applicationUser.Id + "_" + id;
        }

        public async Task<ActionResult> CreateLots()
        {
            _applicationUser = await _userManager.FindByNameAsync(User.Identity.Name);
            GetProjectId((Int32.Parse(_applicationUser.CountProject) - 1).ToString());
            var projectTemp = _context.Projects.FirstOrDefault(c => c.Id == ProjectId);

            ViewData["Price"] = projectTemp.Price;

            if (projectTemp.Enable)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendLots([FromBody] SendLot[] model)
        {
            _applicationUser = await _userManager.FindByNameAsync(User.Identity.Name);
            GetProjectId((Int32.Parse(_applicationUser.CountProject) - 1).ToString());
            var projectTemp = _context.Projects.FirstOrDefault(c => c.Id == ProjectId);
            
            if (projectTemp.Enable)
                return RedirectToAction("Index", "Home");
            projectTemp.Enable = true;
            String Lots = "";
            for(int i = 0; i < model.Length; i++)
            {
                Lots = Lots + model[i].Text + "_" + model[i].Price + ";";
            }
            projectTemp.Lots = Lots;
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<ActionResult> GetMoney(Content money)
        {
            await GetProjectIdAsyncByJustBackRequest();
            var projectTemp = _context.Projects.FirstOrDefault(c => c.Id == ProjectId);

            if (projectTemp.Money == null)
                projectTemp.Money = "0";
            projectTemp.Money = (Int32.Parse(projectTemp.Money) + Int32.Parse(money.Value)).ToString();
            _context.SaveChanges();
            return Json("Ok");
        }
    }
}