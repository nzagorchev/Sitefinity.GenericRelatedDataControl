<%@ Control Language="C#" %>

<style>
    .sfrelatedItmTitle, .sfmediaFieldTitle, .sfitemFieldLbl {
        font-size: 15px;
        font-weight: bold;
        margin-bottom: 5px;
    }

    h2.sfHeading {
        border-bottom: 2px solid #e4e4e4;
        border-top: 2px solid #e4e4e4;
    }
</style>

<script>
    if (typeof jQuery !== undefined) {
        jQuery(document).ready(function () {
            jQuery("div[id*='RelatedDataView']").addClass("sfitemFieldLbl");
        });
    }
</script>

<asp:Label ID="Label1" runat="server" Text="Label" Visible="false"></asp:Label>
<asp:Panel ID="ContentPanel" runat="server">
    <h2 class="sfrelatedItmTitle sfHeading">Related Fields
    </h2>

    <telerik:RadListView ID="RelatedDataView" ItemPlaceholderID="ItemContainer" AllowPaging="False"
        runat="server" EnableEmbeddedSkins="false" EnableEmbeddedBaseStylesheet="false">
        <LayoutTemplate>
            <asp:PlaceHolder ID="ItemContainer" runat="server" />
        </LayoutTemplate>
        <ItemTemplate>
        </ItemTemplate>
    </telerik:RadListView>
</asp:Panel>
