function HandleOnChangeLookup() {

    var site = new Array();
    site = Xrm.Page.getAttribute("cvt_relatedsiteid").getValue();
    var type = Xrm.Page.getAttribute("cvt_type");

    var tsaType = Xrm.Page.ui.controls.get("cvt_tsaresourcetype");
    var ResourceGroupControl = Xrm.Page.ui.controls.get("cvt_relatedresourcegroupid");
    var ResourceControl = Xrm.Page.ui.controls.get("cvt_relatedresourceid");
    var UserControl = Xrm.Page.ui.controls.get("cvt_relateduserid");

    if ((tsaType != "undefined") && (tsaType.getAttribute().getValue() != null) && (site != null)) {

        var attribute = tsaType.getAttribute();
        var tsaTypeFieldValue = attribute.getValue();

        var siteID = site[0].id;

        var viewID = siteID;
        var ViewDisplayName = "Filtered by Site and Type";
        var viewIsDefault = true;

        if (UserControl.getVisible()) {

            if (tsaTypeFieldValue == 2) {
                var UserfetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="systemuser"><attribute name="fullname"/><attribute name="title"/><attribute name="address1_telephone1"/><attribute name="businessunitid"/><attribute name="cvt_type"/><attribute name="cvt_site"/><attribute name="systemuserid"/><order attribute="fullname" descending="false"/><filter type="and"><condition attribute="isdisabled" operator="eq" value="0"/><condition attribute="accessmode" operator="ne" value="3"/><condition attribute="cvt_site" operator="eq" uiname="' + site[0].name + '" uitype="mcs_site" value="' + siteID + '"/><condition attribute="cvt_type" operator="eq" value="917290001"/></filter></entity></fetch>';
                var UserlayoutXml = '<grid name="resultset" object="8" jump="fullname" select="1" icon="1" preview="1"><row name="result" id="systemuserid"><cell name="fullname" width="200" /><cell name="cvt_site" width="125" /><cell name="cvt_type" width="125" /><cell name="businessunitid" width="150" /><cell name="title" width="100" /><cell name="address1_telephone1" width="100" /></row></grid>';
                UserControl.addCustomView(viewID, "systemuser", ViewDisplayName, UserfetchXml, UserlayoutXml, viewIsDefault);
            }
            if (tsaTypeFieldValue == 3) {
                var UserfetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="systemuser"><attribute name="fullname"/><attribute name="title"/><attribute name="address1_telephone1"/><attribute name="businessunitid"/><attribute name="cvt_type"/><attribute name="cvt_site"/><attribute name="systemuserid"/><order attribute="fullname" descending="false"/><filter type="and"><condition attribute="isdisabled" operator="eq" value="0"/><condition attribute="accessmode" operator="ne" value="3"/><condition attribute="cvt_site" operator="eq" uiname="' + site[0].name + '" uitype="mcs_site" value="' + siteID + '"/><condition attribute="cvt_type" operator="eq" value="917290000"/></filter></entity></fetch>';
                var UserlayoutXml = '<grid name="resultset" object="8" jump="fullname" select="1" icon="1" preview="1"><row name="result" id="systemuserid"><cell name="fullname" width="200" /><cell name="cvt_site" width="125" /><cell name="cvt_type" width="125" /><cell name="businessunitid" width="150" /><cell name="title" width="100" /><cell name="address1_telephone1" width="100" /></row></grid>';
                UserControl.addCustomView(viewID, "systemuser", ViewDisplayName, UserfetchXml, UserlayoutXml, viewIsDefault);
            }
        }
        if ((type != "undefined") && (type.getValue() != null)) {

            if (ResourceGroupControl.getVisible()) {
                var RGfetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resourcegroup"><attribute name="mcs_resourcegroupid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/><condition attribute="mcs_relatedsiteid" operator="eq" uiname="' + site[0].name + '" uitype="mcs_site" value="' + siteID + '"/><condition attribute="mcs_type" operator="eq" value="' + type.getValue() + '"/></filter></entity></fetch>';
                var RGlayoutXml = '<grid name="resultset" object="10007" jump="mcs_name" select="1" icon="1" preview="1"><row name="result" id="mcs_resourcegroupid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
                ResourceGroupControl.addCustomView(viewID, "mcs_resourcegroup", ViewDisplayName, RGfetchXml, RGlayoutXml, viewIsDefault);
            }

            if (ResourceControl.getVisible()) {
                var ResourcefetchXml = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false"><entity name="mcs_resource"><attribute name="mcs_resourceid"/><attribute name="mcs_name"/><attribute name="createdon"/><order attribute="mcs_name" descending="false"/><filter type="and"><condition attribute="statecode" operator="eq" value="0"/><condition attribute="mcs_relatedsiteid" operator="eq" uiname="' + site[0].name + '" uitype="mcs_site" value="' + siteID + '"/><condition attribute="mcs_type" operator="eq" value="' + type.getValue() + '"/></filter></entity></fetch>';
                var ResourcelayoutXml = '<grid name="resultset" object="10006" jump="mcs_name" select="1" icon="1" preview="1"><row name="result" id="mcs_resourceid"><cell name="mcs_name" width="200" /><cell name="mcs_relatedsiteid" width="125" /><cell name="mcs_type" width="125" /></row></grid>';
                ResourceControl.addCustomView(viewID, "mcs_resource", ViewDisplayName, ResourcefetchXml, ResourcelayoutXml, viewIsDefault);
            }
        }
    }
}



function showDetails() {

    //get control for field
    var tsaType = Xrm.Page.ui.controls.get("cvt_tsaresourcetype");
    //get control for dependent field
    var type = Xrm.Page.ui.controls.get("cvt_type");
    var relatedResourceGroup = Xrm.Page.ui.controls.get("cvt_relatedresourcegroupid");
    var relatedUser = Xrm.Page.ui.controls.get("cvt_relateduserid");
    var relatedResource = Xrm.Page.ui.controls.get("cvt_relatedresourceid");

    //check if tsaType is null then get attribute value if not. 
    if (tsaType != null) var tsaTypeFieldValue = tsaType.getAttribute("cvt_tsaresourcetype").getValue();
    //check if type is null and then get attritbute value if not. 
    if (type != null) var typeFieldValue = type.getAttribute("cvt_type").getValue();

    if ((tsaType != "undefined") && (tsaType != null)) {
        //set visible based on dependent field's value       
        //if TSA is Resource or Single Resource, we need type
        if ((tsaTypeFieldValue <= 1) && (tsaTypeFieldValue != null)) {
            type.setVisible(true);
            Xrm.Page.getAttribute("cvt_type").setRequiredLevel("required");
        }
        else {
            type.setVisible(false);
            Xrm.Page.getAttribute("cvt_type").setValue(null);
            Xrm.Page.getAttribute("cvt_type").setRequiredLevel("none");
        }

        //this to me would have made more sense in a switch like this:
        //switch (tsaTypeFieldValue) {
        //    case 0:
        //        //code
        //        break;
        //    case 1:
        //        //code
        //    default:
        //}
        //Add comments

        //Check to see if tsaType is a Resource Group(0), and then see if there is a value for Type. Sets Visibility and Requirements.
        // Clears value of related Resource Group if no type is selected. 
        if (tsaTypeFieldValue == 0) {
            if ((typeFieldValue >= 0) && (typeFieldValue != null)) {
                relatedResourceGroup.setVisible(true);
                //don't forget to make this required
                Xrm.Page.getAttribute("cvt_relatedresourcegroupid").setRequiredLevel("required");
            }
            else {
                relatedResourceGroup.setVisible(false);
                Xrm.Page.getAttribute("cvt_relatedresourcegroupid").setValue(null);
                //don't forget to make this required - and turn it off
                Xrm.Page.getAttribute("cvt_relatedresourcegroupid").setRequiredLevel("none");
            }
        }
        else {
            relatedResourceGroup.setVisible(false);
            Xrm.Page.getAttribute("cvt_relatedresourcegroupid").setValue(null);
            Xrm.Page.getAttribute("cvt_relatedresourcegroupid").setRequiredLevel("none");

        }
        //Check to see if tsaType is a Resource(1), and then checks value for Type. Sets Visiblity and Requirements levels. 
        if (tsaTypeFieldValue == 1) {
            if ((typeFieldValue >= 0) && (typeFieldValue != null)) {
                relatedResource.setVisible(true);
                Xrm.Page.getAttribute("cvt_relatedresourceid").setRequiredLevel("required");
            }
            else {
                relatedResource.setVisible(false);
                Xrm.Page.getAttribute("cvt_relatedresourceid").setValue(null);
                Xrm.Page.getAttribute("cvt_type").setValue(null);
                Xrm.Page.getAttribute("cvt_relatedresourceid").setRequiredLevel("none");
            }
        }
        else {
            relatedResource.setVisible(false);
            Xrm.Page.getAttribute("cvt_relatedresourceid").setValue(null);
            Xrm.Page.getAttribute("cvt_relatedresourceid").setRequiredLevel("none");

        }
        //Check to see if tsaType is a Single User, and then sets visiblity and requirements levels. 
        if ((tsaTypeFieldValue == 2) || (tsaTypeFieldValue == 3)) {
            relatedUser.setVisible(true);
            Xrm.Page.getAttribute("cvt_relateduserid").setRequiredLevel("required");
        }
        else {
            relatedUser.setVisible(false);
            Xrm.Page.getAttribute("cvt_relateduserid").setValue(null);
            Xrm.Page.getAttribute("cvt_relateduserid").setRequiredLevel("none");
        }
    }
}
