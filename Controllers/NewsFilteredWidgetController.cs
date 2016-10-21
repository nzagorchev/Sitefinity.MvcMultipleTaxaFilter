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
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [Category("String Properties")]
        public string Message { get; set; }

        /// <summary>
        /// This is the default Action.
        /// </summary>
        public ActionResult Index()
        {
            var context = SystemManager.CurrentHttpContext;
            var taxa = context.Request.RequestContext.RouteData.Values["taxa"] as Dictionary<Guid, List<TaxonModel>>;
            string filter = string.Empty;
            if (taxa != null && taxa.Count > 0)
            {
                for (int i = 0; i < taxa.Keys.Count; i++)
                {
                    Guid key = taxa.Keys.ElementAt(i);
                    // Category.Contains("{Taxonomy ID}") AND Category.Contains("{Taxonomy ID}")
                    string fieldName = this.GetTaxonomyField(key);
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

            var model = new NewsFilteredWidgetModel();
            var manager = NewsManager.GetManager();
            var newsItemsViewModels = manager.GetNewsItems()
                .Where(n => n.Visible && n.Status == Telerik.Sitefinity.GenericContent.Model.ContentLifecycleStatus.Live)
                .Where(filter)
                .Select(n => new NewsViewModel()
                {
                    Title = n.Title
                })
                .ToList();

            model.Items = newsItemsViewModels;
            return View("Default", model);
        }

        private Type contentType;

        public Type ContentType
        {
            get { return typeof(NewsItem); }
        }


        private string GetTaxonomyField(Guid taxonomyId)
        {
            // TODO: Optimize or execute once for all fields
            var fields = TypeDescriptor.GetProperties(this.ContentType).Cast<PropertyDescriptor>()
                .Where(p => p is TaxonomyPropertyDescriptor)
                .Cast<TaxonomyPropertyDescriptor>()
                .ToList();

            var field = fields.Where(f => f.TaxonomyId == taxonomyId).FirstOrDefault();
            if (field != null)
            {
                return field.MetaField.FieldName;
            }

            return null;
        }
    }
}