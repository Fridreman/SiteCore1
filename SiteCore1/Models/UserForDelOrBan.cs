using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SiteCore1.Models
{
    public class Content
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class Where
    {
        public int Page { get; set; }
        public string Href { get; set; }
    }
}
