﻿/*
Venkat - toJSON method is default function now

  if (!this.JSON) { this.JSON = {}; } (function () { function f(n) { return n < 10 ? '0' + n : n; } if (typeof Date.prototype.toJSON !== 'function') { Date.prototype.toJSON = function (key) { return isFinite(this.valueOf()) ? this.getUTCFullYear() + '-' + f(this.getUTCMonth() + 1) + '-' + f(this.getUTCDate()) + 'T' + f(this.getUTCHours()) + ':' + f(this.getUTCMinutes()) + ':' + f(this.getUTCSeconds()) + 'Z' : null; }; String.prototype.toJSON = Number.prototype.toJSON = Boolean.prototype.toJSON = function (key) { return this.valueOf(); }; } var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, escapable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, gap, indent, meta = { '\b': '\\b', '\t': '\\t', '\n': '\\n', '\f': '\\f', '\r': '\\r', '"': '\\"', '\\': '\\\\' }, rep; function quote(string) { escapable.lastIndex = 0; return escapable.test(string) ? '"' + string.replace(escapable, function (a) { var c = meta[a]; return typeof c === 'string' ? c : '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4); }) + '"' : '"' + string + '"'; } function str(key, holder) { var i, k, v, length, mind = gap, partial, value = holder[key]; if (value && typeof value === 'object' && typeof value.toJSON === 'function') { value = value.toJSON(key); } if (typeof rep === 'function') { value = rep.call(holder, key, value); } switch (typeof value) { case 'string': return quote(value); case 'number': return isFinite(value) ? String(value) : 'null'; case 'boolean': case 'null': return String(value); case 'object': if (!value) { return 'null'; } gap += indent; partial = []; if (Object.prototype.toString.apply(value) === '[object Array]') { length = value.length; for (i = 0; i < length; i += 1) { partial[i] = str(i, value) || 'null'; } v = partial.length === 0 ? '[]' : gap ? '[\n' + gap + partial.join(',\n' + gap) + '\n' + mind + ']' : '[' + partial.join(',') + ']'; gap = mind; return v; } if (rep && typeof rep === 'object') { length = rep.length; for (i = 0; i < length; i += 1) { k = rep[i]; if (typeof k === 'string') { v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ': ' : ':') + v); } } } } else { for (k in value) { if (Object.hasOwnProperty.call(value, k)) { v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ': ' : ':') + v); } } } } v = partial.length === 0 ? '{}' : gap ? '{\n' + gap + partial.join(',\n' + gap) + '\n' + mind + '}' : '{' + partial.join(',') + '}'; gap = mind; return v; } } if (typeof JSON.stringify !== 'function') { JSON.stringify = function (value, replacer, space) { var i; gap = ''; indent = ''; if (typeof space === 'number') { for (i = 0; i < space; i += 1) { indent += ' '; } } else if (typeof space === 'string') { indent = space; } rep = replacer; if (replacer && typeof replacer !== 'function' && (typeof replacer !== 'object' || typeof replacer.length !== 'number')) { throw new Error('JSON.stringify'); } return str('', { '': value }); }; } if (typeof JSON.parse !== 'function') { JSON.parse = function (text, reviver) { var j; function walk(holder, key) { var k, v, value = holder[key]; if (value && typeof value === 'object') { for (k in value) { if (Object.hasOwnProperty.call(value, k)) { v = walk(value, k); if (v !== undefined) { value[k] = v; } else { delete value[k]; } } } } return reviver.call(holder, key, value); } text = String(text); cx.lastIndex = 0; if (cx.test(text)) { text = text.replace(cx, function (a) { return '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4); }); } if (/^[\],:{}\s]*$/.test(text.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, '@').replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, ']').replace(/(?:^|:|,)(?:\s*\[)+/g, ''))) { j = eval('(' + text + ')'); return typeof reviver === 'function' ? walk({ '': j }, '') : j; } throw new SyntaxError('JSON.parse'); }; } }());
 */


/*globales Xrm, $ */

///
/// AlfaPeople CRM 2011 CrmRestKit
///
/// Based on 'MSCRM4 Web Service Toolkit for JavaScript v2.1' (http://crmtoolkit.codeplex.com/releases/view/48329)
/// and XrmSvcTookit 'http://xrmsvctoolkit.codeplex.com/
///

/// Credits:
///     Daniel Cai (getClientUrl, associate, disassociate)
///     Matt (https://www.codeplex.com/site/users/view/MattMatt)
///
/// @author:
///     Daniel Rene Thul, drt@alfapeople.com
///
/// @version:
///     2.6.1
///
/// requires (jquery.1.7.2.js, JSON2.js)
///
//Venkate - Commented as its not being used
/*
var CrmRestKit = (function (window, document, undefined) {
    'use strict';
 
    ///
    /// Private members
    ///
    var ODATA_ENDPOINT = "/XRMServices/2011/OrganizationData.svc",
        version = '2.6.0';
 
    ///
    /// Private function to the context object.
    ///
    function getContext() {
 
        if (typeof GetGlobalContext !== "undefined") {
           // /*ignore jslint start
            return GetGlobalContext();
           // /*ignore jslint end
 
        }
        else {
 
            if (typeof Xrm !== "undefined") {
                return Xrm.Page.context;
            }
            else {
 
                throw new Error("Context is not available.");
            }
        }
    }
 
    ///
    /// Private function to return the server URL from the context
    ///
    function getClientUrl() {
 
        var url = null,
            localServerUrl = window.location.protocol + "//" + window.location.host,
            context = getContext();
 
 
        if (Xrm.Page.context.getClientUrl !== undefined) {
            // since version SDK 5.0.13 
            // http://www.magnetismsolutions.com/blog/gayan-pereras-blog/2013/01/07/crm-2011-polaris-new-xrm.page-method
 
            url = Xrm.Page.context.getClientUrl();
        }
        else if (context.isOutlookClient() && !context.isOutlookOnline()) {
            url = localServerUrl;
        }
        else {
            url = context.getClientUrl();
            url = url.replace(/^(http|https):\/\/([_a-zA-Z0-9\-\.]+)(:([0-9]{1,5}))?/, localServerUrl);
            url = url.replace(/\/$/, "");
        }
        return url;
    }
 
    ///
    /// Private function to return the path to the REST endpoint.
    ///
    function getODataPath() {
 
        return getClientUrl() + ODATA_ENDPOINT;
    }
 
    ///
    /// Returns an object that reprensts a entity-reference 
    ///
    function entityReferenceFactory(id, opt_logicalName) {
 
        var reference = null;
 
        if (id !== undefined && id !== null) {
 
            reference = {
                __metadata: { type: "Microsoft.Crm.Sdk.Data.Services.EntityReference" },
                Id: id
            };
 
            if (opt_logicalName !== undefined && opt_logicalName !== null) {
 
                reference.LogicalName = opt_logicalName;
            }
        }
 
        return reference;
    }
 
    ///
    /// Returns an object that reprensts a option-set-value 
    ///
    function optionSetValueFactory(option_value) {
 
        return {
            __metadata: { type: 'Microsoft.Crm.Sdk.Data.Services.OptionSetValue' },
            Value: option_value
        };
    }
 
    ///
    /// Returns an object that represents an money value
    ///
    function moneyValueFactory(value) {
 
        return {
            __metadata: { type: 'Microsoft.Crm.Sdk.Data.Services.Money' },
            Value: value
        };
    }
 
    ///
    /// Parses the ODATA date-string into a date-object
    /// All queries return a date in the format "/Date(1368688809000)/"
    /// 
    function parseODataDate(value) {
 
        return new Date(parseInt(value.replace('/Date(', '').replace(')/', ''), 10));
    }
 
    ///
    /// Generics ajax-call funciton. Returns a promise object
    ///
    function doRequest(options, asyn) {
 
        // default values for the ajax queries
        var ajaxDefaults = {
            type: "GET",
            async: true,
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            beforeSend: function (request) {
 
                request.setRequestHeader("Accept", "application/json");
            }
        };
 
        // merge the default-settings with the options-object
        options = $.extend(ajaxDefaults, options);
 
        // request could be executed in sync or asyn mode
        options.async = (asyn === undefined) ? true : asyn;
 
        return $.ajax(options);
    }
 
    ///
    /// Creates a link between records 
    ///
    function associate(entity1Id, entity1Name, entity2Id, entity2Name, relationshipName, opt_asyn) {
 
        // default is 'true'
        var asyn = (opt_asyn === undefined) ? true : opt_asyn,
            odatapath = getODataPath(),
            request = {
                url: odatapath + "/" + entity1Name + "Set(guid'" + entity1Id + "')/$links/" + relationshipName,
                type: "POST",
                data: window.JSON.stringify({
                    uri: odatapath + "/" + entity2Name + "Set(guid'" + entity2Id + "')"
                })
            };
 
        return doRequest(request, asyn);
    }
 
    ///
    /// Removes a link between records 
    ///
    function disassociate(entity1Id, entity1Name, entity2Id, relationshipName, opt_asyn) {
 
        var asyn = (opt_asyn === undefined) ? true : opt_asyn,
            odatapath = getODataPath(),
            request = {
                url: odatapath + "/" + entity1Name + "Set(guid'" + entity1Id + "')/$links/" + relationshipName + "(guid'" + entity2Id + "')",
                type: "POST",
                // method: "DELETE",
                beforeSend: function (request) {
                    request.setRequestHeader('Accept', 'application/json');
                    request.setRequestHeader('X-HTTP-Method', 'DELETE');
                }
            };
 
        return doRequest(request, asyn);
    }
 
    ///
    /// Retrieves a single record 
    ///
    function retrieve(entityName, id, columns, opt_asyn) {
 
        // default is 'true'
        var asyn = (opt_asyn === undefined) ? true : opt_asyn,
            setName = entityName + 'Set',
            query = getODataPath() + "/" + setName + "(guid'" + id + "')" + "?$select=" + columns.join(',');
 
        // returns a promise instance
        return doRequest({ url: query }, asyn);
    }
 
    ///
    /// Used in the context of lazy-loading (more than 50 records found in the retrieveMultiple request)
    /// Query (url) needs to define the entity, columns and filter
    ///
    function byQueryUrl(queryUrl, opt_asyn) {
 
        return doRequest({ url: queryUrl }, opt_asyn);
    }
 
    ///
    /// Used for joins
    ///
    function byExpandQuery(entityName, columns, expand, filter, opt_asyn) {
 
        // default is 'true'
        var asyn = (opt_asyn === undefined) ? true : opt_asyn;
 
        // in case filter is empty 
        filter = (filter) ? "&$filter=" + encodeURIComponent(filter) : '';
 
        // create defered object
        var setName = entityName + 'Set',
            query = getODataPath() + "/" + setName + "?$select=" + columns.join(',') + '&$expand=' + expand + filter;
 
        return doRequest({ url: query }, asyn);
    }
 
    ///
    /// Retrievs multiuple records based on filter
    /// The max number of records returned by Odata is limited to 50, the result object contains the property 
    /// 'next' and the fn loadNext that could be used to load the addional records 
    ///
    function byQuery(entityName, columns, filter, opt_asyn) {
 
        // default is 'true'
        var asyn = (opt_asyn === undefined) ? true : opt_asyn;
 
        // in case filter is empty 
        filter = (filter) ? "&$filter=" + encodeURIComponent(filter) : '';
 
        // create defered object
        var setName = entityName + 'Set',
            query = getODataPath() + "/" + setName + "?$select=" + columns.join(',') + filter;
 
        return doRequest({ url: query }, asyn);
    }
 
    ///
    /// Per default a REST query returns only 50 record. This function will load all records
    ///
    function byQueryAll(entityName, columns, filter, opt_asyn) {
 
        var dfdAll = new $.Deferred(),
            allRecords = [];
 
        byQuery(entityName, columns, filter, opt_asyn).then(function byQueryAllSuccess(result) {
 
            // add the elements to the collection
            allRecords = allRecords.concat(result.d.results);
 
            if (result.d.__next) {
 
                // the success-handler will be this function
                byQueryUrl(result.d.__next, opt_asyn).then(byQueryAllSuccess, dfdAll.reject);
 
                // call the progressCallbacks of the promise
                dfdAll.notify(result);
            }
            else {
                dfdAll.resolve(allRecords);
            }
 
        }, dfdAll.reject);
 
        return dfdAll.promise();
    }
 
    ///
    /// Create a single reocrd
    ///
    function created(entityName, entityObject, opt_asyn) {
 
        // default is 'true'
        var asyn = (opt_asyn === undefined) ? true : opt_asyn,
            setName = entityName + 'Set',
            json = window.JSON.stringify(entityObject),
            query = getODataPath() + "/" + setName;
 
        // returns a promise object
        return doRequest({ type: "POST", url: query, data: json }, asyn);
    }
 
    ///
    /// Updates the record with the stated intance.
    /// MERGE methode does not return data
    ///
    /// Sample:
    ///     CrmRestKit.Update('Account', id, { 'Address1_City': 'sample', 'Name': 'sample' }).done(...).fail(..)
    ///
    function update(entityName, id, entityObject, opt_asyn) {
 
        // default is 'true'
        var asyn = (opt_asyn === undefined) ? true : opt_asyn,
            setName = entityName + 'Set',
            json = window.JSON.stringify(entityObject),
            query = getODataPath() + "/" + setName + "(guid'" + id + "')",
            // ajax-call-options
            options = {
                type: "POST",
                url: query,
                data: json,
                beforeSend: function (request) {
                    request.setRequestHeader("Accept", "application/json");
                    request.setRequestHeader("X-HTTP-Method", "MERGE");
                }
            };
 
        // MERGE methode does not return data
        return doRequest(options, asyn);
    }
 
    ///
    /// Deletes as single record identified by the id
    /// Sample:
    ///         CrmRestKit.Delete('Account', id).done(...).fail(..);
    ///
    function deleteRecord(entityName, id, opt_asyn) {
 
        // default is 'true'
        var asyn = (opt_asyn === undefined) ? true : opt_asyn,
            setName = entityName + 'Set',
            query = getODataPath() + '/' + setName + "(guid'" + id + "')",
            options = {
                type: "POST",
                url: query,
                beforeSend: function (request) {
                    request.setRequestHeader('Accept', 'application/json');
                    request.setRequestHeader('X-HTTP-Method', 'DELETE');
                }
            };
 
        return doRequest(options, asyn);
    }
 
    ///
    /// Public API
    ///
    return {
        Version: version,
       // /* Read /retrieve methods
        Retrieve: retrieve,
        ByQuery: byQuery,
        ByQueryUrl: byQueryUrl,
        ByExpandQuery: byExpandQuery,
        ByQueryAll: byQueryAll,
       // /* C U D 
        Create: created,
        Update: update,
        Delete: deleteRecord,
       // /* N:M relationship operations 
        Associate: associate,
        Disassociate: disassociate,
       // /* Factory methods * /
        EntityReferenceFactory: entityReferenceFactory,
        OptionSetValueFactory: optionSetValueFactory,
        MoneyValueFactory: moneyValueFactory,
        ///* util methods * /
        ParseODataDate: parseODataDate
    };
}(window, document));
*/






//Library Name: cvt_CommonFunctions.js
//If the SDK namespace object is not defined, create it.
if (typeof MCS === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
if (typeof MCS.cvt_Common === "undefined") { MCS.cvt_Common = {}; }

//Form Types
MCS.cvt_Common.FORM_TYPE_CREATE = 1;
MCS.cvt_Common.FORM_TYPE_UPDATE = 2;
MCS.cvt_Common.FORM_TYPE_READ_ONLY = 3;
MCS.cvt_Common.FORM_TYPE_DISABLED = 4;
MCS.cvt_Common.FORM_TYPE_QUICKCREATE = 5;
MCS.cvt_Common.FORM_TYPE_BULKEDIT = 6;

MCS.cvt_Common.BlankGUID = "00000000-0000-0000-0000-000000000000";

MCS.cvt_Common.AppointmentOccursInPast = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() === MCS.cvt_Common.FORM_TYPE_CREATE)
        return false;
    var startTimeObj = formContext.getAttribute("scheduledstart");
    if (startTimeObj == null)
        return false;
    var startTime = startTimeObj.getValue();
    if (startTime == null)
        return false;
    var now = new Date();
    if (now > startTime)
        return true;
    else
        return false;
};

//Get Server URL
MCS.cvt_Common.BuildRelationshipServerUrl = function () {
    var globalContext = Xrm.Utility.getGlobalContext();
    var server = globalContext.getClientUrl();
    // var server = Xrm.Page.context.getClientUrl();
    if (server.match(/\/$/)) {
        server = server.substring(0, server.length - 1);
    }
    return server;
};

//Check if Obj is null else get Value
MCS.cvt_Common.checkNull = function (executionContext, fieldname) {
    var formContext = executionContext.getFormContext();
    var fieldObj = formContext.getAttribute(fieldname);

    if (fieldObj != null)
        return fieldObj.getValue();

    return null;
};

//Close window
MCS.cvt_Common.closeWindow = function (executionContext, msg) {
    var formContext = executionContext.getFormContext();
    if (msg != null)
        alert(msg);
    //Clear all fields so there are no dirty fields
    var attributes = formContext.data.entity.attributes.get();
    for (var i in attributes) {
        attributes[i].setSubmitMode("never");
    }
    //Close record         
    formContext.ui.close();
};

MCS.cvt_Common.fireChange = function (executionContext, field) {
    var formContext = executionContext.getFormContext();
    /*********************************************************************
    USAGE:  Causes 'onChange' event to fire on a related field.  Typically
    would be called to initiate onChange event for a field changed 
    programmatically (and which would not have a "real" onChange fired)
    *********************************************************************/
    var ctlControl = formContext.getControl(field);

    formContext.getAttribute(ctlControl).fireOnChange();

}

//collapse a tab
MCS.cvt_Common.collapseTab = function (executionContext, tab, field) {
    var formContext = executionContext.getFormContext();
    /*********************************************************************
    USAGE: collapses/expands a tab based upon whether a control is empty.
    Pass in the schema name of the tab and the name of the field to check
 
    Example:
    //tab name: "tab_9"  <--schema name is what we want passed in
    //mcs_relatedtsa  <--pass in the field name to check
 
    **********************************************************************/

    var ctlControl = formContext.getControl(field);
    var atrControl = ctlControl.getAttribute();
    var valControl = atrControl.getValue();

    var tabObj = formContext.ui.tabs.get(tab);

    if (valControl !== "" && valControl != null) {
        tabObj.setDisplayState("expanded");
    }
    else {
        tabObj.setDisplayState("collapsed");
    }
};

MCS.cvt_Common.collapse2Tab = function (executionContext, tab1, tab2) {
    var formContext = executionContext.getFormContext();
    /*********************************************************************
    USAGE: collapses/expands a tab based upon whether a control is empty.
    Pass in the schema name of the tab and the name of the field to check
 
    Example:
    //tab name: "tab_9"  <--schema name is what we want passed in
    //mcs_relatedtsa  <--pass in the field name to check
 
    **********************************************************************/
    var field = "serviceid";
    var ctlControl = formContext.getControl(field);
    var atrControl = ctlControl.getAttribute();
    var valControl = atrControl.getValue();

    var tabObj1 = formContext.ui.tabs.get(tab1);
    var tabObj2 = formContext.ui.tabs.get(tab2);


    if (valControl !== "" && valControl != null) {
        tabObj1.setDisplayState("expanded");
        tabObj2.setVisible(false);
    }
    else {
        tabObj1.setDisplayState("collapsed");
        tabObj2.setVisible(true);
    }
};


//Check if GUIDS are the same
MCS.cvt_Common.compareGUIDS = function (guid1, guid2) {
    if (guid1 == null && guid2 == null)
        return true;

    if (guid1 == null || guid2 == null)
        return false;

    var guid1Cleaned = guid1.replace(/\W/g, '');
    guid1Cleaned = guid1Cleaned.toString().toUpperCase();

    var guid2Cleaned = guid2.replace(/\W/g, '');
    guid2Cleaned = guid2Cleaned.toString().toUpperCase();

    if (guid1Cleaned === guid2Cleaned)
        return true;
    else
        return false;
};


//Venkat Comment- changed method with WebApi
//Change a Record's Status
MCS.cvt_Common.changeRecordStatus = function (executionContext, recordId, entityLogicalName, statecode, statuscode) {

    //Remove brackets from the GUID if there’s any
    var id = recordId.replace("{", "").replace("}", "");
    // Set statecode and statuscode
    var data = {
        "statecode": statecode,
        "statuscode": statuscode
    };
    // WebApi call
    Xrm.WebApi.updateRecord(entityLogicalName, id, data).then(
        function success(result) {
            executionContext.getFormContext().data.refresh(true);
        },
        function (error) {
            alert(error);
        });
};

/*
//Change a Record's Status
MCS.cvt_Common.changeRecordStatus = function (executionContext, RECORD_ID, Entity_Name, stateCode, statusCode) {
    var formContext = executionContext.getFormContext();
    var globalContext = Xrm.Utility.getGlobalContext();
    var url = globalContext.getClientUrl();
    //var url = Xrm.Page.context.getClientUrl();
 
    // create the SetState request
    var request = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">";
    request += "<s:Body>";
    request += "<Execute xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">";
    request += "<request i:type=\"b:SetStateRequest\" xmlns:a=\"http://schemas.microsoft.com/xrm/2011/Contracts\" xmlns:b=\"http://schemas.microsoft.com/crm/2011/Contracts\">";
    request += "<a:Parameters xmlns:c=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">";
    request += "<a:KeyValuePairOfstringanyType>";
    request += "<c:key>EntityMoniker</c:key>";
    request += "<c:value i:type=\"a:EntityReference\">";
    request += "<a:Id>" + RECORD_ID + "</a:Id>";
    request += "<a:LogicalName>" + Entity_Name + "</a:LogicalName>";
    request += "<a:Name i:nil=\"true\" />";
    request += "</c:value>";
    request += "</a:KeyValuePairOfstringanyType>";
    request += "<a:KeyValuePairOfstringanyType>";
    request += "<c:key>State</c:key>";
    request += "<c:value i:type=\"a:OptionSetValue\">";
    request += "<a:Value>" + stateCode + "</a:Value>";
    request += "</c:value>";
    request += "</a:KeyValuePairOfstringanyType>";
    request += "<a:KeyValuePairOfstringanyType>";
    request += "<c:key>Status</c:key>";
    request += "<c:value i:type=\"a:OptionSetValue\">";
    request += "<a:Value>" + statusCode + "</a:Value>";
    request += "</c:value>";
    request += "</a:KeyValuePairOfstringanyType>";
    request += "</a:Parameters>";
    request += "<a:RequestId i:nil=\"true\" />";
    request += "<a:RequestName>SetState</a:RequestName>";
    request += "</request>";
    request += "</Execute>";
    request += "</s:Body>";
    request += "</s:Envelope>";
    //send set state request
    $.ajax({
        type: "POST",
        contentType: "text/xml; charset=utf-8",
        datatype: "xml",
        url: url + "/XRMServices/2011/Organization.svc/web",
        data: request,
        beforeSend: function (XMLHttpRequest) {
            XMLHttpRequest.setRequestHeader("Accept", "application/xml, text/xml, * /*");
            XMLHttpRequest.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
        },
        success: function (data, textStatus, XmlHttpRequest) {
            formContext.data.refresh();
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });
}; */
//Venkat- Ends here

//Create Fetch
MCS.cvt_Common.CreateFetch = function (entityName, columns, conditions, order) {
    var formattedColumns = '';
    var formattedConditions = '';
    var formattedOrder = '';

    //columns is an array, so that we can build that string with the xml tags
    if (columns != null && columns.length > 0) {
        for (column in columns) {
            formattedColumns += '<attribute name="' + columns[column] + '" />';
        }
    }
    //prefix filter type and add conditions
    if (conditions != null && conditions.length > 0) {
        formattedConditions = "<filter type='and'>";
        for (condition in conditions) {
            formattedConditions += conditions[condition];
        }
    }
    //format order
    if (order !== null && order.length == 2)
        formattedOrder = '<order attribute="' + order[0] + '" descending="' + order[1] + '" />';

    var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true' aggregate='false'>";
    fetchXml += "<entity name='" + entityName + "'>";
    fetchXml += formattedColumns;
    fetchXml += formattedOrder;
    fetchXml += formattedConditions;
    fetchXml += "</filter>";
    fetchXml += "</entity>";
    fetchXml += "</fetch>";

    return fetchXml;
};

MCS.cvt_Common.DateTime = function (executionContext, attributeName, hour, minute) {
    var formContext = executionContext.getFormContext();
    var attribute = formContext.getAttribute(attributeName);
    if (attribute.getValue() == null) {
        attribute.setValue(new Date());
    }
    attribute.setValue(attribute.getValue().setHours(hour, minute, 0));
};

//Used for Specialty Subtype based off of Subtype
MCS.cvt_Common.EnableDependentLookup = function (executionContext, primaryLU, secondaryLU) {
    var formContext = executionContext.getFormContext();
    var primaryLUattribute = formContext.getAttribute(primaryLU);
    var primaryLUvalue = primaryLUattribute != null ? primaryLUattribute.getValue() : null;
    var primaryLUvalueproperty = primaryLUvalue != null ? primaryLUvalue[0].name : null;

    if (primaryLUvalueproperty != null) {
        formContext.getControl(secondaryLU).setVisible(true);
        formContext.getControl(secondaryLU).setFocus();
    }
    else {
        formContext.getControl(secondaryLU).setVisible(false);
        formContext.getAttribute(secondaryLU).setValue(null);
    }
};

MCS.cvt_Common.EnableOtherDetails = function (executionContext, source, target, value) {
    var formContext = executionContext.getFormContext();
    var targetFieldControl = formContext.ui.controls.get(target);
    var targetFieldObject = formContext.getAttribute(target);
    var sourceValue = formContext.getAttribute(source).getValue();
    if (sourceValue !== null && sourceValue.toString() == value) {
        targetFieldControl.setDisabled(false);
        targetFieldControl.setVisible(true);
        targetFieldObject.setRequiredLevel("required");
        targetFieldObject.setSubmitMode("dirty");
    }
    else {
        if (targetFieldObject.getValue() !== "") {
            targetFieldObject.setValue("");
            targetFieldObject.setSubmitMode("always");
        }
        targetFieldControl.setDisabled(true);
        targetFieldControl.setVisible(false);
        targetFieldObject.setRequiredLevel("none");
    }
};

//XML Fix - replace & with &amp;
MCS.cvt_Common.formatXML = function (str) {
    if (str) {
        str = str.replace(/&/g, "&amp;");
        return str;
    }
};

//Gets the EntityTypeCode / ObjectTypeCode of a entity
MCS.cvt_Common.getObjectTypeCode = function (entityName) {
    var lookupService = new parent.RemoteCommand("LookupService", "RetrieveTypeCode");
    lookupService.SetParameter("entityName", entityName);
    var result = lookupService.Execute();
    if (result.Success && typeof result.ReturnValue === "number") {
        return result.ReturnValue;
    } else {
        return null;
    }
};

//MCS.cvt_Common.JSDebugAlert = function (msg) {
//    Set showAlerts to false to stop showing Alerts
//    var showAlerts = false;

//    if (showAlerts == true) {
//        if (msg != null) {
//            alert("JS Debug Message: \n\n" + msg);
//        }
//    }
//};

MCS.cvt_Common.MVIConfig = function () {
    var deferred = $.Deferred();
    var returnData = {
        success: false,
        data: {}
    };
    var roles = "";
    var MVIConfig = false;
    var filter = "mcs_name eq 'Active Settings'";
    Xrm.WebApi.retrieveMultipleRecords("mcs_setting", "?$select=cvt_usemvi,cvt_mviroles&$filter=" + filter).then(
        function success(result) {
            if (result !== null && result.entities.length != 0) {
                MVIConfig = result.entities[0].cvt_usemvi != null ? result.entities[0].cvt_usemvi : false;
                roles = result.entities[0].cvt_mviroles;
            }
            // var roleCheck = MCS.cvt_Common.userHasRoleInList(roles);
            var roleCheckretrieveTokenDeferred = MCS.cvt_Common.userHasRoleInList(roles);
            $.when(roleCheckretrieveTokenDeferred).done(function (returnData) {
                //return MVIConfig && roleCheck;
                var roleCheck = returnData.data.result;
                returnData.success = true;
                returnData.data.result = MVIConfig && roleCheck;
                deferred.resolve(returnData);
            },
                function (error) {
                    //return MVIConfig;
                    returnData.success = false;
                    deferred.resolve(returnData);

                }
            );

        }
    );
    return deferred.promise();
};


//UNSUPPORTED: Add Message to Notifications area
MCS.cvt_Common.Notifications = function (action, icon, message) {
    var notificationsList = Sys.Application.findComponent('crmNotifications');

    switch (action) {
        case "Add":
            if (notificationsList && icon && message)
                notificationsList.AddNotification('noteId1', icon, 'namespace', message);
            break;
        case "Hide":
            notificationsList.SetVisible(false);
            break;
    }
};

MCS.cvt_Common.openDialogOnCurrentRecord = function (primaryControl, dialogId) {
    var formContext = primaryControl.getFormContext();
    EntityName = formContext.data.entity.getEntityName();
    objectId = formContext.data.entity.getId();
    return MCS.cvt_Common.openDialogProcess(primaryControl, dialogId, EntityName, objectId);
};

MCS.cvt_Common.openDialogProcess = function (primaryControl, dialogId, EntityName, objectId) {
    var formContext = primaryControl.getFormContext();
    if (EntityName === null || EntityName == "")
        EntityName = formContext.data.entity.getEntityName();
    if (objectId === null || objectId == "")
        objectId = formContext.data.entity.getId();
    var globalContext = Xrm.Utility.getGlobalContext();
    var url = globalContext.getClientUrl() +
        //var url = Xrm.Page.context.getClientUrl() +
        "/cs/dialog/rundialog.aspx?DialogId=" +
        dialogId + "&EntityName=" +
        EntityName + "&ObjectId=" +
        objectId;
    var width = 400;
    var height = 400;
    var left = (screen.width - width) / 2;
    var top = (screen.height - height) / 2;
    return window.open(url, '', 'location=0,menubar=1,resizable=1,width=' + width + ',height=' + height + ',top=' + top + ',left=' + left + '');
};

MCS.cvt_Common.RestError = function (err) {
    return JSON.parse(err.responseText).error.message.value;
};

//From the Site, Set Facility
MCS.cvt_Common.SetFacilityFromSite = function (executionContext, siteFieldName, facilityFieldName) {
    var formContext = executionContext.getFormContext();
    var siteField = formContext.getAttribute(siteFieldName);
    var facilityField = formContext.getAttribute(facilityFieldName);
    var priorFacilityValue = facilityField.getValue() != null ? facilityField.getValue()[0].id : null;
    var siteValue = siteField.getValue() != null ? siteField.getValue()[0].id : null;

    if (siteValue != null) {
        //Get Parent Facility of Site

        Xrm.WebApi.retrieveRecord("mcs_site", siteValue, "?$select=mcs_FacilityId").then(
            function success(result) {
                if (result && result.mcs_FacilityId) {
                    //Check and Set Facility
                    var value = new Array();
                    value[0] = new Object();
                    value[0].id = '{' + result.mcs_FacilityId.Id + '}';
                    value[0].name = result.mcs_FacilityId.Name;
                    value[0].entityType = "mcs_facility";

                    //Set Facility field
                    facilityField.setValue(value);
                }
            },
            function (error) {
            }
        );

        //var calls = CrmRestKit.Retrieve("mcs_site", siteValue, ['mcs_FacilityId'], false);
        //calls.fail(
        //        function (error) {
        //        }).done(function (data) {
        //            if (data && data.d && data.d.mcs_FacilityId) {
        //                //Check and Set Facility
        //                var value = new Array();
        //                value[0] = new Object();
        //                value[0].id = '{' + data.d.mcs_FacilityId.Id + '}';
        //                value[0].name = data.d.mcs_FacilityId.Name;
        //                value[0].entityType = "mcs_facility";

        //                //Set Facility field
        //                facilityField.setValue(value);
        //            }
        //        });
    }
    else {
        //Clear Facility field
        facilityField.setValue(null);
    }
    if (MCS.cvt_Common.compareGUIDS(priorFacilityValue, ((facilityField.getValue() !== null) ? facilityField.getValue()[0].id : null)) != true)
        facilityField.setSubmitMode("always");
};

MCS.cvt_Common.TrimBookendBrackets = function (stringVar) {
    if (stringVar != null && stringVar.length > 0)
        return stringVar.charAt(0) === '{' ? stringVar.slice(1, stringVar.length - 1) : stringVar;
    else
        return "";
};


if (typeof jQuery !== 'undefined' && typeof $ !== 'undefined') {
    if (jQuery.when.all === undefined) {
        jQuery.when.all = function (deferreds) {
            var deferred = new jQuery.Deferred();
            $.when.apply(jQuery, deferreds).then(
                function () {
                    deferred.resolve(Array.prototype.slice.call(arguments));
                },
                function () {
                    deferred.fail(Array.prototype.slice.call(arguments));
                });

            return deferred;
        }
    }
}

//Check if the passed in User has a particular role
MCS.cvt_Common.userHasRoleInList = function (roles) {
    var deferred = $.Deferred();
    var returnData = {
        success: false,
        data: {}
    };
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.securityRoles;
    //var userRoles = Xrm.Page.context.getUserRoles();
    var hasRole = false;
    var deferreds = [];
    for (var i = 0; i < userRoles.length; i++) {
        if (hasRole) {
            return true;
        }
        var currentUserRole = userRoles[i];
        var localDeferred1 = getCurrentUserRole(roles, currentUserRole);

        deferreds.push(localDeferred1);

        //CrmRestKit.Retrieve('Role', userRoles[i], ['Name'], false).fail(
        //    function (err) {
        //        return;
        //    }).done(
        //    function (data) {
        //        if (data != null && data.d != null) {
        //            var roleName = data.d.Name.trim().toLowerCase();
        //            if (roles.toLowerCase().indexOf(roleName) !== -1) {
        //                hasRole = true;
        //                return;
        //            }
        //        }
        //    });

    }
    if (typeof $.when.all === 'undefined')
        loadWhenAllDefinition()

    $.when.all(deferreds).then(function (objects) {
        //console.log("Resolved objects:", objects);
        returnData.data.result = false;
        for (var i = 0; i < objects.length; i++) {
            if (objects[i].data.result)
                returnData.data.result = true
        }
        returnData.success = true;
        deferred.resolve(returnData)
    });

    //return hasRole;
    return deferred.promise();
};



getCurrentUserRole = function (roles, currentUserRole) {
    var localDeferred = $.Deferred();
    var returnData = {
        success: true,
        data: {}
    };
    Xrm.WebApi.retrieveRecord("Role", currentUserRole, "?$select=name").then(
        function success(result) {
            if (result != null) {
                var roleName = result.name.trim().toLowerCase();
                if (roles.toLowerCase().indexOf(roleName) != -1) {
                    hasRole = true;
                    //return;
                    // return hasRole;

                    returnData.success = true;
                    returnData.data.result = hasRole;

                }
                localDeferred.resolve(returnData);
            }
        },
        function (error) {
            returnData.success = false;
            localDeferred.resolve(returnData);

        }
    );
    return localDeferred.promise();
}


loadWhenAllDefinition = function () {
    if (typeof jQuery !== 'undefined' && typeof $ !== 'undefined') {
        if (jQuery.when.all === undefined) {
            jQuery.when.all = function (deferreds) {
                var deferred = new jQuery.Deferred();
                $.when.apply(jQuery, deferreds).then(
                    function () {
                        deferred.resolve(Array.prototype.slice.call(arguments));
                    },
                    function () {
                        deferred.fail(Array.prototype.slice.call(arguments));
                    });

                return deferred;
            }
        }
    }
}

/***********************************************************************
/** 
/** MCSGlbal Functions.js
/** Description: Global rules called by form level jscripts 
/** 
***********************************************************************/
//If the MCS namespace object is not defined, create it. 
if (typeof (MCS) === "undefined") { MCS = { __namespace: true }; }
MCS.GlobalFunctions = {
    GetRequestObject: function () {
        //SD: UCI Changes
        //web-avoid-browser-specific-api
        //https://developpaper.com/a-general-review-of-ajax-knowledge-system/
        "use strict";
        var xhr = null;
        if (window.XMLHttpRequest) {

            xhr = new XMLHttpRequest();
        } else if (window.ActiveXObject) {
            try {
                xhr = new ActiveXObject("Msxml2.XMLHTTP");
            } catch (e) {
                try {
                    xhr = new ActiveXObject("Microsoft.XMLHTTP");
                } catch (e) {
                    Alert("your browser does not support Ajax!");
                }
            }
        }
        return xhr;
        //if (window.XMLHttpRequest) {
        //    return new window.XMLHttpRequest;
        //}
        //else {
        //    try {
        //        return new ActiveXObject("MSXML2.XMLHTTP.3.0");
        //    }
        //    catch (ex) {
        //        return null;
        //    }
        //}
    },
    GuidsAreEqual: function (guid1, guid2) {
        var isEqual = false;

        if (guid1 == null || guid2 == null) {
            isEqual = false;
        }
        else {
            isEqual = guid1.replace(/[{}]/g, "").toLowerCase() === guid2.replace(/[{}]/g, "").toLowerCase();
        }

        return isEqual;
    },

    getODataUTCDateFilter: function (date) {

        var monthString;
        var rawMonth = (date.getUTCMonth() + 1).toString();
        if (rawMonth.length === 1) {
            monthString = "0" + rawMonth;
        }
        else { monthString = rawMonth; }

        var dateString;
        var rawDate = date.getUTCDate().toString();
        if (rawDate.length === 1) {
            dateString = "0" + rawDate;
        }
        else { dateString = rawDate; }


        var DateFilter = "datetime\'";
        DateFilter += date.getUTCFullYear() + "-";
        DateFilter += monthString + "-";
        //DateFilter += "07-";
        DateFilter += dateString;
        DateFilter += "T";
        var temp = date.getUTCHours();
        if (temp.toString().length === 1) {
            temp = "0" + temp;
        }
        DateFilter += temp + ":";

        temp = date.getUTCMinutes();
        if (temp.toString().length === 1) {
            temp = "0" + temp;
        }
        DateFilter += temp + ":";

        temp = date.getUTCSeconds();
        if (temp.toString().length === 1) {
            temp = "0" + temp;
        }
        DateFilter += temp + "\'";

        //
        //         DateFilter += date.getUTCSeconds() + ":";
        //         DateFilter += date.getUTCMilliseconds();
        //         DateFilter += "Z\'";
        return DateFilter;
    },
    getAuthenticationHeader: function () {
        //SD
        //web-avoid-2011-api
        //https://social.microsoft.com/Forums/en-US/71ebd8ff-6b1a-4426-80bf-1c41de6b45f7/mscrm-2013-getauthenticationheader?forum=crmdevelopment
        //https://docs.microsoft.com/en-us/previous-versions/dynamicscrm-2016/developers-guide/gg334511(v=crm.8)?redirectedfrom=MSDN#BKMK_getAuthenticationHeader
        //The getAuthenticationHeader and getServerUrl methods were deprecated with Microsoft Dynamics CRM 2011 and are 
        //no longer present in Microsoft Dynamics 365(online & on - premises).

        //var authenticationHeader = Xrm.Page.context.getAuthenticationHeader();

    },
    _getClientUrl: function (urlType) {

        //SD
        //web-avoid-crm2011-service-soap
        //https://community.dynamics.com/crm/f/microsoft-dynamics-crm-forum/311096/is-organization-service-is-going-to-be-deprecated
        //The URL '/xrmservices/2011/organization.svc/web' targets a deprecated Dynamics CRM 2011 SOAP endpoint. 
        //The Dynamics CRM 2011 SOAP endpoints were deprecated in Dynamics CRM 2016(v8.0) and will soon be considered unsupported.
        //Replace with the preferred Web API endpoint '/api/data/[version]' which implements the OData 4.0 messaging protocol.
        //var orgServicePath = "/xrmservices/2011/organization.svc/web";
        var orgServicePath = "/api/data/v9.1";
        //SD
        //web-avoid-crm2011-service-odata
        //https://community.dynamics.com/crm/f/microsoft-dynamics-crm-forum/311096/is-organization-service-is-going-to-be-deprecated
        //The URL '/xrmservices/2011/organizationdata.svc' targets the deprecated Dynamics CRM 2011 OData 2.0 endpoint. 
        //The Dynamics CRM 2011 OData 2.0 endpoint was deprecated in Dynamics CRM 2016(v8.0) and will soon be considered unsupported.
        //Replace with the new Web API endpoint '/api/data/[version]' which implements OData 4.0 messaging protocol.
        //if (urlType == "ODATA") {
        //    orgServicePath = "/xrmservices/2011/organizationdata.svc";
        //}
        var serverUrl = "";
        if (typeof GetGlobalContext === "function") {
            var context = GetGlobalContext();
            serverUrl = context.getClientUrl();
        }
        else {
            //SD web-use-client-context	
            //if (typeof Xrm.Page.context === "object") {
            //    serverUrl = Xrm.Page.context.getClientUrl();
            //}
            if (Xrm.Utility.getGlobalContext() === "object") {
                serverUrl = Xrm.Utility.getGlobalContext().getClientUrl();
            }
            else { throw new Error("Unable to access the server URL"); }
        }
        if (serverUrl.match(/\/$/)) {
            serverUrl = serverUrl.substring(0, serverUrl.length - 1);
        }
        return serverUrl + orgServicePath;
    },

    runWorkflow: function (objectId, workflowId, runResponse) {
        var request = "<Execute xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">";
        request += "<request i:type=\"b:ExecuteWorkflowRequest\" xmlns:a=\"http://schemas.microsoft.com/xrm/2011/Contracts\" xmlns:b=\"http://schemas.microsoft.com/crm/2011/Contracts\">";
        request += "<a:Parameters xmlns:c=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">";
        request += "<a:KeyValuePairOfstringanyType>";
        request += "<c:key>EntityId</c:key>";
        request += "<c:value i:type=\"d:guid\" xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + objectId + "</c:value>";
        request += "</a:KeyValuePairOfstringanyType>";
        request += "<a:KeyValuePairOfstringanyType>";
        request += "<c:key>WorkflowId</c:key>";
        request += "<c:value i:type=\"d:guid\" xmlns:d=\"http://schemas.microsoft.com/2003/10/Serialization/\">" + workflowId + "</c:value>";
        request += "</a:KeyValuePairOfstringanyType>";
        request += "</a:Parameters>";
        request += "<a:RequestId i:nil=\"true\" />";
        request += "<a:RequestName>ExecuteWorkflow</a:RequestName>";
        request += "</request>";
        request += "</Execute>";
        request = this._getSOAPWrapper(request);

        var req = new XMLHttpRequest();
        req.open("POST", this._getClientUrl("SOAP"), true);
        req.setRequestHeader("Accept", "application/xml, text/xml, */*");
        req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
        req.setRequestHeader("SOAPAction", this._Action.RunWorkflow);
        req.onreadystatechange = function () { runResponse(req); };
        req.send(request);

    },
    runWorkflowResponse: function () {
        return new MCS.GlobalFunctions._response(200, function (responseXML) {
            return "OK";
        });
    },
    createSOAPRequest: function (type, attributes, createResponse) {
        var request = "<Create xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\">";
        request += "<entity xmlns:b=\"http://schemas.microsoft.com/xrm/2011/Contracts\"";
        request += " xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">";
        request += this._getAttributeWrapper(attributes);
        request += "<b:EntityState i:nil=\"true\"/>";
        request += "<b:FormattedValues xmlns:c=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\"/>";
        request += "<b:Id>00000000-0000-0000-0000-000000000000</b:Id>";
        request += "<b:LogicalName>" + type + "</b:LogicalName>";
        request += "<b:RelatedEntities xmlns:c=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\"/>";
        request += "</entity>";
        request += "</Create>";
        request = this._getSOAPWrapper(request);

        var req = new XMLHttpRequest();
        req.open("POST", this._getClientUrl("SOAP"), true);
        req.setRequestHeader("Accept", "application/xml, text/xml, */*");
        req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
        req.setRequestHeader("SOAPAction", this._Action.Create);
        req.onreadystatechange = function () { createResponse.parseResponse(req); };
        req.send(request);

    }, createResponse: function () {
        return new MCS.GlobalFunctions._response(200, function (responseXML) {
            return responseXML.selectSingleNode("//CreateResult").text;
        });
    },
    retrieveRequest: function (type, id, columnSet, retrieveResponse) {

        var request = "<Retrieve xmlns=\"http://schemas.microsoft.com/xrm/2011/Contracts/Services\">";
        request += "<entityName>" + type + "</entityName>";
        request += "<id>" + id + "</id>";
        request += MCS.GlobalFunctions._getColumnSet(columnSet);
        request += "</Retrieve>";
        request = this._getSOAPWrapper(request);

        var req = new XMLHttpRequest();
        req.open("POST", this._getClientUrl("SOAP") + "/web", true);
        req.setRequestHeader("Accept", "application/xml, text/xml, */*");
        req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
        req.setRequestHeader("SOAPAction", this._Action.Retrieve);
        req.onreadystatechange = function () { retrieveResponse._parseResponse(req); };
        req.send(request);

    },
    retrieveResponse: function () {
        return new MCS.GlobalFunctions._response(200, function (responseXML) {
            var attributesData = responseXML.selectNodes("//a:KeyValuePairOfstringanyType")
            var entityInstance = {};
            for (var i = 0; i < attributesData.length; i++) {

                var attributeName = attributesData[i].selectSingleNode("b:key").text;
                var attributeType = attributesData[i].selectSingleNode("b:value").attributes.getNamedItem("i:type").text;
                var attributeValue = attributesData[i].selectSingleNode("b:value");
                switch (attributeType) {
                    case "c:guid":
                    case "c:string":
                        entityInstance[attributeName] = attributeValue.text;
                        break;
                    case "a:EntityReference":
                        var value = {};
                        value.Id = attributeValue.selectSingleNode("a:Id").text;
                        value.LogicalName = attributeValue.selectSingleNode("a:LogicalName").text;
                        value.Name = attributeValue.selectSingleNode("a:Name").text;
                        entityInstance[attributeName] = value;
                        break;
                    default:
                        throw new Error("Parsing " + attributeType + " attributes not Implemented.");
                        break;
                }

            }

            return entityInstance;


        });
    },
    updateRequest: function () { },
    updateResponse: function () { },
    deleteRequest: function () { },
    deleteResponse: function () { },
    _getSOAPWrapper: function (request) {
        var SOAP = "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\"><s:Body>";
        SOAP += request;
        SOAP += "</s:Body></s:Envelope>";
        return SOAP;
    },
    _getAttributeWrapper: function (attributes) {
        var attributesString = "<b:Attributes xmlns:c=\"http://schemas.datacontract.org/2004/07/System.Collections.Generic\">";

        for (var i = 0; i < attributes.length; i++) {

            var attribute = "<b:KeyValuePairOfstringanyType>";
            attribute += "<c:key>" + attributes[i].name + "</c:key>";

            switch (attributes[i].type) {

                case "string":
                    attribute += "<c:value i:type=\"d:" + attributes[i].type + "\" ";
                    attribute += " xmlns:d=\"http://www.w3.org/2001/XMLSchema\">" + attributes[i].value + "</c:value>";
                    break;
                case "EntityReference":
                    attribute += "<c:value i:type=\"b:EntityReference\">";
                    attribute += "<b:Id>" + attributes[i].value.Id + "</b:Id>";
                    attribute += "<b:LogicalName>" + attributes[i].value.LogicalName + "</b:LogicalName>";
                    if (attributes[i].value.Name == null) { attribute += "<b:Name i:nil=\"true\"/>"; }
                    else { attribute += "<b:Name>" + attributes[i].value.Name + "</b:Name>"; }
                    attribute += "</c:value>";
                    break;
            }




            attribute += "</b:KeyValuePairOfstringanyType>"
            attributesString += attribute;

        }
        attributesString += "</b:Attributes>";

        return attributesString;

    },
    columnSet: function (columns) {
        if (columns == null) {
            return { allColumns: false, columns: [] };
        }
        else {
            var errorMessage = "The columns parameter must be a comma separated list of strings or an array of strings.";
            var arrColumns = [];
            switch (typeof columns) {
                case "string":
                    arrColumns = columns.split(",");
                    break;
                case "object":
                    if (columns instanceof Array) {
                        var stringArray = true;
                        for (var i = 0; i < columns.length; i++) {
                            if (typeof columns[i] !== "string") {
                                stringArray = false;
                                break;
                            }
                        }
                        if (stringArray) { arrColumns = columns; }
                        else { throw new Error(errorMessage); }
                    }
                    else { throw new Error(errorMessage); }
                    break;
                default:
                    throw new Error(errorMessage);
                    break;
            }
            return { allColumns: false, columns: arrColumns };
        }
    },
    _getColumnSet: function (columnSet) {

        var col = "<columnSet xmlns:b=\"http://schemas.microsoft.com/xrm/2011/Contracts\"";
        col += " xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">";
        if (columnSet.allColumns === true) { col += "<b:AllColumns>true</b:AllColumns>"; }
        else {
            col += "<b:AllColumns>false</b:AllColumns>";
            col += "<b:Columns xmlns:c=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">";
            for (var i = 0; i < columnSet.columns.length; i++) {
                col += "<c:string>" + columnSet.columns[i] + "</c:string>";
            }
            col += "</b:Columns>";
        }

        col += "</columnSet>";

        return col;

    },
    _Action: {
        Execute: "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute",
        Create: "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Create",
        Retrieve: "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Retrieve",
        Update: "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Update",
        Delete: "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Delete",
        RunWorkflow: "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute"
    },
    _getError: function (faultXml) {
        var errorMessage = "Unknown Error (Unable to parse the fault)";
        if (typeof faultXml === "object") {
            try {
                var bodyNode = faultXml.firstChild.firstChild;
                //Retrieve the fault node
                for (var i = 0; i < bodyNode.childNodes.length; i++) {
                    var node = bodyNode.childNodes[i];

                    //NOTE: This comparison does not handle the case where the XML namespace changes
                    if ("s:Fault" === node.nodeName) {
                        for (var j = 0; j < node.childNodes.length; j++) {
                            var faultStringNode = node.childNodes[j];
                            if ("faultstring" === faultStringNode.nodeName) {
                                errorMessage = faultStringNode.text;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            catch (e) { };
        }

        return new Error(errorMessage);

    },
    _response: function (successStatus, parseData) {
        return {
            _state: "sent",
            _data: null,
            _error: null,
            _parseData: parseData,
            onComplete: null,
            _parseResponse: function (req) {

                if (req.readyState === 4) {
                    this._state = "recieved";
                    if (req.status === successStatus) {
                        this._data = parseData(req.responseXML);
                        this._state = "complete";
                        if (this.onComplete != null) {
                            this.onComplete();
                        }
                    }
                    else {
                        this._state = "error";
                        this._error = new MCS.GlobalFunctions._getError(req.responseXML);
                        if (this.onComplete != null) {
                            this.onComplete();
                        }
                    }
                }

            },
            getError: function () {
                if (this._error != null) { return this._error; }
                else { throw new Error("No error exists."); }
            },
            getState: function () { return this._state; },
            getData: function () {
                if (this._state === "complete") {
                    return this._data;
                }
                else { throw new Error("Data is not ready yet."); }
            }
        };
    },

    createRestRecord: function (recordToCreate, callback, recordType) {
        var jsonRecord = window.JSON.stringify(recordToCreate);

        var createReq = new XMLHttpRequest();
        createReq.open("POST", this._getClientUrl("ODATA") + "/" + recordType, true);
        createReq.setRequestHeader("Accept", "application/json");
        createReq.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        createReq.onreadystatechange = function () {
            MCS.GlobalFunctions.createRestReqCallBack(this, callback);
        };
        createReq.send(jsonRecord);

    },
    createRestReqCallBack: function (createReq, callback) {
        if (createReq.readyState === 4 /* complete */) {
            if (createReq.status === 201) {
                //Success
                var newRecord = JSON.parse(createReq.responseText).d;
                callback(newRecord);
            }
            else {
                //Failure
                MCS.GlobalFunctions.errorHandler(createReq);
            }
        }
    },


    RetrieveRecords: function (filter, callback) {
        /// <summary>
        /// Initiates an asynchronous request to retrieve records.
        /// If there are additional pages of records the SDK.RestEndpointPaging.RetrieveRecordsCallBack function will
        /// call this function.
        /// </summary>
        var retrieveRecordsReq = new XMLHttpRequest();
        retrieveRecordsReq.open("GET", this._getClientUrl("ODATA") + filter, true);
        retrieveRecordsReq.setRequestHeader("Accept", "application/json");
        retrieveRecordsReq.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        retrieveRecordsReq.onreadystatechange = function () {
            /// <summary>
            /// This event handler passes the callback through
            /// </summary>
            MCS.GlobalFunctions.RetrieveRecordsCallBack(this, callback);
        };
        retrieveRecordsReq.send();

    },
    dateReviver: function (key, value) {
        var a;
        if (typeof value === 'string') {
            a = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)Z$/.exec(value);
            if (a) {
                return new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6]));
            }
        }
        return value;
    },
    RetrieveRecordsCallBack: function (retrieveRecordsReq, callback) {
        /// <summary>
        /// Handles the onreadystatechange event to process the records returned.
        /// If more pages are available this function will call the SDK.RestEndpointPaging.RetrieveRecords 
        /// function to get the rest.
        /// </summary>
        if (retrieveRecordsReq.readyState === 4 /* complete */) {
            if (retrieveRecordsReq.status === 200) {
                //Success
                var retrievedRecords = JSON.parse(retrieveRecordsReq.responseText, MCS.GlobalFunctions.dateReviver).d;
                /// The callback is called with the results.
                callback(retrievedRecords.results);

                if (null != retrievedRecords.__next) {
                    // The existance of the '__next' property indicates that more records are available
                    // So the originating function is called again using the filter value returned
                    var filter = retrievedRecords.__next.replace(MCS.GlobalFunctions.GetODataPath(), "");
                    MCS.GlobalFunctions.RetrieveRecords(filter, callback);
                }


            }
            else {
                //Failure
                MCS.GlobalFunctions.errorHandler(retrieveRecordsReq);

            }
        }
    },
    //Function to handle any http errors
    errorHandler: function (XmlHttpRequest) {
        /// <summary>
        /// Simply displays an alert message with details about any errors.
        /// </summary>
        if (XmlHttpRequest) {
            if (XmlHttpRequest.status != null) {
                if (XMLHttpRequest.statusText != null) {
                    alert("Error : " +
                        XmlHttpRequest.status + ": " +
                        XmlHttpRequest.statusText + ": " +
                        JSON.parse(XmlHttpRequest.responseText).error.message.value);
                }
                else {
                    alert(XMLHttpRequest);
                }
            }
            else {
                alert(XMLHttpRequest);
            }
        }
        else {
            alert("Unknown error occurred");
        }
    },

    __namespace: true
};

//Venkat- Commenting as its not being used
/*
var XrmSvcToolkit = (function (window, undefined) {
    /**
    * XrmSvcToolkit v0.2, a small JavaScript library that helps access 
    * Microsoft Dynamics CRM 2011 web service interfaces (SOAP and REST)
    *
    * @copyright    Copyright (c) 2011 - 2013, KingswaySoft (http://www.kingswaysoft.com)
    * @license      Microsoft Public License (Ms-PL)
    * @developer    Daniel Cai (http://danielcai.blogspot.com)
 
    * @contributors George Doubinski, Mitch Milam, Carsten Groth
    *
    * THIS SOFTWARE IS PROVIDED BY KingswaySoft ''AS IS'' AND ANY
    * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
    * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
    * DISCLAIMED. IN NO EVENT SHALL KingswaySoft BE LIABLE FOR ANY
    * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
    * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
    * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
    * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
    * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
    * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
    *
    * /
 
    var odataEndpoint = "/XRMServices/2011/OrganizationData.svc",
        soapEndpoint = "/XRMServices/2011/Organization.svc/web";
 
    // Type sniffering
    var toString = Object.prototype.toString,
        isFunction = function (o) {
            return toString.call(o) === "[object Function]";
        },
        isInteger = function (o) {
            return !isNaN(parseInt(o));
        },
        isString = function (o) {
            return toString.call(o) === "[object String]";
        },
        isArray = function (o) {
            return toString.call(o) === "[object Array]";
        },
        isNonEmptyString = function (o) {
            if (!isString(o) || o.length === 0) {
                return false;
            }
 
            // checks for a non-white space character 
            return /[^\s]+/.test(o);
        };
 
    var isoDateExpr = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})\.?(\d*)?(Z|[+-]\d{2}?(:\d{2})?)?$/,
        jsonDateExpr = /^\/Date\(([-+]?\d+)\)\/$/;
 
    var context = (function () {
        if (isFunction(window.GetGlobalContext)) {
            return GetGlobalContext();
        } else if (Xrm !== undefined) {
            return Xrm.Page.context;
        } else {
            throw new Error("CRM context is not available.");
        }
    })();
 
    var clientUrl = (function () {
        if (context.getClientUrl !== undefined) {
            return context.getClientUrl();
        }
 
        var localServerUrl = window.location.protocol + "//" + window.location.host;
        if (context.isOutlookClient() && !context.isOutlookOnline()) {
            return localServerUrl;
        } else {
            var crmServerUrl = context.getClientUrl();
            crmServerUrl = crmServerUrl.replace(/^(http|https):\/\/([_a-zA-Z0-9\-\.]+)(:([0-9]{1,5}))?/, localServerUrl);
            crmServerUrl = crmServerUrl.replace(/\/$/, "");
        }
 
        return crmServerUrl;
    })();
 
    var restErrorHandler = function (req) {
        var errorMessage;
 
        try {
            errorMessage = JSON.parse(req.responseText).error.message.value;
        } catch (err) {
            // Ignore any error when parsing the error message. 
            errorMessage = req.responseText;
        }
 
        errorMessage = errorMessage.length > 0
            ? "Error: " + req.status + ": " + req.statusText + ": " + errorMessage
            : "Error: " + req.status + ": " + req.statusText;
 
        return new Error(errorMessage);
    };
 
    var soapErrorHandler = function (req) {
        var errorMessage = req.responseText.length > 0
            ? "Error: " + req.status + ": " + req.statusText + ": " + req.responseText
            : "Error: " + req.status + ": " + req.statusText;
 
        return new Error(errorMessage);
    };
 
    var dateReviver = function (key, value) {
        if (typeof value === 'string') {
            if (value.match(jsonDateExpr)) {
                var dateValue = value.replace(jsonDateExpr, "$1");
                return new Date(parseInt(dateValue, 10));
            }
        }
        return value;
    };
 
    var xmlEncode = function (input) {
        if (input == null) {
            return null;
        }
 
        if (input == '') {
            return '';
        }
 
        var c;
        var result = '';
 
        for (var pos = 0; pos < input.length; pos++) {
            c = input.charCodeAt(pos);
 
            if ((c > 96 && c < 123) ||
                (c > 64 && c < 91) ||
                (c > 47 && c < 58) ||
                (c == 32) ||
                (c == 44) ||
                (c == 46) ||
                (c == 45) ||
                (c == 95)) {
                result = result + String.fromCharCode(c);
            } else {
                result = result + '&#' + c + ';';
            }
        }
 
        return result;
    };
 
    var parseIsoDate = function (s) {
        if (s == null || !s.match(isoDateExpr))
            return null;
 
        var dateParts = isoDateExpr.exec(s);
        return new Date(Date.UTC(parseInt(dateParts[1], 10),
            parseInt(dateParts[2], 10) - 1,
            parseInt(dateParts[3], 10),
            parseInt(dateParts[4], 10) - (dateParts[8] == "" || dateParts[8] == "Z" ? 0 : parseInt(dateParts[8])),
            parseInt(dateParts[5], 10),
            parseInt(dateParts[6], 10)));
    };
 
    var getAttribute = function (xmlNode, attrName) {
        for (var i = 0; i < xmlNode.attributes.length; i++) {
            var attr = xmlNode.attributes[i];
            if (attr.name == attrName) {
                return attr.value;
            }
        }
    };
 
    var getNodeText = function (node) {
        return node.text !== undefined
            ? node.text
            : node.textContent;
    }
 
    var getTypedValue = function (fieldType, valueNode) {
        switch (fieldType) {
            case "c:string":
            case "c:guid":
                return getNodeText(valueNode);
            case "c:boolean":
                return getNodeText(valueNode) === "true";
            case "c:int":
                return parseInt(getNodeText(valueNode));
            case "c:decimal":
            case "c:double":
                return parseFloat(getNodeText(valueNode));
            case "c:dateTime":
                return parseIsoDate(getNodeText(valueNode));
            case "a:OptionSetValue":
                valueNode = getChildNode(valueNode, "a:Value");
                return {
                    Value: parseInt(getNodeText(valueNode))
                };
            case "a:Money":
                valueNode = getChildNode(valueNode, "a:Value");
                return {
                    Value: getNodeText(valueNode)
                };
            case "a:EntityReference":
                return getEntityReference(valueNode);
            case "a:EntityCollection":
                return getEntityCollection(valueNode);
            case "a:AliasedValue":
                valueNode = getChildNode(valueNode, "a:Value");
                fieldType = getAttribute(valueNode, "i:type");
                return getTypedValue(fieldType, valueNode);
 
            default:
                throw new Error("Unhandled field type: \"" + fieldType + "\", please report the problem to the developer. ");
        }
    };
 
    var concatOdataFields = function (fields, parameterName) {
        if (isArray(fields) && fields.length > 0) {
            return fields.join(',');
        } else if (isString(fields)) {
            return fields;
        }
        else if (parameterName != undefined) {
            throw new Error(parameterName + " parameter must be either a delimited string or an array. ");
        }
        else {
            return "";
        }
    };
 
    // Get a list of entities from an EntityCollection XML node.
    var getEntityCollection = function (entityCollectionNode) {
        var entityName, moreRecords, pagingCookie, totalRecordCount, entitiesNode;
 
        // Try to get all child nodes in one pass
        for (var m = 0; m < entityCollectionNode.childNodes.length; m++) {
            var collectionChildNode = entityCollectionNode.childNodes[m];
            switch (collectionChildNode.nodeName) {
                case "a:EntityName":
                    entityName = getNodeText(collectionChildNode);
                    break;
                case "a:MoreRecords":
                    moreRecords = getNodeText(collectionChildNode) === "true";
                    break;
                case "a:PagingCookie":
                    pagingCookie = getNodeText(collectionChildNode);
                    break;
                case "a:TotalRecordCount":
                    totalRecordCount = parseInt(getNodeText(collectionChildNode));
                    break;
                case "a:Entities":
                    entitiesNode = collectionChildNode;
                    break;
            }
        }
 
        var result = {
            entityName: entityName,
            moreRecords: moreRecords,
            pagingCookie: pagingCookie,
            totalRecordCount: totalRecordCount,
            entities: []
        };
 
        for (var i = 0; i < entitiesNode.childNodes.length; i++) {
            var entity = { formattedValues: [] };
            var entityNode = entitiesNode.childNodes[i];
            var attributes = getChildNode(entityNode, "a:Attributes");
            for (var j = 0; j < attributes.childNodes.length; j++) {
                var attr = attributes.childNodes[j];
 
                var fieldName = getNodeText(getChildNode(attr, "b:key"));
                var valueNode = getChildNode(attr, "b:value");
                var fieldType = getAttribute(valueNode, "i:type");
 
                entity[fieldName] = getTypedValue(fieldType, valueNode);
            }
 
            var formattedValues = getChildNode(entityNode, "a:FormattedValues");
 
            for (var k = 0; k < formattedValues.childNodes.length; k++) {
                var valuePair = formattedValues.childNodes[k];
                entity.formattedValues[getNodeText(getChildNode(valuePair, "b:key"))] = getNodeText(getChildNode(valuePair, "b:value"));
            }
 
            result.entities.push(entity);
        }
 
        return result;
    };
 
    // Get an EntityReference from an XML node. For performance reason, we try to
    // get the entity reference in one pass, instead of multiple.
    var getEntityReference = function (xmlNode) {
        var id, logicalName, name;
        for (var i = 0; i < xmlNode.childNodes.length; i++) {
            var childNode = xmlNode.childNodes[i];
 
            switch (childNode.nodeName) {
                case "a:Id":
                    id = getNodeText(childNode);
                    break;
                case "a:LogicalName":
                    logicalName = getNodeText(childNode);
                    break;
                case "a:Name":
                    name = getNodeText(childNode);
                    break;
            }
        }
 
        return {
            Id: id,
            LogicalName: logicalName,
            Name: name
        };
    }
 
    // Get a single child node that matches the specified name.
    var getChildNode = function (xmlNode, nodeName) {
        for (var i = 0; i < xmlNode.childNodes.length; i++) {
            var childNode = xmlNode.childNodes[i];
 
            if (childNode.nodeName == nodeName) {
                return childNode;
            }
        }
    }
 
    var getSoapError = function (soapXml) {
        try {
            var bodyNode = soapXml.firstChild.firstChild;
            var faultNode = getChildNode(bodyNode, "s:Fault");
            var faultStringNode = getChildNode(faultNode, "faultstring");
            return new Error(getNodeText(faultStringNode));
        }
        catch (e) {
            return new Error("An error occurred when parsing the error returned from CRM server: " + e.message);
        }
    }
 
    var processSoapResponse = function (responseXml, successCallback, errorCallback) {
        try {
            var executeResult = responseXml.firstChild.firstChild.firstChild.firstChild; // "s:Envelope/s:Body/ExecuteResponse/ExecuteResult"
        } catch (err) {
            errorCallback(err);
            return;
        }
 
        return successCallback(executeResult);
    };
 
    var getFetchResults = function (resultXml) {
        // For simplicity reason, we are assuming the returned SOAP message uses the following three namespace aliases
        //   xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts"
        //   xmlns:i="http://www.w3.org/2001/XMLSchema-instance"
        //   xmlns:b="http://schemas.datacontract.org/2004/07/System.Collections.Generic"
        // however it is possible that the namespace aliases returned from CRM server could be different, in which
        // case, the fetch function will not work properly
        // For future reference, XPath to the entity collection node:
        // a:Results/a:KeyValuePairOfstringanyType/b:value[@i:type='a:EntityCollection']
        var resultsNode = getChildNode(resultXml, "a:Results"); // a:Results
        var entityCollectionNode = getChildNode(resultsNode.firstChild, "b:value"); // b:value
        return getEntityCollection(entityCollectionNode);
    };
 
    var processRestResult = function (req, successCallback, errorCallback) {
        if ((req.status >= 200 && req.status < 300) || req.status === 304 || req.status === 1223) {
            try {
                var result = (!!req.responseText)
                            ? JSON.parse(req.responseText, dateReviver).d
                            : null;
            } catch (err) {
                errorCallback(err);
                return;
            }
 
            return successCallback(result);
 
        } else {
            errorCallback(restErrorHandler(req));
        }
    };
 
    var doRestRequest = function (restReq, successCallback, errorCallback) {
        var req = new XMLHttpRequest();
        req.open(restReq.type, restReq.url, restReq.async);
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        if (!!restReq.method) {
            req.setRequestHeader("X-HTTP-Method", restReq.method);
        }
 
        var erred = false;
 
        if (restReq.async) {
            req.onreadystatechange = function () {
                if (req.readyState == 4 /* complete  * /) {
                    processRestResult(req, successCallback, errorCallback);
                }
            };
 
            if (!!restReq.data) {
                req.send(restReq.data);
            } else {
                req.send();
            }
        } else {
            try {
                //synchronous: send request, then call the callback functions
                if (!!restReq.data) {
                    req.send(restReq.data);
                } else {
                    req.send();
                }
 
                return processRestResult(req, successCallback, errorCallback);
 
            } catch (err) {
                errorCallback(err);
            }
        }
    };
 
    var doSoapRequest = function (soapBody, async, successCallback, errorCallback) {
        var req = new XMLHttpRequest();
 
        req.open("POST", clientUrl + soapEndpoint, async);
        req.setRequestHeader("Accept", "application/xml, text/xml, * /*");
        req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
        req.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
 
        var soapXml = [
'<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body>',
    soapBody,
'</s:Body></s:Envelope>'].join('');
 
        if (async) {
            req.onreadystatechange = function () {
                if (req.readyState == 4) { // "complete"
                    if (req.status == 200) { // "OK"
                        processSoapResponse(req.responseXML, successCallback, errorCallback);
                    } else {
                        errorCallback(soapErrorHandler(req));
                    }
                }
            };
 
            req.send(soapXml);
        } else {
            var syncResult;
            try {
                //synchronous: send request, then call the callback function directly
                req.send(soapXml);
                if (req.status == 200) {
                    return processSoapResponse(req.responseXML, successCallback, errorCallback);
                }
                else {
                    var syncErr = getSoapError(req.responseXML);
                    errorCallback(syncErr);
                    return;
                }
            } catch (err) {
                errorCallback(err);
                return;
            }
 
            successCallback(syncResult);
        }
    };
 
    var execute = function (opts) {
 
        if (!isNonEmptyString(opts.executeXml)) {
            throw new Error("executeXml parameter was not provided. ");
        }
 
        var async = !!opts.async;
 
        return doSoapRequest(opts.executeXml, async, function (result) {
            if (isFunction(opts.successCallback)) {
                opts.successCallback(result);
            }
 
            if (!async) {
                return result;
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            }
            else {
                throw err;
            }
        });
    };
 
    var setState = function (opts) {
 
        if (!isNonEmptyString(opts.id)) {
            throw new Error("id parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.entityName)) {
            throw new Error("entityName parameter was not provided. ");
        }
 
        if (!isInteger(opts.stateCode)) {
            throw new Error("stateCode parameter must be an integer. ");
        }
 
        if (opts.statusCode == null) {
            opts.statusCode = -1;
        }
 
        var request = [
'<Execute xmlns="http://schemas.microsoft.com/xrm/2011/Contracts/Services">',
    '<request i:type="b:SetStateRequest"',
            ' xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts" ',
            ' xmlns:b="http://schemas.microsoft.com/crm/2011/Contracts" ',
            ' xmlns:c="http://schemas.datacontract.org/2004/07/System.Collections.Generic" ',
            ' xmlns:i="http://www.w3.org/2001/XMLSchema-instance">',
        '<a:Parameters>',
            '<a:KeyValuePairOfstringanyType>',
                '<c:key>EntityMoniker</c:key>',
                '<c:value i:type="a:EntityReference">',
                    '<a:Id>', opts.id, '</a:Id>',
                    '<a:LogicalName>', opts.entityName, '</a:LogicalName>',
                    '<a:Name i:nil="true" />',
                '</c:value>',
            '</a:KeyValuePairOfstringanyType>',
            '<a:KeyValuePairOfstringanyType>',
                '<c:key>State</c:key>',
                '<c:value i:type="a:OptionSetValue">',
                    '<a:Value>', opts.stateCode, '</a:Value>',
                '</c:value>',
            '</a:KeyValuePairOfstringanyType>',
            '<a:KeyValuePairOfstringanyType>',
                '<c:key>Status</c:key>',
                '<c:value i:type="a:OptionSetValue">',
                    '<a:Value>', opts.statusCode, '</a:Value>',
                '</c:value>',
            '</a:KeyValuePairOfstringanyType>',
        '</a:Parameters>',
        '<a:RequestId i:nil="true"/>',
        '<a:RequestName>SetState</a:RequestName>',
    '</request>',
'</Execute>'].join("");
 
        var async = !!opts.async;
 
        return doSoapRequest(request, async, function (result) {
            if (isFunction(opts.successCallback)) {
                opts.successCallback(result);
            }
 
            if (!async) {
                return result;
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            }
            else {
                throw err;
            }
        });
    };
 
    var fetch = function (opts) {
        if (!isNonEmptyString(opts.fetchXml)) {
            throw new Error("fetchXml parameter was not provided. ");
        }
 
        var request = [
'<Execute xmlns="http://schemas.microsoft.com/xrm/2011/Contracts/Services">',
    '<request i:type="a:RetrieveMultipleRequest"',
            ' xmlns:a="http://schemas.microsoft.com/xrm/2011/Contracts" ',
            ' xmlns:i="http://www.w3.org/2001/XMLSchema-instance">',
        '<a:Parameters xmlns:c="http://schemas.datacontract.org/2004/07/System.Collections.Generic">',
            '<a:KeyValuePairOfstringanyType>',
                '<c:key>Query</c:key>',
                '<c:value i:type="a:FetchExpression">',
                    '<a:Query>', xmlEncode(opts.fetchXml), '</a:Query>',
                '</c:value>',
            '</a:KeyValuePairOfstringanyType>',
        '</a:Parameters>',
        '<a:RequestId i:nil="true"/>',
        '<a:RequestName>RetrieveMultiple</a:RequestName>',
    '</request>',
'</Execute>'].join("");
 
        var async = !!opts.async;
 
        return doSoapRequest(request, async, function (result) {
            var fetchResults = getFetchResults(result);
 
            if (isFunction(opts.successCallback)) {
                opts.successCallback(fetchResults);
            }
 
            if (!async) {
                return fetchResults;
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            }
            else {
                throw err;
            }
        });
    };
 
    var retrieve = function (opts) {
        if (!isNonEmptyString(opts.entityName)) {
            throw new Error("entityName parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.id)) {
            throw new Error("id parameter was not provided. ");
        }
 
        var select = opts.select == null
            ? ""
            : concatOdataFields(opts.select, "select");
 
        var expand = opts.expand == null
            ? ""
            : concatOdataFields(opts.expand, "expand");
 
        var odataQuery = "";
 
        if (select.length > 0 || expand.length > 0) {
            odataQuery = "?";
            if (select.length > 0) {
                odataQuery += "$select=" + select;
 
                if (expand.length > 0) {
                    odataQuery += "&";
                }
            }
 
            if (expand.length > 0) {
                odataQuery += "$expand=" + expand;
            }
        }
 
        var restReq = {
            url: clientUrl + odataEndpoint + "/" + opts.entityName + "Set(guid'" + opts.id + "')" + odataQuery,
            type: "GET",
            async: !!opts.async
        };
 
        return doRestRequest(restReq, function (result) {
            if (isFunction(opts.successCallback)) {
                opts.successCallback(result);
            }
 
            if (!opts.async) {
                return result;
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            } else {
                throw err;
            }
        });
    };
 
    var retrieveMultiple = function (opts) {
        if (!isNonEmptyString(opts.entityName)) {
            throw new Error("entityName parameter was not provided. ");
        }
 
        var odataQuery = "";
        if (opts.odataQuery != null) {
            if (!isString(opts.odataQuery)) {
                throw new Error("odataQuery parameter must be a string. ");
            }
 
            if (opts.odataQuery.charAt(0) != "?") {
                odataQuery = "?" + opts.odataQuery;
            } else {
                odataQuery = opts.odataQuery;
            }
        }
 
        var restReq = {
            url: clientUrl + odataEndpoint + "/" + opts.entityName + "Set" + odataQuery,
            type: "GET",
            async: !!opts.async
        };
 
        return doRestRequest(restReq, function (result) {
            if (isFunction(opts.successCallback)) {
                opts.successCallback(result.results);
            }
 
            if (!opts.async) {
                return result.results;
            }
 
            if (result.__next != null) {
                opts.odataQuery = result.__next.substring((clientUrl + odataEndpoint + "/" + opts.entityName + "Set").length);
                retrieveMultiple(opts);
            } else {
                if (isFunction(opts.completionCallback)) {
                    opts.completionCallback();
                }
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            } else {
                throw err;
            }
        });
    };
 
    var createRecord = function (opts) {
        if (!isNonEmptyString(opts.entityName)) {
            throw new Error("entityName parameter was not provided. ");
        }
 
        if (opts.entity === null || opts.entity === undefined) {
            throw new Error("entity parameter was not provided. ");
        }
 
        var restReq = {
            url: clientUrl + odataEndpoint + "/" + opts.entityName + 'Set',
            type: "POST",
            data: window.JSON.stringify(opts.entity),
            async: !!opts.async
        };
 
        return doRestRequest(restReq, function (result) {
            if (isFunction(opts.successCallback)) {
                opts.successCallback(result);
            }
 
            if (!opts.async) {
                return result;
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            } else {
                throw err;
            }
        });
    };
 
    var updateRecord = function (opts) {
        if (!isNonEmptyString(opts.entityName)) {
            throw new Error("entityName parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.id)) {
            throw new Error("id parameter was not provided. ");
        }
 
        if (opts.entity === null || opts.entity === undefined) {
            throw new Error("entity parameter was not provided. ");
        }
 
        var restReq = {
            url: clientUrl + odataEndpoint + "/" + opts.entityName + "Set(guid'" + opts.id + "')",
            type: "POST",
            method: "MERGE",
            data: window.JSON.stringify(opts.entity),
            async: !!opts.async
        };
 
        return doRestRequest(restReq, function (result) {
            if (isFunction(opts.successCallback)) {
                opts.successCallback(result);
            }
 
            if (!opts.async) {
                return result;
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            } else {
                throw err;
            }
        });
    };
 
    var deleteRecord = function (opts) {
 
        if (!isNonEmptyString(opts.entityName)) {
            throw new Error("entityName parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.id)) {
            throw new Error("id parameter was not provided. ");
        }
 
        var restReq = {
            url: clientUrl + odataEndpoint + "/" + opts.entityName + "Set(guid'" + opts.id + "')",
            type: "POST",
            method: "DELETE",
            async: !!opts.async
        };
 
        return doRestRequest(restReq, function (result) {
            if (isFunction(opts.successCallback)) {
                opts.successCallback(result);
            }
 
            if (!opts.async) {
                return result;
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            }
            else {
                throw err;
            }
        });
    };
 
    var associate = function (opts) {
 
        if (!isNonEmptyString(opts.entity1Id)) {
            throw new Error("entity1Id parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.entity1Name)) {
            throw new Error("entity1Name parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.entity2Id)) {
            throw new Error("entity2Id parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.entity2Name)) {
            throw new Error("entity2Name parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.relationshipName)) {
            throw new Error("relationshipName parameter was not provided. ");
        }
 
        var entity2Uri = {
            uri: clientUrl + odataEndpoint + "/" + opts.entity2Name + "Set(guid'" + opts.entity2Id + "')"
        };
 
        var restReq = {
            url: clientUrl + odataEndpoint + "/" + opts.entity1Name + "Set(guid'" + opts.entity1Id + "')/$links/" + opts.relationshipName,
            type: "POST",
            data: window.JSON.stringify(entity2Uri),
            async: !!opts.async
        };
 
        return doRestRequest(restReq, function (result) {
            if (isFunction(opts.successCallback)) {
                opts.successCallback(result);
            }
 
            if (!opts.async) {
                return result;
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            } else {
                throw err;
            }
        });
    };
 
    var disassociate = function (opts) {
 
        if (!isNonEmptyString(opts.entity1Id)) {
            throw new Error("entity1Id parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.entity1Name)) {
            throw new Error("entity1Name parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.entity2Id)) {
            throw new Error("entity2Id parameter was not provided. ");
        }
 
        if (!isNonEmptyString(opts.relationshipName)) {
            throw new Error("relationshipName parameter was not provided. ");
        }
 
        var restReq = {
            url: clientUrl + odataEndpoint + "/" + opts.entity1Name + "Set(guid'" + opts.entity1Id + "')/$links/" + opts.relationshipName + "(guid'" + opts.entity2Id + "')",
            type: "POST",
            method: "DELETE",
            async: !!opts.async
        };
 
        return doRestRequest(restReq, function (result) {
            if (isFunction(opts.successCallback)) {
                opts.successCallback(result);
            }
 
            if (!opts.async) {
                return result;
            }
        }, function (err) {
            if (isFunction(opts.errorCallback)) {
                opts.errorCallback(err);
            } else {
                throw err;
            }
        });
    };
 
    // Toolkit's public members
    return {
        context: context,
        serverUrl: clientUrl,
        retrieve: retrieve,
        retrieveMultiple: retrieveMultiple,
        createRecord: createRecord,
        updateRecord: updateRecord,
        deleteRecord: deleteRecord,
        associate: associate,
        disassociate: disassociate,
        setState: setState,
        execute: execute,
        fetch: fetch
    };
})(window);
*/
//Venkat- Ends here

if (typeof (MCS) == "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
if (typeof (MCS.ResourcePackage) === "undefined") {
    MCS.ResourcePackage = {};
}

MCS.ResourcePackage.OnLoad = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() != 1) {
        formContext.getControl("cvt_specialty").setDisabled(true);
        formContext.getControl("cvt_specialtysubtype").setDisabled(true);
        formContext.getControl("cvt_providerlocationtype").setDisabled(true);
        formContext.getControl("cvt_patientlocationtype").setDisabled(true);
        formContext.getControl("cvt_availabletelehealthmodality").setDisabled(true);
        formContext.getControl("cvt_groupappointment").setDisabled(true);
        formContext.getControl("cvt_usagetype").setDisabled(true);
        if (formContext.getAttribute("cvt_usagetype").getValue() != 1) {
            //its a 'scheduling' package so hide the FA Tab
            formContext.ui.tabs.get("tab_approval").setVisible(false);
        }
        if (formContext.getAttribute("cvt_intraorinterfacility").getValue() != null)
            formContext.getControl("cvt_intraorinterfacility").setDisabled(true);
        if (formContext.getAttribute("cvt_patientfacility").getValue() != null)
            formContext.getControl("cvt_patientfacility").setDisabled(true);
        if (formContext.getAttribute("cvt_providerfacility").getValue() != null)
            formContext.getControl("cvt_providerfacility").setDisabled(true);
        formContext.getControl("cvt_hub").setDisabled(true);
    }
    else {
        formContext.getControl("cvt_usagetype").setDisabled(true);
    }
    MCS.ResourcePackage.SOS(executionContext);
    // MCS.ResourcePackage.HM(executionContext);
    MCS.ResourcePackage.Scope(executionContext);

    //4.8 Enhancement - Show FA Tab
    MCS.ResourcePackage.DisplayFATab(executionContext);

    if (formContext.getAttribute("cvt_patientfacility").getValue() != null)
        formContext.getControl("cvt_patientfacility").setDisabled(true);

};

MCS.ResourcePackage.NewFormSetControls = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() === 1) {
        // this is a new form
        if (formContext.getAttribute("cvt_intraorinterfacility").getValue() === 917290000) //intrafacility
        {
            formContext.getAttribute("cvt_usagetype").setValue(false);
            formContext.getControl("cvt_usagetype").setDisabled(true);
        }
        else {
            if (formContext.getAttribute("cvt_availabletelehealthmodality").getValue() == 917290002) { }
            else
                formContext.getControl("cvt_usagetype").setDisabled(false);
        }


        //commenting for now, since we can't seem to make up our mind.
        /*if (formContext.getAttribute("cvt_usagetype").getValue() == 1) {
            formContext.getAttribute("cvt_providerfacility").setRequiredLevel("recommended");
        }
        else {
            formContext.getAttribute("cvt_providerfacility").setRequiredLevel("required");
        }*/
    }
}

MCS.ResourcePackage.SOS = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //Make the SOS field visible and get the url
    if (formContext.getAttribute("cvt_specialty").getValue() != null) {

        Xrm.WebApi.retrieveRecord("mcs_servicetype", formContext.getAttribute("cvt_specialty").getValue()[0].id, "?$select=cvt_specialtyoperationssupplement").then(
            function success(result) {
                var url = result["cvt_specialtyoperationssupplement"];

                if (url) {
                    formContext.getAttribute("cvt_specialtyoperationsmanual").setValue(url);
                }
            },
            function (error) {
            }
        );

        //CrmRestKit.Retrieve('mcs_servicetype', Xrm.Page.getAttribute("cvt_specialty").getValue()[0].id, ["cvt_specialtyoperationssupplement"], false).fail(function (err) {
        //    //alert("fail");
        //}).done(function (data) {
        //    //alert("success");
        //    var url = data.d["cvt_specialtyoperationssupplement"];

        //    if (url) {
        //        Xrm.Page.getAttribute("cvt_specialtyoperationsmanual").setValue(url);
        //    }
        //});
    }
};

MCS.ResourcePackage.Scope = function (executionContext) {
    var formContext = executionContext.getFormContext();
    //if cvt_intraorinterfacility == intra
    var intraOrInter = formContext.getAttribute("cvt_intraorinterfacility").getValue();
    var provFacility = formContext.getAttribute("cvt_providerfacility").getValue();
    var patFacility = formContext.getAttribute("cvt_patientfacility").getValue();
    var isGroup = formContext.getAttribute("cvt_groupappointment").getValue();
    MCS.ResourcePackage.Group(executionContext);

    if (isGroup) {
        if (intraOrInter != null) {
            if (intraOrInter === 917290000) {
                //read only Patient facility
                formContext.getControl("cvt_patientfacility").setDisabled(true);
                //if provider facility is not equal to patient facility then set
                if (provFacility != patFacility)
                    formContext.getAttribute("cvt_patientfacility").setValue(provFacility);
            }
            else {
                //Patient facility is editable
                formContext.getControl("cvt_patientfacility").setDisabled(false);
            }
        }
        //NULL so set it to intrafacility
        else {
            formContext.getAttribute("cvt_intraorinterfacility").setValue(917290000)
            formContext.getControl("cvt_patientfacility").setDisabled(true);
            formContext.getAttribute("cvt_patientfacility").setValue(null);
        }
        formContext.getAttribute('cvt_patientfacility').setSubmitMode("always");
    }
};

MCS.ResourcePackage.Hub = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var hub = formContext.getAttribute("cvt_hub").getValue();
    var provFacility = formContext.getAttribute("cvt_providerfacility").getValue();

    if (hub != null) {
        formContext.getControl("cvt_providerfacility").setDisabled(true);
        if (hub != provFacility)
            formContext.getAttribute("cvt_providerfacility").setValue(hub);
    }
    else {
        formContext.getControl("cvt_providerfacility").setDisabled(false);
    }
};

MCS.ResourcePackage.setDefaultDurations = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var atrApptLength = formContext.getAttribute('cvt_appointmentlength');
    var atrStartEvery = formContext.getAttribute('cvt_startappointmentsevery');

    if (formContext.ui.getFormType() === 1) { //its a new form
        atrApptLength.setValue(60);
        atrStartEvery.setValue(15);
    }
    else {
        var valApptLength = atrApptLength.getValue();
        if ((!valApptLength) || (valApptLength === 0)) { atrApptLength.setValue(60) };

        var valStartEvery = atrStartEvery.getValue();
        if ((!valStartEvery) || (valStartEvery === 0)) { atrStartEvery.setValue(15) };
    }
}

MCS.ResourcePackage.HM = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_patientlocationtype").getValue() === 917290001) {
        formContext.ui.tabs.get('tab_approval').setVisible(false);
    }
};

// 4.8 Enhancement - Display FA Tab
MCS.ResourcePackage.DisplayFATab = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_intraorinterfacility").getValue() === 917290001) {
        if (formContext.getAttribute("cvt_usagetype").getValue() === 1) {
            if (formContext.getAttribute("cvt_patientlocationtype").getValue() === 917290001) {
                formContext.ui.tabs.get('tab_approval').setVisible(true);
            }
        }
    }
};

MCS.ResourcePackage.Group = function (executionContext) {
    var formContext = executionContext.getFormContext();
    if (formContext.getAttribute("cvt_groupappointment").getValue() === true) {
        formContext.getControl("cvt_patientfacility").setVisible(true);
        formContext.getAttribute('cvt_patientfacility').setRequiredLevel("required");
        formContext.getAttribute('cvt_patientfacility').setSubmitMode("dirty");
    }
    else {

        formContext.getControl("cvt_patientfacility").setVisible(false);
        formContext.getAttribute("cvt_patientfacility").setValue(null);
        formContext.getAttribute('cvt_patientfacility').setRequiredLevel("none");
    }
};

//If specialty value is removed, remove value of specailty sub type and make it read only
//if specialty value exists, make sub type editable
MCS.ResourcePackage.Specialty = function (executionContext) {
    var formContext = executionContext.getFormContext();
    var specialty = formContext.getControl("cvt_specialty");
    var specialtysubtype = formContext.getControl("cvt_specialtysubtype");

    if (formContext.getAttribute("cvt_specialty").getValue() != null) {
        specialtysubtype.setDisabled(false);
        MCS.ResourcePackage.SOS(executionContext);
    }
    else {
        formContext.getAttribute("cvt_specialtysubtype").setValue(null);
        specialtysubtype.setDisabled(true);
    }
};

//[Specialty] [Specialty Sub-Type]: Pat [CB or TW] Pro [CB or HM] [Ind or Grp]
MCS.ResourcePackage.CreateName = function (executionContext, eventObj) {
    var formContext = executionContext.getFormContext();
    var derivedResultField = "";

    var specialty = formContext.getAttribute("cvt_specialty").getValue();
    var specialtySubtype = formContext.getAttribute("cvt_specialtysubtype").getValue();
    var providerLocation = formContext.getAttribute("cvt_providerlocationtype").getValue();
    var patientLocation = formContext.getAttribute("cvt_patientlocationtype").getValue();
    var telehealthModality = formContext.getAttribute("cvt_availabletelehealthmodality").getValue();
    var isGroup = formContext.getAttribute("cvt_groupappointment").getValue();
    var intraOrInter = formContext.getAttribute("cvt_intraorinterfacility").getValue();

    var specialtyName = "";
    var providerLocationText = "Pro";
    var patientLocationText = "Pat";
    var telehealthModalityText = "";
    var isGroupText = "";
    var isInterText = "";


    if (specialty != null) {
        specialtyName = specialty[0].name;
        //query for potential abbreviation
        Xrm.WebApi.retrieveRecord("mcs_servicetype", formContext.getAttribute("cvt_specialty").getValue()[0].id, "?$select=cvt_abbreviation").then(
            function success(result) {
                var specialtyAbbrev = result["cvt_abbreviation"];

                if (specialtyAbbrev) {
                    specialtyName = specialtyAbbrev;
                }
            },
            function (error) {
            }
        );

        //CrmRestKit.Retrieve('mcs_servicetype', formContext.getAttribute("cvt_specialty").getValue()[0].id, ["cvt_abbreviation"], false).fail(function (err) {
        //}).done(function (data) {
        //    var specialtyAbbrev = data.d["cvt_abbreviation"];

        //    if (specialtyAbbrev) {
        //        specialtyName = specialtyAbbrev;
        //    }
        //});

        if (specialtySubtype != null) {
            //query for potential abbreviation
            Xrm.WebApi.retrieveRecord("mcs_servicesubtype", formContext.getAttribute("cvt_specialtysubtype").getValue()[0].id, "?$select=cvt_abbreviation").then(
                function success(result) {
                    var specialtySubtypeAbbrev = data.d["cvt_abbreviation"];

                    if (specialtySubtypeAbbrev)
                        specialtyName += ":" + specialtySubtypeAbbrev;
                    else
                        specialtyName += ":" + specialtySubtype[0].name;
                },
                function (error) {
                }
            );

            //CrmRestKit.Retrieve('mcs_servicesubtype', formContext.getAttribute("cvt_specialtysubtype").getValue()[0].id, ["cvt_abbreviation"], false).fail(function (err) {
            //}).done(function (data) {
            //    var specialtySubtypeAbbrev = data.d["cvt_abbreviation"];

            //    if (specialtySubtypeAbbrev)
            //        specialtyName += ":" + specialtySubtypeAbbrev;
            //    else
            //        specialtyName += ":" + specialtySubtype[0].name;
            //});
        }
    }

    if (providerLocation != null) {
        switch (providerLocation) {
            case 917290000:
                providerLocationText += " CB";
                break;
            case 917290001:
                providerLocationText += " TW";
                break;
        }
    }

    if (patientLocation != null) {
        switch (patientLocation) {
            case 917290000:
                patientLocationText += " CB";
                break;
            case 917290001:
                patientLocationText += " HM";
                break;
        }
    }

    if (telehealthModality != null) {
        switch (telehealthModality) {
            case 917290000:
                telehealthModalityText = "CVT";
                break;
            case 917290001:
                telehealthModalityText = "SFT";
                break;
        }
    }
    if (isGroup) {
        isGroupText = "Grp";
    }
    else
        isGroupText = "Ind";

    if (intraOrInter != null) {
        switch (intraOrInter) {
            case 917290000:
                isInterText = ", Intra";
                break;
            case 917290001:
                isInterText = ", Inter";
                break;
        }
    }

    derivedResultField = specialtyName + " " + telehealthModalityText + " - " + patientLocationText + ", " + providerLocationText + ", " + isGroupText + isInterText;

    if (telehealthModalityText === "SFT" && isGroupText === "Grp") {
        alert("Preventing Save. Cannot have a Group SFT Scheduling Package. Please change Group or Modality before saving again.")
        eventObj.getEventArgs().preventDefault();
        return;
    }

    if (formContext.getAttribute("cvt_name").getValue() != derivedResultField) {
        formContext.getAttribute("cvt_name").setSubmitMode("always");
        formContext.getAttribute("cvt_name").setValue(derivedResultField);
    }
};


MCS.ResourcePackage.ValidateRoles = function (executionContext, eventObj) {
    var formContext = executionContext.getFormContext();
    if (formContext.ui.getFormType() === 1) {
        //Detect if current user has System Administrator
        var hasSysAdminRole = MCS.cvt_Common.userHasRoleInList("System Administrator");
        if (hasSysAdminRole)
            return;

        //Determine the type of SP
        var isHub = formContext.getAttribute("cvt_hub").getValue();

        //Must have TMP TSA Manager to create a non Hub SP
        if (isHub == null) {
            var hasTSAManagerRole = MCS.cvt_Common.userHasRoleInList("TMP Scheduling Package Manager");
            if (!hasTSAManagerRole) {
                alert("Preventing Save. Necessary role (TMP Scheduling Package Manager) not detected.")
                eventObj.getEventArgs().preventDefault();
                return;
            }
        }
        else //Must have Hub TSA Manager to create a Hub SP
        {
            var hasHubTSAManagerRole = MCS.cvt_Common.userHasRoleInList("TMP Hub SP Manager");
            if (hasHubTSAManagerRole) {
                //Check if on correct hub team
                var onHubTeam = MCS.ResourcePackage.checkHubTeamMembership(isHub[0].id);
                if (!onHubTeam) {
                    alert("Preventing Save. User is not on correct Hub TSA Manager Team.")
                    eventObj.getEventArgs().preventDefault();
                    return;
                }
            }
            else {
                alert("Preventing Save. Necessary role (TMP Hub SP Manager) not detected.")
                eventObj.getEventArgs().preventDefault();
                return;
            }
        }
    }
};

MCS.ResourcePackage.checkHubTeamMembership = function (executionContext, hubId) {
    var formContext = executionContext.getFormContext();
    var teamFound = false;
    if (hubId === null)
        return false;

    //Get the correct Hub Team based on the hub facility listed

    Xrm.WebApi.retrieveMultipleRecords("Team", "?$select=Name,TeamId&$filter=cvt_Type/Value eq 917290010 and cvt_Facility/Id eq (Guid'" + hubId + "')").then(
        function success(result) {
            if (result && result.entities != null && result.entities.length != 0) {
                var filter = "SystemUserId eq (Guid' " + formContext.context.getUserId() + "') and TeamId eq (Guid' " + result.entities[0].TeamId + "')";
                //Query for any team membership records where the team ID equals the team listed and the userId of logged in user matches the TeamMemberShip UserId

                Xrm.WebApi.retrieveMultipleRecords("TeamMembership", "?$select=TeamId&$filter=" + filter).then(
                    function success(result1) {
                        if (result1 && result1.entities) {
                            teamFound = result1.entities.length > 0;
                        }
                    },
                    function (error) {
                        // console.log(error.message);
                        // handle error conditions
                    });
                //var query = CrmRestKit.ByQuery("TeamMembership", ['TeamId'], filter, false);
                //query.fail(function (err) {
                //    //alert("ERROR: " + err);
                //}).done(function (data) {
                //    if (data && data.d.results)
                //        teamFound = data.d.results.length > 0;
                //});
            }
        },
        function (error) {
            alert(MCS.cvt_Common.RestError(error));
        }
    );
    //var calls = CrmRestKit.ByQuery("Team", ['Name', 'TeamId'], "cvt_Type/Value eq 917290010 and cvt_Facility/Id eq (Guid'" + hubId + "')", false);
    //calls.fail(function (err) {
    //    //Fail
    //    alert(MCS.cvt_Common.RestError(err));
    //}).done(function (data) {
    //    if (data && data.d && data.d.results != null && data.d.results.length != 0) {
    //        var filter = "SystemUserId eq (Guid' " + Xrm.Page.context.getUserId() + "') and TeamId eq (Guid' " + data.d.results[0].TeamId + "')";
    //        //Query for any team membership records where the team ID equals the team listed and the userId of logged in user matches the TeamMemberShip UserId
    //        var query = CrmRestKit.ByQuery("TeamMembership", ['TeamId'], filter, false);
    //        query.fail(function (err) {
    //            //alert("ERROR: " + err);
    //        }).done(function (data) {
    //            if (data && data.d.results)
    //                teamFound = data.d.results.length > 0;
    //        });
    //    }
    //});
    return teamFound;
};
// added by Naveen
MCS.ResourcePackage.ShowHidePhoneInstructions = function (executionContext) {

    var formContext = executionContext.getFormContext();
    //we only want to see the 'Telephone Instructions' section when it's a Telephone SCHEDULING (not TSA package).  
    // if visible, its always editable whether its a new form or not
    if ((formContext.getAttribute("cvt_patientlocationtype").getValue() == 917290001) && (formContext.getAttribute("cvt_groupappointment").getValue() == 0)) {
        formContext.ui.tabs.get("tab_Details").sections.get("section_phone_instructions").setVisible(true);
    }
    else {
        formContext.ui.tabs.get("tab_Details").sections.get("section_phone_instructions").setVisible(false);
    }
};

//Duane M. 7/27/2020 added for updates to 4.8.1 telephone modality missing items
MCS.ResourcePackage.SetAppointmentGrpOnTelephoneModality = function (executionContext) {

    var formContext = executionContext.getFormContext();
    //when available telehealth modality is set to Telephone, Goup appointment is set to NO and field is readonly  
    if (formContext.getAttribute("cvt_availabletelehealthmodality").getValue() == 917290002) {
        formContext.getAttribute("cvt_groupappointment").setValue(false);
        formContext.getControl("cvt_groupappointment").setDisabled(true);
        formContext.getAttribute("cvt_usagetype").setValue(false);
        formContext.getControl("cvt_usagetype").setDisabled(true);
    }
    else {
        formContext.getControl("cvt_groupappointment").setDisabled(false);
    }
};