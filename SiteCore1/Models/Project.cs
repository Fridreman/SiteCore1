using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SiteCore1.Models
{
    public class ProjectSetup
    {
        [Required(ErrorMessage = "Не указано название проекта")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 20 символов")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Неверно указана дата")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "Неверно указаны средства проекта")]
        [RegularExpression(@"[\d]+$", ErrorMessage = "Неверно указаны средства проекта")]
        public string Price { get; set; }
    }

    public class ProjectSetupContent
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string ImageTitle { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Date { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public string Price { get; set; }
        [Required]
        public string Lots { get; set; }
        [Required]
        public bool Enable { get; set; } = false;
        [Required]
        public HtmlText Content { get; set; }
    }

    public class Project
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string ImageTitle { get; set; }
        public string Text { get; set; }
        public string Price { get; set; }
        public string Lots { get; set; }
        public bool Enable { get; set; } = false;
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string Money { get; set; }
    }

    public class SendLot
    {
        public string Text { get; set; }
        public string Price { get; set; }
    }
}
