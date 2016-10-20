using SitefinityWebApp.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Telerik.Sitefinity;

namespace SitefinityWebApp.Mvc.Helpers
{
    public static class UrlHelpers
    {
        public static string FilterAction(this UrlHelper helper, string action, string controllerName, KeyValuePair<string, string> filter)
        {
            string url = string.Empty;
            if (!string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(controllerName))
            {
                url = helper.Action(action, controllerName);
            }
            else if (!string.IsNullOrEmpty(action))
            {
                url = helper.Action(action);
            }
            else
            {
                url = helper.Action();
            }

            try
            {
                if (!string.IsNullOrEmpty(filter.Key) && !string.IsNullOrEmpty(filter.Value))
                {
                    NameValueCollection queryString = new NameValueCollection(helper.RequestContext.HttpContext.Request.QueryString);
                    bool exists = queryString.AllKeys.Any(k => k.ToLowerInvariant() == filter.Key.ToLowerInvariant());
                    if (exists)
                    {
                        var values = queryString[filter.Key]
                            .Split(new char[] { MultipleTaxaFilterController.queryStringSplitChar }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();

                        int index = values.FindIndex(v => v.ToLowerInvariant() == filter.Value.ToLowerInvariant());
                        if (index >= 0)
                        {
                            values.RemoveAt(index);
                            if (values.Count == 0)
                            {
                                //queryString[filter.Key] = string.Empty;
                                queryString.Remove(filter.Key);
                            }
                            else
                            {
                                queryString[filter.Key] = string.Join(MultipleTaxaFilterController.queryStringSplitChar.ToString(), values);
                            }
                        }
                        else
                        {
                            if (values.Count == 0)
                            {
                                queryString[filter.Key] = filter.Value;
                            }
                            else
                            {
                                queryString[filter.Key] = string.Join(MultipleTaxaFilterController.queryStringSplitChar.ToString(), values);
                                queryString[filter.Key] += MultipleTaxaFilterController.queryStringSplitChar + filter.Value;
                            }
                        }
                    }
                    else
                    {
                        queryString.Add(filter.Key, filter.Value);
                    }

                    // Get current query
                    string query = helper.RequestContext.HttpContext.Request.Url.Query;
                    if (!string.IsNullOrEmpty(query))
                    {
                        // Remove old query
                        url = url.Replace(query, string.Empty);                      
                    }

                    // Append new query string
                    url += queryString.ToQueryString();
                }
            }
            catch (Exception ex)
            {
                // TODO: log the exception instead of throw
                throw ex;
            }

            return url;
        }
    }
}