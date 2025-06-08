//If the MCS namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.mcs_Component = {};
MCS.mcs_Component.Util = {};
MCS.mcs_Component.DisplayFields = {};

MCS.mcs_Component.FORM_TYPE_CREATE = 1;
MCS.mcs_Component.FORM_TYPE_UPDATE = 2;
MCS.mcs_Component.FORM_TYPE_READ_ONLY = 3;
MCS.mcs_Component.FORM_TYPE_DISABLED = 4;

MCS.mcs_Component.GenerateName = function (executionContext) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var formContext = executionContext.getFormContext();

    var derivedResultField = "";
    var componenttype = formContext.getAttribute("cvt_componenttype").getValue();
    var uniqueid = formContext.getAttribute("cvt_uniqueid").getValue();

    if (componenttype != null) {
        derivedResultField = ""
        derivedResultField += formContext.getAttribute("cvt_componenttype").getValue()[0].name;
    }

    if (uniqueid != null) {
        derivedResultField = ""
        derivedResultField += formContext.getAttribute("cvt_componenttype").getValue()[0].name;
        derivedResultField += ": "
        derivedResultField += uniqueid;
    }

    //SD: web-use-strict-equality-operators
    if (formContext.getAttribute("cvt_name").getValue() !== derivedResultField) {
        formContext.getAttribute("cvt_name").setValue(derivedResultField);
        formContext.getAttribute("cvt_name").setSubmitMode("always");
    }
};

MCS.mcs_Component.DisplayFields.init = function (executionContext, webResourceName) {
    //Retrieve the XML Web Resource specified by the parameter passed

    var globalContext = Xrm.Utility.getGlobalContext();
    var clientURL = globalContext.getClientUrl();
    MCS.mcs_Component.OtherComponentTypeSetDisplay(executionContext);
    MCS.mcs_Component.OtherManufacturerSetDisplay(executionContext);
    MCS.mcs_Component.OtherModelNumberSetDisplay(executionContext);
    MCS.mcs_Component.StatusSetDisplay(executionContext);

    var pathToWR = clientURL + "/WebResources/" + webResourceName;
    var xhr = new XMLHttpRequest();
    xhr.open("GET", pathToWR, true);
    xhr.setRequestHeader("Content-Type", "text/xml");
    xhr.onreadystatechange = function () { MCS.mcs_Component.DisplayFields.completeInitialization(executionContext, xhr); };
    xhr.send();
};
MCS.mcs_Component.DisplayFields.completeInitialization = function (executionContext, xhr) {
    var formContext = executionContext.getFormContext();
    //SD: web-use-strict-equality-operators
    if (xhr.readyState === 4 /* complete */) {
        //SD: web-use-strict-equality-operators
        if (xhr.status == 200) {
            xhr.onreadystatechange = null; //avoids memory leaks
            var JSConfig = [];
            //Get all configuration fields, starting with one
            var OptionSetFields = xhr.responseXML.documentElement.getElementsByTagName("LookupField");
            for (var i = 0; i < OptionSetFields.length; i++) {
                var OptionSetField = OptionSetFields[i];
                var mapping = {};
                mapping.fieldname = OptionSetField.getAttribute("id");
                mapping.type = [];
                //Get all Type nodes
                var ComponentTypes = MCS.mcs_Component.Util.selectNodes(OptionSetField, "ComponentType");

                //Loop through Type nodes
                for (var i = 0; i < ComponentTypes.length; i++) {
                    //Use current obj
                    var ComponentType = ComponentTypes[i];
                    //Create Type Obj to house field properties
                    var typeconfig = {};
                    typeconfig.id = ComponentType.getAttribute("id");
                    typeconfig.description = ComponentType.getAttribute("description");
                    typeconfig.fields = [];

                    //Get Field nodes
                    var fields = MCS.mcs_Component.Util.selectNodes(ComponentType, "Field");
                    //Loop through Field nodes
                    for (var a = 0; a < fields.length; a++) {
                        //Set the actual mappings                     
                        fieldproperties = {};
                        fieldproperties.id = fields[a].getAttribute("id");
                        fieldproperties.display = fields[a].getAttribute("display");

                        typeconfig.fields.push(fieldproperties);
                    }
                    mapping.type.push(typeconfig);
                }
                JSConfig.push(mapping);
            }
            //Attach the configuration object to DisplayFields so it will be available for the OnChange events 
            MCS.mcs_Component.DisplayFields.config = JSConfig;
            //Fire the onchange event for the mapped optionset fields so that the dependent fields are filtered for the current values.
            for (var configFile in MCS.mcs_Component.DisplayFields.config) {
                var fieldname = MCS.mcs_Component.DisplayFields.config[configFile].fieldname;
                formContext.data.entity.attributes.get(fieldname).fireOnChange();
            }
        }
    }
};
// This is the function set on the onchange event for parent fields
MCS.mcs_Component.DisplayFields.filterFieldVisibility = function (executionContext, fieldname) {
    var formContext = executionContext.getFormContext();
    for (var Nodes in MCS.mcs_Component.DisplayFields.config) {
        var Node = MCS.mcs_Component.DisplayFields.config[Nodes];
        /* Match the parameters to the correct field mapping ( in the XML to config object)*/
        //SD: web-use-strict-equality-operators
        if (Node.fieldname === fieldname) {
            var rootField = formContext.data.entity.attributes.get(fieldname);
            //Check for optionset value
            if (rootField.getValue()) {
                MCS.mcs_Component.fnMatch(executionContext, Node, "hiddenByDefault"); //hide all then make some visible.
                MCS.mcs_Component.fnMatch(executionContext, Node, rootField.getValue());
            }
        }
    }
};

MCS.mcs_Component.fnMatch = function (executionContext, Node, id) {
    var formContext = executionContext.getFormContext();
    //Get Types within Node
    for (var Types in Node.type) {
        var Type = Node.type[Types];
        var displayLabel = false;

        //Validation it is "hidden" or actual lookup with [0].name property
        var varMatch = "hiddenByDefault";
        if (id[0].id != null)
            varMatch = id[0].name;

        //Check if it matches
        //SD: web-use-strict-equality-operators
        if (Type.id === varMatch) {
            if (Type.fields) {
                for (var fields in Type.fields) {
                    //Change display for fields
                    var field = Type.fields[fields];
                    //Validate field exists, so no errors
                    if (formContext.data.entity.attributes.get(field.id)) {
                        try {
                            var actualfield = formContext.data.entity.attributes.get(field.id);
                            var actualfieldcontrol = actualfield.controls.get();
                            actualfieldcontrol = actualfieldcontrol[0];

                            //Map "hiddenByDefault" to show
                            //SD: web-use-strict-equality-operators
                            if (varMatch !== "hiddenByDefault") {
                                actualfieldcontrol.setVisible(true);
                                actualfieldcontrol.setDisabled(false);
                                displayLabel = true;
                            }
                            else {
                                actualfieldcontrol.setVisible(false);
                                actualfieldcontrol.setDisabled(true);
                                //actualfield.setValue(null);
                            }
                            actualfield.setSubmitMode("always");
                        }
                        catch (err) {
                            alert("Error with setting : " + field.id + "to display = " + field.display + ".\n" + err);
                        }
                    }
                }
            }

            var compType = formContext.getAttribute("cvt_componenttype");
            //SD: web-use-strict-equality-operators
            if (displayLabel === true) {
                var compTypeText = compType.getValue()[0].name;
                formContext.ui.tabs.get('component_details_tab').sections.get('section_tms').setLabel(compTypeText + " Specific fields");
            }
            else
                formContext.ui.tabs.get('component_details_tab').sections.get('section_tms').setLabel("");
        }
    }
};
//Helper methods to merge differences between browsers for this sample
MCS.mcs_Component.Util.selectSingleNode = function (node, elementName) {
    //SD: web-use-strict-equality-operators
    if (typeof (node.selectSingleNode) !== "undefined") {
        return node.selectSingleNode(elementName);
    }
    else {
        return node.getElementsByTagName(elementName)[0];
    }
};
MCS.mcs_Component.Util.selectNodes = function (node, elementName) {
    //SD: web-use-strict-equality-operators
    if (typeof (node.selectNodes) !== "undefined") {
        return node.selectNodes(elementName);
    }
    else {
        return node.getElementsByTagName(elementName);
    }
};

MCS.mcs_Component.DefaultManufacturerModel = function (executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    //Check for Models for this component Type.  If only 1, default it and manufacturer.  
    //If only 1 manufacturer but more than 1 model, default the manufacturer but not model.  
    var compType = formContext.getAttribute("cvt_componenttype");
    if (compType.getValue() == null || !compType.getIsDirty())
        return;
    var columns = ["cvt_modelId", "cvt_modelnumber", "cvt_Manufacturer", "cvt_Description"];
    var filter = "statecode/Value eq 0 and cvt_ComponentType/Id eq (Guid'" + formContext.getAttribute("cvt_componenttype").getValue()[0].id + "')";
    debugger;
    Xrm.WebApi.retrieveMultipleRecords("cvt_model", "?$select=cvt_modelId, cvt_modelnumber,cvt_Manufacturer,cvt_Description&$filter=" + filter).then(
        function success(results) {
            debugger;
            if (results && results.entities.length == 1) {
                //only 1 model, meaning only 1 manufacturer, so set both fields
                var model = [{
                    id: results.entities[0].cvt_modelId,
                    name: results.entities[0].cvt_modelnumber,
                    entityType: 'cvt_model'
                }];
                formContext.getAttribute("cvt_modelnumber").setValue(model);
                formContext.getAttribute("cvt_modelnumber").fireOnChange();
                var manufacturerObj = results.entities[0].cvt_Manufacturer;
                var manufacturer = [{
                    id: manufacturerObj.Id,
                    name: manufacturerObj.Name,
                    entityType: 'cvt_manufacturer'
                }];
                formContext.getAttribute("cvt_manufacturerid").setValue(manufacturer);
                formContext.getAttribute("cvt_manufacturerid").fireOnChange();
                formContext.getAttribute("cvt_modeldescription").setValue(results[0].cvt_Description);
            }
            else if (results && results.entities.length > 1) {
                //more than 1 model, maybe more than 1 manufacturer
                var multipleManufacturers = true;
                for (var i = 0; i < results.length; i++) {
                    //Compare all manufacturers to first.  If any 1 is different, do not set field, otherwise, use the first one in the set
                    //SD: web-use-strict-equality-operators
                    if (results[i].cvt_Manufacturer.Id !== results.entities[0].cvt_Manufacturer.Id)
                        multipleManufacturers = false;
                }
                if (multipleManufacturers) {
                    var manufacturer = [{
                        id: results.entities[0].cvt_Manufacturer.Id,
                        name: results.entities[0].cvt_Manufacturer.Name,
                        entityType: 'cvt_manufacturer'
                    }];
                    formContext.getAttribute("cvt_manufacturerid").setValue(manufacturer);
                    formContext.getAttribute("cvt_manufacturerid").fireOnChange();
                }
            }
        },
        function (error) {
            alert(MCS.cvt_Common.RestError(error));
        }
    );


    //var collectionResults = CrmRestKit.ByQuery('cvt_model', columns, filter);
    //collectionResults.fail(function (err) {
    //    alert(MCS.cvt_Common.RestError(err));
    //})
    //.done(function (collection) {
    //    var results = collection.d.results;
    //    if (results && results.length == 1) {
    //        //only 1 model, meaning only 1 manufacturer, so set both fields
    //        var model = [{
    //            id: results[0].cvt_modelId,
    //            name: results[0].cvt_modelnumber,
    //            entityType: 'cvt_model'
    //        }];
    //        Xrm.Page.getAttribute("cvt_modelnumber").setValue(model);
    //        Xrm.Page.getAttribute("cvt_modelnumber").fireOnChange();
    //        var manufacturerObj = results[0].cvt_Manufacturer;
    //        var manufacturer = [{
    //            id: manufacturerObj.Id,
    //            name: manufacturerObj.Name,
    //            entityType: 'cvt_manufacturer'
    //        }];
    //        Xrm.Page.getAttribute("cvt_manufacturerid").setValue(manufacturer);
    //        Xrm.Page.getAttribute("cvt_manufacturerid").fireOnChange();
    //        Xrm.Page.getAttribute("cvt_modeldescription").setValue(results[0].cvt_Description);
    //    }
    //    else if (results && results.length > 1) {
    //        //more than 1 model, maybe more than 1 manufacturer
    //        var multipleManufacturers = true;
    //        for (var i = 0; i < results.length; i++) {
    //            //Compare all manufacturers to first.  If any 1 is different, do not set field, otherwise, use the first one in the set
    //            if (results[i].cvt_Manufacturer.Id != results[0].cvt_Manufacturer.Id)
    //                multipleManufacturers = false;
    //        }
    //        if (multipleManufacturers) {
    //            var manufacturer = [{
    //                id: results[0].cvt_Manufacturer.Id,
    //                name: results[0].cvt_Manufacturer.Name,
    //                entityType: 'cvt_manufacturer'
    //            }];
    //            Xrm.Page.getAttribute("cvt_manufacturerid").setValue(manufacturer);
    //            Xrm.Page.getAttribute("cvt_manufacturerid").fireOnChange();
    //        }
    //    }
    //});

};

MCS.mcs_Component.PopulateModelDescription = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var model = formContext.getAttribute("cvt_modelnumber");
    if (model.getValue() == null || !model.getIsDirty())
        return;
    var id = model.getValue()[0].id;

    Xrm.WebApi.retrieveRecord("cvt_model", id, "?$select=cvt_Description").then(
        function success(result) {
            if (result != null && result.cvt_Description != null)
                formContext.getAttribute("cvt_modeldescription").setValue(result.cvt_Description);
        },
        function (error) {
            return;
        }
    );

    //CrmRestKit.Retrieve('cvt_model', id, ["cvt_Description"], false).fail(
    //    function (err) {
    //        return;
    //    }
    //    ).done(
    //    function (data) {
    //        if (data != null && data.d != null && data.d.cvt_Description != null)
    //            Xrm.Page.getAttribute("cvt_modeldescription").setValue(data.d.cvt_Description);
    //    });
};


MCS.mcs_Component.PopulateDescription = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var type = formContext.getAttribute("cvt_componenttype");
    if (type.getValue() == null || !type.getIsDirty())
        return;
    var id = formContext.getAttribute("cvt_componenttype").getValue()[0].id;

    Xrm.WebApi.retrieveRecord("cvt_componenttype", id, "?$select=cvt_Description").then(
        function success(result) {
            formContext.getAttribute("cvt_description").setValue(result.cvt_Description);
        },
        function (error) {
            return;
        }
    );

    //CrmRestKit.Retrieve('cvt_componenttype', id, ["cvt_Description"], false).fail(
    //    function (err) {
    //        return;
    //    }
    //    ).done(
    //    function (data) {
    //        Xrm.Page.getAttribute("cvt_description").setValue(data.d.cvt_Description);
    //    });
};

MCS.mcs_Component.AddLookupView = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //TODO - Filter Manufacturers to only ones where model contains current componentID
    //return;
    if (formContext.getAttribute("cvt_componenttype").getValue() == null)
        return;
    formContext.getControl("cvt_modelnumber").addPreSearch(function () {
        MCS.mcs_Component.AddModelLookupView(executionContext);
    });

    var type = formContext.getAttribute("cvt_componenttype").getValue()[0];
    var viewID = type.id;
    var TSAlayoutXML = '<grid name="resultset" object="10034" jump="mcs_name" select="1" icon="0" preview="0">' +
        '<row name="result" id="cvt_manufacturerid"><cell name="cvt_name" width="300"/>' +
        '</row></grid>';
    var fetch = '<fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true"><entity name="cvt_manufacturer">' +
        '<attribute name="cvt_manufacturerid" /><attribute name="cvt_name" /><order attribute="cvt_name" descending="false" />' +
        '<link-entity name="cvt_model" from="cvt_manufacturer" to="cvt_manufacturerid" alias="aa">' +
        '<filter type="and">' + '<condition attribute="cvt_componenttype" operator="eq" uiname="' +
        MCS.cvt_Common.formatXML(type.name) + '" uitype="cvt_componenttype" value="' + type.id + '" /></filter>' +
        '</link-entity>' +
        '</entity></fetch>';
    formContext.ui.controls.get("cvt_manufacturerid").addCustomView(viewID, "cvt_manufacturer", "Manufacturers that make this Component Type", fetch, TSAlayoutXML, true);

};

MCS.mcs_Component.AddModelLookupView = function (executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();
    ////Filter Models based on selected manufacturer and component Type
    if (formContext.getAttribute("cvt_componenttype").getValue() == null)
        return;
    var type = formContext.getAttribute("cvt_componenttype").getValue()[0];

    var componentTypeFilter = "<filter type='and'><condition attribute='cvt_componenttype' operator='eq' uiname='" +
        MCS.cvt_Common.formatXML(type.name) + "' uitype='cvt_componenttype' value='" + type.id + "' /></filter>";
    formContext.getControl("cvt_modelnumber").addCustomFilter(componentTypeFilter, "cvt_model");

};

MCS.mcs_Component.ComponentTypeOnchange = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var manufacturer = formContext.getAttribute("cvt_manufacturerid");
    var modelNumber = formContext.getAttribute("cvt_modelnumber");
    //if manufacturer is of Component type then keep value, otherwise clear both  
    if (manufacturer.getValue() == null || modelNumber.getValue() == null) {
        modelNumber.setValue(null);
        manufacturer.setValue(null);
        return;
    }

    var id = modelNumber.getValue()[0].id;
    Xrm.WebApi.retrieveRecord("cvt_model", id, "?$select=_cvt_ComponentType_value", "cvt_Manufacturer").then(
        function success(result) {
            if (result != null) {
                var pass = true;
                if (result.cvt_ComponentType != null) {
                    //SD: web-use-strict-equality-operators
                    if ("{" + result.cvt_ComponentType.Id.toUpperCase() + "}" !== formContext.getAttribute("_cvt_ComponentType_value").getValue()[0].id) {
                        //alert("{" + data.d.cvt_ComponentType.Id.toUpperCase() + "} : " + formContext.getAttribute("cvt_componenttype").getValue()[0].id);
                        pass = false;
                    }
                }
                else
                    pass = false;

                if (result.cvt_Manufacturer != null) {
                    //SD: web-use-strict-equality-operators
                    if ("{" + result.cvt_Manufacturer.Id.toUpperCase() + "}" !== manufacturer.getValue()[0].id) {
                        pass = false;
                    }
                }
                else
                    pass = false;

                //SD: web-use-strict-equality-operators
                if (pass === false) {
                    modelNumber.setValue(null);
                    manufacturer.setValue(null);
                }
            }

        },
        function (error) {
            console.log(error.message);
            // handle error conditions
        }
    );

    //CrmRestKit.Retrieve('cvt_model', id, ["cvt_ComponentType", "cvt_Manufacturer"], false).fail(
    //    function (err) {
    //        return;
    //    }
    //    ).done(
    //    function (data) {
    //        if (data != null && data.d != null) {
    //            var pass = true;
    //            if (data.d.cvt_ComponentType != null) {
    //                if ("{" + data.d.cvt_ComponentType.Id.toUpperCase() + "}" != formContext.getAttribute("cvt_componenttype").getValue()[0].id) {
    //                    //alert("{" + data.d.cvt_ComponentType.Id.toUpperCase() + "} : " + formContext.getAttribute("cvt_componenttype").getValue()[0].id);
    //                    pass = false;
    //                }
    //            }
    //            else
    //                pass = false;

    //            if (data.d.cvt_Manufacturer != null) {
    //                if ("{" + data.d.cvt_Manufacturer.Id.toUpperCase() + "}" != manufacturer.getValue()[0].id) {
    //                    pass = false;
    //                }
    //            }
    //            else
    //                pass = false;

    //            if (pass == false) {
    //                modelNumber.setValue(null);
    //                manufacturer.setValue(null);
    //            }
    //        }
    //    });
};

MCS.mcs_Component.ManufacturerOnchange = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var modelNumber = formContext.getAttribute("cvt_modelnumber");
    modelNumber.setValue(null);
    MCS.mcs_Component.OtherManufacturerSetDisplay(executionContext);
};

//OnLoad
MCS.mcs_Component.CheckForTSSResource = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //"Components should hook onto Technology systems.
    //To properly add a New Resouce
    var msg = "A Component must be created from a Technology Resource.\n\nPlease close this Component form and start from a Technology Resource.";
    //SD: web-use-strict-equality-operators
    if (formContext.ui.getFormType() === MCS.cvt_Common.FORM_TYPE_CREATE) {
        //Make field read-only, only on create
        formContext.getControl("cvt_relatedresourceid").setDisabled(true);

        //Check for TSS Resource
        if (formContext.getAttribute("cvt_relatedresourceid").getValue() == null) {
            //No TSS Resource - must be created from one.
            MCS.cvt_Common.closeWindow(executionContext, msg);
        }
        else {
            //Check that TSS Resource = Technology
            var id = formContext.getAttribute("cvt_relatedresourceid").getValue()[0].id;
            Xrm.WebApi.retrieveRecord("mcs_resource", id, "?$select=mcs_Type").then(
                function success(result) {
                    //SD: web-use-strict-equality-operators
                    if (result.mcs_Type.Value === 251920002) {
                        //ForceSubmit for TSS Resource
                        formContext.getAttribute("cvt_relatedresourceid").setSubmitMode('always');
                    }
                    else {
                        MCS.cvt_Common.closeWindow(executionContext, msg);
                    }
                },
                function (error) {
                    return;
                }
            );

            Xrm.WebApi.retrieveRecord("mcs_resource", id, "?$select=mcs_Type").then(
                function success(result) {
                    //SD: web-use-strict-equality-operators
                    if (result.mcs_Type.Value === 251920002) {
                        //ForceSubmit for TSS Resource
                        formContext.getAttribute("cvt_relatedresourceid").setSubmitMode('always');
                    }
                    else {
                        MCS.cvt_Common.closeWindow(executionContext, msg);
                    }
                },
                function (error) {
                    console.log(error.message);
                    // handle error conditions
                }
            );

            //CrmRestKit.Retrieve('mcs_resource', id, ["mcs_Type"], false).fail(
            //    function (err) {
            //        return;
            //    }
            //    ).done(
            //    function (data) {
            //        if (data.d.mcs_Type.Value == 251920002) {
            //            //ForceSubmit for TSS Resource
            //            formContext.getAttribute("cvt_relatedresourceid").setSubmitMode('always');
            //        }
            //        else {                       
            //            MCS.cvt_Common.closeWindow(msg);
            //        }
            //    });
        }
    }
};
//TO DO: Combine the next 3 functions into a single function where you pass in the field names
MCS.mcs_Component.OtherComponentTypeSetDisplay = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var src = formContext.getAttribute("cvt_componenttype");
    var dest = formContext.getAttribute("cvt_othercomponenttype");
    var destControl = formContext.getControl("cvt_othercomponenttype");
    if (src != null && destControl != null) {
        if (src.getValue() != null) {
            var srcObjValue = src.getValue();//Check for Lookup Value
            var lookupRecordName = srcObjValue[0].name; //To get record Name 
            //SD: web-use-strict-equality-operators
            if (lookupRecordName.toLowerCase() === "other") {
                dest.setRequiredLevel("required");
                destControl.setVisible(true);
                return;
            }
        }
        dest.setRequiredLevel("none");
        dest.setValue(null);
        destControl.setVisible(false);
    }
};

MCS.mcs_Component.OtherManufacturerSetDisplay = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var src = formContext.getAttribute("cvt_manufacturerid");
    var dest = formContext.getAttribute("cvt_othermanufacturer");
    var destControl = formContext.getControl("cvt_othermanufacturer");
    if (src != null && destControl != null) {
        if (src.getValue() != null) {
            var srcObjValue = src.getValue();//Check for Lookup Value
            var lookupRecordName = srcObjValue[0].name; //To get record Name 
            //SD: web-use-strict-equality-operators
            if (lookupRecordName.toLowerCase() === "other") {
                dest.setRequiredLevel("required");
                destControl.setVisible(true);
                return;
            }
        }
        dest.setRequiredLevel("none");
        dest.setValue(null);
        destControl.setVisible(false);
    }
};

MCS.mcs_Component.OtherModelNumberSetDisplay = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var src = formContext.getAttribute("cvt_modelnumber");
    var dest = formContext.getAttribute("cvt_othermodel");
    var destControl = formContext.getControl("cvt_othermodel");
    if (src != null && destControl != null) {
        if (src.getValue() != null) {
            var srcObjValue = src.getValue();//Check for Lookup Value
            var lookupRecordName = srcObjValue[0].name; //To get record Name 
            //SD: web-use-strict-equality-operators
            if (lookupRecordName.toLowerCase() === "other") {
                dest.setRequiredLevel("required");
                destControl.setVisible(true);
                return;
            }
        }
        dest.setRequiredLevel("none");
        dest.setValue(null);
        destControl.setVisible(false);
    }
};

MCS.mcs_Component.StatusSetDisplay = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var options = formContext.getControl("cvt_status");
    if (options != null) {
        //SD: web-use-strict-equality-operators
        if (formContext.getAttribute("cvt_status").getValue() !== 917290002)
            options.removeOption(917290002);
        //SD: web-use-strict-equality-operators
        if (formContext.getAttribute("cvt_status").getValue() !== 917290003)
            options.removeOption(917290003);
    }
};
