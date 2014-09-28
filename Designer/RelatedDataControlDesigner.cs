using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Sitefinity.Web.UI.ControlDesign;

namespace SitefinityWebApp.GenericRelatedData.Designer
{
    public class RelatedDataControlDesigner : ControlDesignerBase
    {
        #region LayoutTemplate
        protected override string LayoutTemplateName
        {
            get
            {
                return null;
            }
        }

        public override string LayoutTemplatePath
        {
            get
            {
                return RelatedDataControlDesigner.layoutTemplatePath;
            }
            set
            {
                base.LayoutTemplatePath = value;
            }
        }
        #endregion

        #region Control References
        protected virtual CheckBox UseWidgetsCheckBox
        {
            get
            {
                return this.Container.GetControl<CheckBox>("UseWidgetsCheckBox", true);
            }
        }
        #endregion

        protected override void InitializeControls(Telerik.Sitefinity.Web.UI.GenericContainer container)
        {
        }

        #region Script methods
        public override IEnumerable<System.Web.UI.ScriptDescriptor> GetScriptDescriptors()
        {
            var scriptDescriptors = new List<ScriptDescriptor>(base.GetScriptDescriptors());
            var descriptor = (ScriptControlDescriptor)scriptDescriptors.Last();

            descriptor.AddElementProperty("useWidgetsCheckBox", this.UseWidgetsCheckBox.ClientID);

            return scriptDescriptors;
        }

        public override IEnumerable<System.Web.UI.ScriptReference> GetScriptReferences()
        {
            string assemblyName = typeof(RelatedDataControlDesigner).Assembly.FullName;
            var baseScripts = base.GetScriptReferences();
            var all = baseScripts.ToList();
            all.Add(
                new System.Web.UI.ScriptReference(RelatedDataControlDesigner.scriptReference));
            return all;
        }
        #endregion

        #region Private members & constants
        public static readonly string layoutTemplatePath = "~/GenericRelatedData/Designer/RelatedDataControlDesignerTemplate.ascx";
        public const string scriptReference = "~/GenericRelatedData/Designer/RelatedDataControlDesigner.js";
        #endregion
    }
}