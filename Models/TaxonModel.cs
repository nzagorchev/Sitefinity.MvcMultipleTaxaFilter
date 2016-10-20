using System;
using System.Linq.Expressions;
using Telerik.Sitefinity.Taxonomies.Model;

namespace SitefinityWebApp.Mvc.Models
{
    public class TaxonModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public static Expression<Func<Taxon, TaxonModel>> ToTaxonModel
        {
            get
            {
                return t => new TaxonModel()
                {
                    Id = t.Id,
                    Title = t.Title,
                    Url = t.UrlName
                };
            }
        }
    }
}