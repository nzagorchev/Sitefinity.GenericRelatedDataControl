using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.RelatedData.Web.UI;

namespace SitefinityWebApp.GenericRelatedData
{
    public static class RelatedDataControlExtensions
    {
        internal static void SetRelatedDataDefinitionProperties(this IRelatedDataView control, Model dataItem)
        {
            control.RelatedDataDefinition.RelatedFieldName = dataItem.FieldName;
            control.RelatedDataDefinition.RelatedItemType = dataItem.Item.GetType().FullName;
            control.RelatedDataDefinition.RelationTypeToDisplay = RelationDirection.Child;
            control.RelatedDataDefinition.RelatedItemSource = Telerik.Sitefinity.RelatedData.Web.UI.RelatedItemSource.Url;
        }
    }
}