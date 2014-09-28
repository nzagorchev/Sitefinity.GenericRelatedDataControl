using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Telerik.Sitefinity.Blogs.Model;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.DynamicModules;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Ecommerce.Catalog.Model;
using Telerik.Sitefinity.Events.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.GenericContent;
using Telerik.Sitefinity.News.Model;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace SitefinityWebApp.GenericRelatedData
{
    internal static class RelatedDataControlUrlHelper
    {
        private static List<string> types;

        public static List<string> Types
        {
            get
            {
                if (types == null)
                {
                    types = RelatedDataControlUrlHelper.GetTypeNames();
                }

                return types;
            }
        }

        internal static IDataItem TryGetContentItemFromUrl(string urlParams)
        {
            var typeNames = RelatedDataControlUrlHelper.Types;
            foreach (var itemType in typeNames)
            {
                var typeString = itemType;
                Type type = TypeResolutionService.ResolveType(typeString);
                var manager = ManagerBase.GetMappedManager(typeString);
                string redirectUrl = string.Empty;
                var currentItem = ((IContentManager)manager).GetItemFromUrl(type, urlParams, out redirectUrl);
                if (currentItem != null)
                {
                    return currentItem;
                }
            }

            return null;
        }

        private static List<string> GetTypeNames()
        {
            //newsitem
            //blogpost
            //event
            //image
            //video
            //document
            //listitem
            // //product
            var result = new List<string>();
            result.Add(typeof(NewsItem).FullName);
            result.Add(typeof(Event).FullName);
            result.Add(typeof(Product).FullName);
            result.Add(typeof(BlogPost).FullName);
            result.Add(typeof(Image).FullName);
            result.Add(typeof(Video).FullName);
            result.Add(typeof(Document).FullName);

            return result;
        }

        internal static IDataItem TryGetDynamicItemFromUrl(string urlParams)
        {
            var typeStr = typeof(DynamicContent).FullName;
            Type typeCurrent = TypeResolutionService.ResolveType(typeStr);

            var items = DynamicModuleManager.GetManager().GetDataItems(typeCurrent)
                 .Where(i => i.ItemDefaultUrl == urlParams && i.Status == ContentLifecycleStatus.Live);

            var count = items.Count();
            if (count > 1)
            {
                foreach (var item in items)
                {
                    var service = Telerik.Sitefinity.Services.SystemManager.GetContentLocationService();
                    var isThisItem = service.GetItemLocations(item).Any(s => s.PageId == new Guid(SiteMap.CurrentNode.Key));
                    if (isThisItem)
                    {
                        return item;
                    }
                }
            }
            else
            {
                return items.FirstOrDefault();
            }

            return null;
        }
    }
}