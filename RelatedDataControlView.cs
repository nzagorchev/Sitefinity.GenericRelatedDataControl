using SitefinityWebApp.GenericRelatedData.SimpleLinks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using Telerik.Microsoft.Practices.EnterpriseLibrary.Caching;
using Telerik.Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Descriptors;
using Telerik.Sitefinity.DynamicModules.Builder;
using Telerik.Sitefinity.DynamicModules.Builder.Model;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.DynamicModules.Web.UI.Frontend;
using Telerik.Sitefinity.Events.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Metadata.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Ecommerce.Catalog.Web.UI.Views;
using Telerik.Sitefinity.Modules.Events.Web.UI;
using Telerik.Sitefinity.Modules.Libraries.Web.UI.Documents;
using Telerik.Sitefinity.Modules.Libraries.Web.UI.Images;
using Telerik.Sitefinity.Modules.News.Web.UI;
using Telerik.Sitefinity.Modules.Pages;
using Telerik.Sitefinity.News.Model;
using Telerik.Sitefinity.Pages.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web.UI;
using Telerik.Sitefinity.Web.UI.ContentUI.Contracts;
using Telerik.Sitefinity.Web.UI.ContentUI.Views.Backend;
using Telerik.Web.UI;

namespace SitefinityWebApp.GenericRelatedData
{
    public class RelatedDataControlView : ViewBase, IContentViewDefinition
    {
        #region Public Properties
        public bool UseWidgets { get; set; }

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

        public bool isItemFromCache { get; set; }

        public IDataItem cachedItem { get; set; }
        #endregion

        #region Layout Template
        protected override string LayoutTemplateName
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets or sets the layout template path.
        /// </summary>
        /// <value>The layout template path.</value>
        public override string LayoutTemplatePath
        {
            get
            {
                return RelatedDataControlView.layoutTemplatePath;
            }
            set
            {
                base.LayoutTemplatePath = value;
            }
        }
        #endregion

        #region Control References
        public virtual RadListView RelatedDataView
        {
            get
            {
                return this.Container.GetControl<RadListView>("RelatedDataView", true);
            }
        }

        public virtual System.Web.UI.WebControls.Panel ContentPanel
        {
            get
            {
                return this.Container.GetControl<System.Web.UI.WebControls.Panel>("ContentPanel", true);
            }
        }

        public virtual System.Web.UI.WebControls.Label NoItemResolvedLabel
        {
            get
            {
                return this.Container.GetControl<System.Web.UI.WebControls.Label>("Label1", true);
            }
        }
        #endregion

        #region Overridden methods
        /// <summary>
        /// Gets the master definition.
        /// </summary>
        /// <value>The master definition.</value>
        protected virtual IContentViewDetailDefinition DetailDefinition
        {
            get
            {
                return this.Definition as IContentViewDetailDefinition;
            }
        }

        protected override void InitializeControls(GenericContainer container, IContentViewDefinition definition)
        {
            try
            {
                this.RelatedDataView.ItemDataBound += RelatedDataView_ItemDataBound;

                var item = this.Host.DetailItem;
                if (item != null)
                {
                    var itemsFromCache = this.GetItemFromCache(item.Id);
                    if (itemsFromCache != null)
                    {
                        this.isItemFromCache = true;
                        this.RelatedDataView.DataSource = itemsFromCache;
                        this.RelatedDataView.DataBind();
                    }
                    else
                    {
                        var dynItem = item as DynamicContent;
                        if (dynItem != null)
                        {
                            this.ProcessDynamicItem(dynItem);
                        }
                        else
                        {
                            this.ProcessContentItem(item);
                        }
                    }
                }
                else
                {
                    this.ProcessNoUrlParams();
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
        #endregion

        #region Process items
        protected virtual void ProcessContentItem(IDataItem currentItem)
        {
            GetRelatedData(currentItem, currentItem.GetType());
        }

        protected virtual void ProcessDynamicItem(DynamicContent item)
        {
            ModuleBuilderManager moduleBuilderManager = ModuleBuilderManager.GetManager();
            var type = moduleBuilderManager.GetItems(typeof(DynamicModuleType), String.Empty, String.Empty, 0, 0)
                                           .OfType<DynamicModuleType>()
                                           .FirstOrDefault(dmt => item.GetType().FullName.Equals(dmt.GetFullTypeName()));

            var mainShortTextField = type.MainShortTextFieldName;

            var fields = moduleBuilderManager.Provider.GetDynamicModuleFields()
                .Where(f => f.ParentTypeId == type.Id && (f.FieldType == FieldType.RelatedData || f.FieldType == FieldType.RelatedMedia));

            var itemType = item.GetType();

            GetRelatedData(item, itemType, fields);
        }

        protected virtual void ProcessNoUrlParams()
        {
            if (this.IsBackend() || this.IsPreviewMode())
            {
                this.ContentPanel.Visible = false;
                this.NoItemResolvedLabel.Text =
                    "There should be a single item opened in detail view. The Url params should be resolvable. Example: 'page/item-url'";
                this.NoItemResolvedLabel.Visible = true;
            }
            else
            {
                // Hide the control
                this.Visible = false;
            }
        }
        #endregion

        #region Related Data Methods
        public virtual void RelatedDataView_ItemDataBound(object sender, Telerik.Web.UI.RadListViewItemEventArgs e)
        {
            if (e.Item is RadListViewDataItem)
            {
                var dataItem = ((RadListViewDataItem)e.Item).DataItem as Model;

                this.UseWidgets = (this.Host as RelatedDataControl).UseWidgetsInView;
                if (dataItem != null)
                {
                    if (!this.UseWidgets)
                    {
                        var control = new SimpleLinksWidget();
                        control.DataSource = GetRelatedItems(dataItem);
                        control.FieldName = dataItem.FieldName;
                        control.ItemsType = dataItem.FieldType;
                        e.Item.Controls.Add(control);
                    }
                    else
                    {
                        if (dataItem.FieldType.Contains("sf_ec_prdct"))
                        {
                            var productsControl = new LightProductsView();
                            productsControl.Title = dataItem.FieldName;
                            productsControl.SetRelatedDataDefinitionProperties(dataItem);

                            e.Item.Controls.Add(productsControl);
                            return;
                        }

                        if (dataItem.FieldType == typeof(Image).FullName)
                        {
                            var imageControl = new ImagesView();
                            imageControl.MasterViewName = "ImagesFrontendThumbnailsListLightBox";
                            imageControl.Title = dataItem.FieldName;
                            imageControl.SetRelatedDataDefinitionProperties(dataItem);

                            e.Item.Controls.Add(imageControl);
                            return;
                        }

                        if (dataItem.FieldType == typeof(Document).FullName)
                        {
                            var documentsControl = new DownloadListView();
                            documentsControl.ControlDefinitionName = "FrontendDocuments";
                            documentsControl.Title = dataItem.FieldName;
                            documentsControl.SetRelatedDataDefinitionProperties(dataItem);

                            e.Item.Controls.Add(documentsControl);
                            return;
                        }

                        if (dataItem.FieldType == typeof(NewsItem).FullName)
                        {
                            var newsControl = new NewsView();
                            newsControl.ControlDefinitionName = "NewsFrontend";

                            newsControl.Title = dataItem.FieldName;
                            newsControl.SetRelatedDataDefinitionProperties(dataItem);

                            e.Item.Controls.Add(newsControl);
                            return;
                        }

                        if (dataItem.FieldType == typeof(Event).FullName)
                        {
                            var eventsControl = new EventsView();
                            eventsControl.ControlDefinitionName = "EventsFrontend";
                            eventsControl.Title = dataItem.FieldName;

                            eventsControl.SetRelatedDataDefinitionProperties(dataItem);

                            e.Item.Controls.Add(eventsControl);
                            return;
                        }

                        if (dataItem.FieldType.Contains("Telerik.Sitefinity.DynamicTypes.Model"))
                        {
                            string controlType = "Telerik.Sitefinity.DynamicModules.Web.UI.Frontend.DynamicContentViewMaster";
                            var listTemplate = PageManager.GetManager()
                                .GetPresentationItems<ControlPresentation>()
                                .Where(t => t.ControlType ==
                                     controlType && t.Condition == dataItem.FieldType)
                                    .FirstOrDefault();

                            if (listTemplate != null)
                            {
                                var dynamicControl = new DynamicContentView();
                                dynamicControl.DynamicContentTypeName = dataItem.FieldType;
                                dynamicControl.Title = dataItem.FieldName;
                                dynamicControl.SetRelatedDataDefinitionProperties(dataItem);
                                dynamicControl.DefaultMasterTemplateKey = listTemplate.Id.ToString();

                                e.Item.Controls.Add(dynamicControl);
                            }

                            return;
                        }
                    }
                }
            }
        }

        private void GetRelatedData(DynamicContent item, Type typeCurrent, IQueryable<DynamicModuleField> fields)
        {
            var models = new List<Model>();
            foreach (var field in fields)
            {
                var current = new Model();
                current.Item = item;
                current.FieldName = field.Name;
                current.RelatedDataProvider = field.RelatedDataProvider;
                current.FieldType = field.RelatedDataType;

                models.Add(current);
            }

            this.AddItemToCache(item.Id, models);

            this.RelatedDataView.DataSource = models;
            this.RelatedDataView.DataBind();
        }

        private void GetRelatedData(Telerik.Sitefinity.Model.IDataItem currentItem, Type type)
        {
            var item = currentItem as IDynamicFieldsContainer;
            if (item != null)
            {
                var fields = TypeDescriptor.GetProperties(type).OfType<RelatedDataPropertyDescriptor>();

                var models = new List<Model>();
                foreach (var field in fields)
                {
                    var current = new Model();
                    current.Item = item;
                    current.FieldName = field.Name;

                    var attributesCollection = field.Attributes[typeof(MetaFieldAttributeAttribute)] as MetaFieldAttributeAttribute;
                    if (attributesCollection != null)
                    {
                        string childItemTypeName = null;
                        string childItemProviderName = null;
                        attributesCollection.Attributes.TryGetValue("RelatedType", out childItemTypeName);
                        attributesCollection.Attributes.TryGetValue("RelatedProviders", out childItemProviderName);
                        current.FieldType = childItemTypeName;
                        current.RelatedDataProvider = childItemProviderName;
                    }

                    models.Add(current);
                }

                this.AddItemToCache(currentItem.Id, models);

                this.RelatedDataView.DataSource = models;
                this.RelatedDataView.DataBind();
            }
        }

        // Get the related items - when using simple links. 
        // Return IEnumerable object - common DataSource
        public IEnumerable<object> GetRelatedItems(object item)
        {
            var it = item as Model;
            if (this.isItemFromCache)
            {
                // Query the item, since the cached one is not in the object scope
                if (cachedItem == null)
                {
                    var type = it.Item.GetType();
                    var manager = ManagerBase.GetMappedManager(type);
                    var attachedItem = manager.GetItem(type, it.Item.GetValue<Guid>("Id"));
                    this.cachedItem = attachedItem as IDataItem;
                }
                // filter the related items by the parent item status (or both master/live will be queried)
                ContentLifecycleStatus status = (this.cachedItem as ILifecycleDataItemGeneric).Status;
                var result = this.cachedItem.GetRelatedItems(it.FieldName).ToList().OfType<ILifecycleDataItemGeneric>()
                    .Where(i => i.Status == status);

                return result;
            }
            else
            {
                ContentLifecycleStatus status = it.Item.GetValue<ContentLifecycleStatus>("Status");

                var result = it.Item.GetRelatedItems(it.FieldName).ToList().OfType<ILifecycleDataItemGeneric>()
                    .Where(i => i.Status == status);

                return result;
            }
        }
        #endregion

        #region Caching
        private void AddItemToCache(Guid id, List<Model> models)
        {
            string key = string.Format("RelatedDataModel_{0}", id);
            this.CacheManager.Add(key,
                            models,
                            CacheItemPriority.Normal,
                            null,
                            new DataItemCacheDependency(typeof(Model), id),
                            new SlidingTime(TimeSpan.FromMinutes(30)));
        }

        private List<Model> GetItemFromCache(Guid id)
        {
            string key = string.Format("RelatedDataModel_{0}", id);
            if (!string.IsNullOrEmpty(key))
            {
                var inCache = this.CacheManager.Contains(key);
                if (inCache)
                {
                    return this.CacheManager[key] as List<Model>;
                }
            }

            return null;
        }
        #endregion

        #region IContentViewDefinition Interface
        public Telerik.Sitefinity.Modules.GenericContent.Contracts.ICommentsSettingsDefinition CommentsSettingsDefinition
        {
            get { return null; }
        }

        public string ControlDefinitionName
        {
            get
            {
                return RelatedDataControlView.viewName;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ControlId
        {
            get
            {
                return Guid.NewGuid().ToString();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Description
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Telerik.Sitefinity.Web.UI.Fields.Enums.FieldDisplayMode DisplayMode
        {
            get
            {
                return Telerik.Sitefinity.Web.UI.Fields.Enums.FieldDisplayMode.Read;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        new public Dictionary<string, string> ExternalClientScripts
        {
            get
            {
                return (base.ExternalClientScripts as Dictionary<string, string>);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsMasterView
        {
            get { return false; }
        }

        public IList<ILabelDefinition> Labels
        {
            get { return null; }
        }

        public Dictionary<string, string> Localization
        {
            get
            {
                return new Dictionary<string, string>();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ParentTitleFormat
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IContentViewPlugInDefinition> PlugIns
        {
            get { return null; }
        }

        public List<Telerik.Sitefinity.Web.UI.Backend.Elements.Contracts.IPromptDialogDefinition> PromptDialogs
        {
            get { return new List<Telerik.Sitefinity.Web.UI.Backend.Elements.Contracts.IPromptDialogDefinition>(); }
        }

        public string ResourceClassId
        {
            get
            {
                return Guid.Empty.ToString();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string TemplateKey
        {
            get
            {
                return base.TemplateKey;
            }
            set
            {
                base.TemplateKey = value;
            }
        }

        public string TemplateName
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string TemplatePath
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Title
        {
            get
            {
                return RelatedDataControlView.viewName;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool? UseWorkflow
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ViewName
        {
            get
            {
                return RelatedDataControlView.viewName;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Type ViewType
        {
            get
            {
                return typeof(RelatedDataControlView);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ViewVirtualPath
        {
            get
            {
                return string.Empty;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public TDefinition GetDefinition<TDefinition>() where TDefinition : Telerik.Sitefinity.Web.UI.DefinitionBase, new()
        {
            return null;
        }

        public Telerik.Sitefinity.Web.UI.DefinitionBase GetDefinition()
        {
            return this.Definition as DefinitionBase;
        }

        #endregion

        #region Private members & constants
        public static readonly string layoutTemplatePath = "~/GenericRelatedData/RelatedDataControlTemplate.ascx";
        public const string viewName = "RelatedDataControlView";
        private ICacheManager cacheManager;
        #endregion  
    }
}