using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiteCore1.Models;
using Microsoft.AspNetCore.Mvc;
using HtmlAgilityPack;

namespace SiteCore1.Controllers
{
    public class SkrollController : Controller
    {
        private ProjectContext _context;
        List<Item> Items = new List<Item>();
        int pageSize = 0;

        public SkrollController(ProjectContext context)
        {
            _context = context;
        }

        public SkrollController()
        {
        }

        public List<Item> GetItemsPageOne(int page = 1)
        {
            pageSize = 10;
            var itemsToSkip = 1;
            return Items.OrderBy(t => t.Id).Skip(itemsToSkip).
                Take(pageSize).ToList();
        }

        public List<Item> GetItemsPage(int page)
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
            var Res = PartialView("~/Views/Home/_Search.cshtml", GetItemsPage(data.Page));
            return Res;
        }

        public void GetAllItemChipdip(HtmlDocument doc, int Page)
        {
            var Nodes = doc.DocumentNode.SelectNodes("//tr[@class='with-hover']");
            for (int i = Page; i < (Page + 5); i++)
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
                    TempItem.Img = cell.OuterHtml.Remove(0, 10);
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
            for (int i = Page; i < (Page + 5); i++)
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
    }

}
