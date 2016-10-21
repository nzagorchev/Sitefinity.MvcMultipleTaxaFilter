using System.Collections.Generic;

namespace SitefinityWebApp.Mvc.Models
{
    public class NewsFilteredWidgetModel
    {
        public IEnumerable<NewsViewModel> Items { get; set; }
    }

    public class NewsViewModel
    {
        public string Title { get; set; }
    }
}