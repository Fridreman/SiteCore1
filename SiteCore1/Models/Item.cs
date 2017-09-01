using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteCore1.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Img { get; set; }
        public string Price { get; set; }
        public string Url { get; internal set; }
    }
    public class Save
    {
        List<Item> Items = new List<Item>();
    }
}
