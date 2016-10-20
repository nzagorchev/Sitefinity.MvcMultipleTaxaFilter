using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Telerik.Sitefinity.Mvc;
using SitefinityWebApp.Mvc.Models;
using Telerik.Sitefinity.Taxonomies;
using Telerik.Sitefinity.Taxonomies.Model;
using System.Linq.Expressions;
using Telerik.Sitefinity.Services;
using System.Collections.Generic;

namespace SitefinityWebApp.Mvc.Controllers
{
    [ControllerToolboxItem(Name = "MultipleTaxaFilter", Title = "MultipleTaxaFilter", SectionName = "MvcWidgets")]
    public class MultipleTaxaFilterController : Controller
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [Category("String Properties")]
        public string TaxonomyProvider { get; set; }

        /// <summary>
        /// This is the default Action.
        /// </summary>
        public ActionResult Index(string categories, string tags)
        {
            var model = new MultipleTaxaFilterModel();

            var manager = TaxonomyManager.GetManager(this.TaxonomyProvider);
            var allCategories = this.GetCategories(manager);
            var allTags = this.GetTags(manager);

            Dictionary<string, List<TaxonModel>> selectedTaxa = this.ResolveSelectedTaxa();
            List<Guid> allSelected = selectedTaxa.Values.SelectMany(x => x).Select(tn => tn.Id).ToList();

            model.Categories = allCategories.Where(t => !allSelected.Contains(t.Id)).Select(TaxonModel.ToTaxonModel).ToList();
            model.Tags = allTags.Where(t => !allSelected.Contains(t.Id)).Select(TaxonModel.ToTaxonModel).ToList();
            model.SelectedTaxa = selectedTaxa;

            return View("Default", model);
        }

        // TODO: move in model
        protected virtual IQueryable<FlatTaxon> GetTags(TaxonomyManager manager)
        {
            return manager.GetTaxa<FlatTaxon>().Where(t => t.TaxonomyId == TaxonomyManager.TagsTaxonomyId).OrderBy(t => t.Title);
        }

        // TODO: move in model
        protected virtual IQueryable<HierarchicalTaxon> GetCategories(TaxonomyManager manager)
        {
            return manager.GetTaxa<HierarchicalTaxon>().Where(t => t.TaxonomyId == TaxonomyManager.CategoriesTaxonomyId).OrderBy(t => t.Title);
        }     

        public virtual Dictionary<string, List<TaxonModel>> ResolveSelectedTaxa()
        {
            var manager = TaxonomyManager.GetManager(this.TaxonomyProvider);

            // TODO: cache the result per query string?
            var result = new Dictionary<string, List<TaxonModel>>();

            var context = SystemManager.CurrentHttpContext;
            var queryString = context.Request.QueryString;
            if (queryString != null && queryString.HasKeys())
            {
                string categoriesKey = manager.GetTaxonomy(TaxonomyManager.CategoriesTaxonomyId).Title.ToLowerInvariant();
                string tagsKey = manager.GetTaxonomy(TaxonomyManager.TagsTaxonomyId).Title.ToLowerInvariant();
                foreach (var key in queryString.AllKeys)
                {
                    string[] filterValues = queryString[key].Split(new char[] { MultipleTaxaFilterController.queryStringSplitChar }, StringSplitOptions.RemoveEmptyEntries);
                    string keyToLowerInvariant = key.ToLowerInvariant();
                    switch (keyToLowerInvariant)
                    {
                        case "categories":
                            {
                                var values = filterValues.Select(v => v.ToUpper()).ToArray();
                                var taxa = this.GetCategories(manager)
                                    .Where(t => values.Contains(t.Title.ToUpper()))
                                    .Select(TaxonModel.ToTaxonModel)
                                    .ToList();

                                // The QueryString will merge if the key is present more than once, for instance categories=cat1,cat2,cat4&tags=tag1&categories=cat1
                                result.Add(keyToLowerInvariant, taxa);
                                break;
                            }
                        case "tags":
                            {
                                var values = filterValues.Select(v => v.ToUpper()).ToArray();
                                var taxa = this.GetTags(manager)
                                    .Where(t => values.Contains(t.Title.ToUpper()))
                                    .Select(TaxonModel.ToTaxonModel)
                                    .ToList();

                                result.Add(keyToLowerInvariant, taxa);
                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            context.Request.RequestContext.RouteData.Values["taxa"] = result;
            return result;
        }

        internal static readonly char queryStringSplitChar = ',';
    }
}