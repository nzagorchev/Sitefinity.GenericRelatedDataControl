using SitefinityWebApp.GenericRelatedData.Designer;
using System;
using System.Web;
using System.Web.UI;
using Telerik.Microsoft.Practices.EnterpriseLibrary.Caching;
using Telerik.Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web.UI.ContentUI;
using Telerik.Sitefinity.Web.UI.ControlDesign;
using Telerik.Sitefinity.Web.UrlEvaluation;

namespace SitefinityWebApp.GenericRelatedData
{
    [ControlDesigner(typeof(RelatedDataControlDesigner))]
    public class RelatedDataControl : ContentView, IContentView
    {
        #region Public properties

        public bool UseWidgetsInView { get; set; }

        public ICacheManager CacheManager
        {
            get
            {
                if (cacheManager == null)
                {
                    cacheManager = SystemManager.GetCacheManager(CacheManagerInstance.Global);
                }

                return cacheManager;
            }
        }

        #endregion

        #region Overriden methods and properties

        protected override void ResolveDetailItem()
        {
            try
            {
                var urlParams = this.GetUrlParameters();
                if (!string.IsNullOrEmpty(urlParams))
                {
                    this.urlParameters = urlParams;
                    var item = GetItemFromCache();
                    if (item != null)
                    {
                        this.DetailItem = item;
                    }
                    else
                    {
                        item = RelatedDataControlUrlHelper.TryGetDynamicItemFromUrl(urlParams);
                        if (item != null)
                        {
                            this.DetailItem = item;
                        }
                        else
                        {
                            item = RelatedDataControlUrlHelper.TryGetContentItemFromUrl(urlParams);
                            if (item != null)
                            {
                                this.DetailItem = item;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (this.IsBackend())
                {
                    throw ex;
                }
                else
                {
                    // Hide the control
                    this.Visible = false;
                    // Write in error log
                    Log.Write(ex, ConfigurationPolicy.ErrorLog);
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            var view = new RelatedDataControlView();
            this.ControlDefinition.Views.Add(view);
            base.OnInit(e);
        }

        protected override string DetermineCurrentViewName()
        {
            if (string.IsNullOrEmpty(this.DetailViewName))
            {
                var detailView = this.ControlDefinition.GetDefaultDetailView();
                if (detailView == null)
                    throw new InvalidOperationException("No detail view.");
                return detailView.ViewName;
            }

            return this.DetailViewName;
        }

        public override string DetailViewName
        {
            get
            {
                return RelatedDataControlView.viewName;
            }
            set
            {
                base.DetailViewName = value;
            }
        }

        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            if (this.Visible)
            {
                this.ResolveDetailItem();
                var currentView = this.DetermineCurrentViewName();

                this.LoadView(currentView);

                this.SubscribeCacheDependency();
            }
        }
        #endregion

        #region Caching
        protected override void SubscribeCacheDependency()
        {
            if (this.DetailItem != null)
            {
                string key = ConstructCacheKey();
                var inCache = this.CacheManager.Contains(key);
                var item = this.DetailItem;
                if (!inCache)
                {
                    this.CacheManager.Add(key,
                            item,
                            CacheItemPriority.Normal,
                            null,
                            new DataItemCacheDependency(item.GetType(), item.Id),
                            new SlidingTime(TimeSpan.FromMinutes(30)));
                }
            }
        }

        protected virtual string ConstructCacheKey()
        {
            var pageId = SiteMap.CurrentNode.Key;
            if (!string.IsNullOrEmpty(this.urlParameters) && !string.IsNullOrEmpty(pageId))
            {
                var key = string.Format("{0}_{1}", this.urlParameters, pageId);
                return key;
            }

            return null;
        }

        protected virtual IDataItem GetItemFromCache()
        {
            string key = ConstructCacheKey();
            if (!string.IsNullOrEmpty(key))
            {
                var inCache = this.CacheManager.Contains(key);
                if (inCache)
                {
                    return this.CacheManager[key] as IDataItem;
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        private string GetUrlParameters()
        {
            var urlParams = this.GetUrlParameterString(true);
            if (!string.IsNullOrEmpty(urlParams))
            {
                urlParams = this.RemovePagingFromUrl(urlParams);
                return urlParams;
            }

            return null;
        }

        private string RemovePagingFromUrl(string url)
        {
            var page = this.GetPageNumber(UrlEvaluationMode.UrlPath, null);
            //the page is greater than 0 - it should exists in the url
            if (!url.IsNullOrEmpty() && page > 0)
            {
                //get paging part from url
                var pageUrl = this.BuildUrl("PageNumber", page, string.Empty);
                pageUrl = pageUrl.TrimEnd("/".ToCharArray());
                var index = url.IndexOf(pageUrl);
                if (index > -1) //the page exists in URL
                {
                    //get the URL till paging
                    url = url.Substring(0, index);
                }
            }
            //remove trailing slash
            url = url.TrimEnd("/".ToCharArray());
            return url;
        }

        #endregion

        #region Private Properties
        private ICacheManager cacheManager;
        private string urlParameters;
        #endregion
    }
}