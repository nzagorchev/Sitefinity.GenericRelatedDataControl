using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Web.UI;

namespace SitefinityWebApp.GenericRelatedData.SimpleLinks
{
    public class SimpleLinksWidget : SimpleView
    {
        #region Properties
        public IEnumerable<object> DataSource { get; set; }
        public string ItemsType { get; set; }
        public string FieldName { get; set; }
        /// <summary>
        /// Obsolete. Use LayoutTemplatePath instead.
        /// </summary>
        protected override string LayoutTemplateName
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the layout template's relative or virtual path.
        /// </summary>
        public override string LayoutTemplatePath
        {
            get
            {
                if (string.IsNullOrEmpty(base.LayoutTemplatePath))
                    return SimpleLinksWidget.layoutTemplatePath;
                return base.LayoutTemplatePath;
            }
            set
            {
                base.LayoutTemplatePath = value;
            }
        }
        #endregion

        #region Control References
        /// <summary>
        /// Reference to the Label control that shows the Message.
        /// </summary>
        protected virtual Repeater RepeaterMediaItems
        {
            get
            {
                return this.Container.GetControl<Repeater>("RepeaterMediaItems", true);
            }
        }

        protected virtual Repeater RepeaterItems
        {
            get
            {
                return this.Container.GetControl<Repeater>("RepeaterItems", true);
            }
        }

        protected virtual Label FieldNameLabel
        {
            get
            {
                return this.Container.GetControl<Label>("FieldNameLabel", true);
            }
        }

        protected virtual System.Web.UI.HtmlControls.HtmlGenericControl EmptyDataSourcePanel
        {
            get
            {
                return this.Container.GetControl<System.Web.UI.HtmlControls.HtmlGenericControl>("EmptyDataSourcePanel", true);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes the controls.
        /// </summary>
        /// <param name="container"></param>
        /// <remarks>
        /// Initialize your controls in this method. Do not override CreateChildControls method.
        /// </remarks>
        protected override void InitializeControls(GenericContainer container)
        {
            this.FieldNameLabel.Text = this.FieldName;
            if (this.DataSource.Count() != 0)
            {
                if (ItemsType == typeof(Telerik.Sitefinity.Libraries.Model.Image).FullName)
                {
                    this.RepeaterMediaItems.DataSource = this.DataSource;
                    this.RepeaterMediaItems.DataBind();
                    this.RepeaterMediaItems.Visible = true;
                }
                else
                {
                    this.RepeaterItems.DataSource = this.DataSource;
                    this.RepeaterItems.DataBind();
                    this.RepeaterItems.Visible = true;
                }
            }
            else
            {
                this.EmptyDataSourcePanel.Visible = true;
            }
        }
        #endregion

        #region Private members & constants
        public static readonly string layoutTemplatePath = "~/GenericRelatedData/SimpleLinks/SimpleLinksWidget.ascx";
        #endregion
    }
}
