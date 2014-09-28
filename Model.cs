using Telerik.Sitefinity.Model;

namespace SitefinityWebApp.GenericRelatedData
{
    public class Model
    {
        public IDynamicFieldsContainer Item { get; set; }

        public string FieldName { get; set; }

        public string FieldType { get; set; }

        public string RelatedDataProvider { get; set; }
    }
}