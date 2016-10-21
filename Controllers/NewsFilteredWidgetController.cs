using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using SitefinityWebApp.Mvc.Models;
using Telerik.Sitefinity.Services;
using System.Collections.Generic;
using Telerik.Sitefinity.Data.Linq.Dynamic;
using Telerik.Sitefinity.News.Model;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Modules.News;

namespace SitefinityWebApp.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "NewsFilteredWidget", Title = "NewsFilteredWidget", SectionName = "MvcWidgets")]
    public class NewsFilteredWidgetController : Controller
    {
        public Type ContentType
        {
            get
            {
                return typeof(NewsItem);
            }
        }

        /// <summary>
        /// This is the default Action.
        /// </summary>
        public ActionResult Index()
        {
            var context = SystemManager.CurrentHttpContext;
            var taxa = context.Request.RequestContext.RouteData.Values[MultipleTaxaFilterController.routeDataTaxaKey] as Dictionary<Guid, List<TaxonModel>>;
            string filter = ConstructFilterExpression(taxa);

            var model = new NewsFilteredWidgetModel();

            // Show news items only if there is a filter
            if (!string.IsNullOrEmpty(filter))
            {
                var manager = NewsManager.GetManager();
                var newsItems = manager.GetNewsItems()
                    .Where(n => n.Visible && n.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Live);

                newsItems = newsItems.Where(filter);

                var newsItemsViewModels = newsItems
                    .Skip(0)
                    .Take(20)
                    .Select(n => new NewsViewModel()
                    {
                        Title = n.Title
                    })
                    .ToList();

                model.Items = newsItemsViewModels;
            }

            return View("Default", model);
        }

        private string ConstructFilterExpression(Dictionary<Guid, List<TaxonModel>> taxa)
        {
            string filter = string.Empty;
            if (taxa != null && taxa.Count > 0)
            {
                var fields = GetTaxonomyFields(this.ContentType);
                for (int i = 0; i < taxa.Keys.Count; i++)
                {
                    Guid key = taxa.Keys.ElementAt(i);
                    // Category.Contains(({Taxon ID})) AND Category.Contains(({Taxon ID}))
                    string fieldName = GetTaxonomyField(fields, key);
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        var items = taxa[key];
                        if (items != null && items.Count > 0)
                        {
                            if (i > 0)
                            {
                                filter += " AND ";
                            }

                            Guid[] ids = items.Select(it => it.Id).ToArray();
                            for (int k = 0; k < ids.Length; k++)
                            {
                                filter += string.Format("{0}.Contains(({1}))", fieldName, ids[k]);
                                if (k < ids.Length - 1)
                                {
                                    filter += " AND ";
                                }
                            }
                        }
                    }
                }
            }
            return filter;
        }

        private static string GetTaxonomyField(TaxonomyPropertyDescriptor[] fields, Guid taxonomyId)
        {
            var field = fields.Where(f => f.TaxonomyId == taxonomyId).FirstOrDefault();
            if (field != null)
            {
                return field.MetaField.FieldName;
            }

            return null;
        }

        private static TaxonomyPropertyDescriptor[] GetTaxonomyFields(Type type)
        {
            var fields = TypeDescriptor.GetProperties(type)
                .Cast<PropertyDescriptor>()
                .Where(descriptor => descriptor is TaxonomyPropertyDescriptor)
                .Cast<TaxonomyPropertyDescriptor>()
                .ToArray();

            return fields;
        }
    }
}