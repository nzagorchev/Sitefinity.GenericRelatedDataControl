<%@ Control Language="C#" %>
<%@ Import Namespace="Telerik.Sitefinity.RelatedData" %>

<div class="sfMultiRelatedItmsWrp">
    <h2 class="sfrelatedItmTitle">
        <asp:Label ID="FieldNameLabel" runat="server" Text="Label"></asp:Label>
    </h2>
    <ul class="sfrelatedList sflist">
        <asp:Repeater ID="RepeaterItems" runat="server"
            Visible="false">
            <ItemTemplate>
                <li class="sfrelatedListItem sflistitem">
                    <a href='<%# (Container.DataItem as Telerik.Sitefinity.Model.IDataItem).GetDefaultUrl() %>'>
                        <%# DataBinder.Eval(Container.DataItem, "Title") %>
                    </a>
                </li>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Repeater ID="RepeaterMediaItems" runat="server"
            Visible="false">
            <ItemTemplate>
                <li class="sfrelatedListItem sflistitem">
                    <a href='<%# DataBinder.Eval(Container.DataItem, "MediaUrl") %>'>
                        <img src='<%# DataBinder.Eval(Container.DataItem, "ThumbnailUrl") %>'
                            alt='<%# DataBinder.Eval(Container.DataItem, "AlternativeText") %>'
                            title='<%# DataBinder.Eval(Container.DataItem, "Title") %>' />
                    </a>
                </li>
            </ItemTemplate>
        </asp:Repeater>

        <li id="EmptyDataSourcePanel" runat="server" visible="false">
            <span>No related items</span>
        </li>
    </ul>
</div>
