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
using SiteCore1;
using System.Text.RegularExpressions;

namespace SiteCore1.Controllers
{
    public class HomeController : Controller
    {
        List<Item> Items = new List<Item>();
        int pageSize = 0;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger _logger;

        public HomeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            this.signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
           
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
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
                if(id != 100)
                return PartialView("_Search", GetItemsPage(page));
                else
                    return PartialView("_Search", GetItemsPageOne(1));
            }
            return View(GetItemsPage(page));
        }

        public IActionResult _Search()
        {
            ViewBag.Message = "Это частичное представление.";
            return _Search();
        }

        private List<Item> GetItemsPageOne(int page = 1)
        {
            pageSize = 10;
            var itemsToSkip = 1;
            return Items.OrderBy(t => t.Id).Skip(itemsToSkip).
                Take(pageSize).ToList();
        }

        private List<Item> GetItemsPage(int page)
        {
            var pageSize = 10;
            var itemsToSkip = page * pageSize;
            if (itemsToSkip == 0)
            {
                return Items.OrderBy(t => t.Id).Skip(0).
                Take(pageSize).ToList();
            }
            if (1 > Items.Count)
                return null;
            return Items.OrderBy(t => t.Id).Skip(0).
                Take(10).ToList();
        }

        [HttpPost]
        public PartialViewResult GetAllItem(Where data)
        {
            var html1 = "http://belchip.by/search/?query=" + @data.Href;
            var html2 = "https://www.ru-chipdip.by/search?searchtext=" + @data.Href;
            HtmlWeb web1 = new HtmlWeb();
            HtmlWeb web2 = new HtmlWeb();
            GetAllItemChipdip(web2.Load(html2), data.Page * 10);
            GetAllItemBelchip(web1.Load(html1), data.Page * 10);
            var Res = PartialView("_Search", GetItemsPage(data.Page));
            return Res;
        }

        public void GetAllItemChipdip(HtmlDocument doc, int Page)
        {
            var Nodes = doc.DocumentNode.SelectNodes("//tr[@class='with-hover']");
            for (int i = Page; i < (Page+5); i++)
            {
                if (i > Nodes.Count)
                    break;
                Item TempItem = new Item();
                foreach (var cell in Nodes[i].SelectNodes(".//a[@class='link']"))
                 {
                    TempItem.Name = cell.InnerText;
                    TempItem.Url = cell.OuterHtml.Remove(0, 31);
                    TempItem.Url = "https://www.ru-chipdip.by/product/" + TempItem.Url.Substring(0, TempItem.Url.IndexOf('"'));
                }
                foreach (var cell in Nodes[i].SelectNodes(".//img[@class='img75']"))
                {
                    TempItem.Img = cell.OuterHtml.Remove(0,10);
                    TempItem.Img = TempItem.Img.Substring(0, TempItem.Img.IndexOf('"'));
                }
                foreach (var cell in Nodes[i].SelectNodes(".//span[@class='price_mr']"))
                {
                    TempItem.Price = cell.InnerHtml;
                }
                TempItem.Id = i + 1;
                Items.Add(TempItem);
            }
        }

        public void GetAllItemBelchip(HtmlDocument doc, int Page)
        {
            var Nodes = doc.DocumentNode.SelectNodes("//div[@class='cat-item']");
            var IdItem = "test";
            var NameItem = "test";
            var PriceItem = "цена по запросу";
            var a = 0;
            for (int i = Page; i < (Page+5); i++)
            {
                if (i >= Nodes.Count)
                    break;

                var node = Nodes[i].SelectSingleNode("//div[@class='denoPrice']").ToString();
                HtmlNodeCollection CountPrice;
                if (node != null)
                    CountPrice = Nodes[i].SelectNodes("//div[@class='denoPrice']");
                else
                    CountPrice = Nodes[i].SelectNodes(".//h3");

                PriceItem = "цена по запросу";
                Item TempItem = new Item();

                //Id
                foreach (var cell in Nodes[i].SelectNodes(".//h3"))
                {
                    IdItem = cell.InnerHtml.ToString();
                    IdItem = IdItem.Substring(35, 5);
                }

                foreach (var cell in Nodes[i].SelectNodes(".//a"))
                {
                    NameItem = cell.InnerText;
                    if (cell.LastChild.Name == "#text")
                        break;
                }
                var tempNode = Nodes[i].InnerText;
                if (!tempNode.Contains("запросу"))
                {
                    PriceItem = CountPrice[a].InnerText;
                    a++;
                }

                TempItem.Name = NameItem;
                TempItem.Price = PriceItem;
                TempItem.Url = "http://belchip.by/product/?selected_product=" + IdItem;
                TempItem.Img = "http://belchip.by/sitepics/" + IdItem + ".jpg";
                TempItem.Id = i + 6;
                Items.Add(TempItem);
            }
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
/*

    function fun(e) {
        var href = "http://belchip.by/search/?query=raspberry";
        var url = "/Home/GetAllItem";

        $.ajax({
            url: url,
            async: false,
            type: "POST",
            data: {"Name": url, "Value": href },
            error: function (jqXHR, textStatus, errorThrown) {
            alert("ERROR");
        console.log("FAIL: " + errorThrown);
            },
            success: function (data, textStatus, jqXHR) {
            alert(data);
        console.log("SUCCES");
                //var content = $(data);
                //$("#result").empty().append(content);
                //var z = document.getElementById("result");
                //z.innerHTML = data;


            }
        });
    }





    <table class="table">
    <thead>
        <tr>
                <th>
                    @Html.DisplayNameFor(model => model.Name)
                </th>
                <th>
                    @Html.DisplayNameFor(model => model.Img)
                </th>
                <th>
                    @Html.DisplayNameFor(model => model.Price)
                </th>
            <th></th>
        </tr>
    </thead>
    <tbody>
    */
