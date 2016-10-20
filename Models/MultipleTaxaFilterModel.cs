using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SitefinityWebApp.Mvc.Models
{
    public class MultipleTaxaFilterModel
    {
        [Category("String Properties")]
        public string TaxonomyProvider { get; set; }

        [Category("String Properties")]
        public string TaxonomyIds { get; set; }

        public List<TaxonModel> Tags { get; set; }

        public List<TaxonModel> Categories { get; set; }

        public Dictionary<string, List<TaxonModel>> SelectedTaxa { get; set; }
    }
}