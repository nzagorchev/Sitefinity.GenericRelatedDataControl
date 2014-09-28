/// <reference name="MicrosoftAjax.js"/>
Type.registerNamespace("SitefinityWebApp.GenericRelatedData.Designer");

SitefinityWebApp.GenericRelatedData.Designer.RelatedDataControlDesigner = function (element) {

    this._useWidgetsCheckBox = null;

    SitefinityWebApp.GenericRelatedData.Designer.RelatedDataControlDesigner.initializeBase(this, [element]);
};

SitefinityWebApp.GenericRelatedData.Designer.RelatedDataControlDesigner.prototype = {

    /* --------------------------------- set up and tear down --------------------------------- */

    initialize: function () {
        SitefinityWebApp.GenericRelatedData.Designer.RelatedDataControlDesigner.callBaseMethod(this, 'initialize');
    },

    dispose: function () {
        SitefinityWebApp.GenericRelatedData.Designer.RelatedDataControlDesigner.callBaseMethod(this, 'dispose');
    },

    refreshUI: function () {
        var controlData = this._propertyEditor.get_control();
        var useWidgets = controlData.UseWidgetsInView;
       
        $("#" + this.get_useWidgetsCheckBox().id).prop('checked', useWidgets);
    },

    applyChanges: function () {
        var controlData = this._propertyEditor.get_control();
        controlData.UseWidgetsInView = $("#" + this.get_useWidgetsCheckBox().id).prop('checked');
    },

    get_useWidgetsCheckBox: function () {
        return this._useWidgetsCheckBox;
    },
    set_useWidgetsCheckBox: function (value) {
        this._useWidgetsCheckBox = value;
    }
};

SitefinityWebApp.GenericRelatedData.Designer.RelatedDataControlDesigner.registerClass('SitefinityWebApp.GenericRelatedData.Designer.RelatedDataControlDesigner',
    Telerik.Sitefinity.Web.UI.ControlDesign.ControlDesignerBase);
