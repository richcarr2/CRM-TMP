/************** udo_process.js ******************************************/
var MCS = MCS || {};
MCS.Scripts = MCS.Scripts || {};

MCS.Scripts.Process = function () {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var serverUrl = null;

    var DataTypes = {
        Bool: "boolean",
        Int: "int",
        String: "string",
        DateTime: "dateTime",
        EntityReference: "EntityReference",
        OptionSet: "OptionSetValue",
        Money: "Money",
        Guid: "guid"
    };

    var soapParams = function (paramArray, genericNSPrefix, schemaNSPrefix) {

        var xmlEncode = function (input) {
            var between = function (i, a, b) { return (i > a && i < b); };
            //SD - Start
            ////web-use-strict-equality-operators
            if (typeof input === "undefined" || input === null || input === '') return '';
            //SD - End

            var output = '';
            for (var i = 0; i < input.length; i++) {
                var c = input.charCodeAt(i);
                //SD - Start
                ////web-use-strict-equality-operators
                if (between(c, 96, 123) || between(c, 64, 91) || between(c, 47, 58) || between(c, 43, 47) || c === 95 || c === 32) {
                    //SD - End
                    output += String.fromCharCode(c);
                } else {
                    output += "&#" + c + ";";
                }
            }
            return output;
        };

        var params = "";
        var value = "";

        if (paramArray) {
            // Add each input param
            for (var i = 0; i < paramArray.length; i++) {
                var param = paramArray[i];
                var includeNS = false;
                var type = ":" + param.Type;
                var typeNS = "http://www.w3.org/2001/XMLSchema";

                switch (param.Type) {
                    case "dateTime":
                        value = param.Value.toISOString();
                        type = schemaNSPrefix + type;
                        includeNS = true;
                        break;
                    case "EntityReference":
                        type = "a" + type;
                        value = "<a:Id>" + param.Value.id + "</a:Id><a:LogicalName>" + param.Value.entityType + "</a:LogicalName><a:Name i:nil='true' />";
                        break;
                    case "OptionSetValue":
                    case "Money":
                        type = "a" + type;
                        value = "<a:Value>" + param.Value + "</a:Value>";
                        break;
                    case "guid":
                        type = schemaNSPrefix + type;
                        value = param.Value;
                        includNS = true;
                        typeNS = "http://schemas.microsoft.com/2003/10/Serialization/";
                        break;
                    case "string":
                        type = schemaNSPrefix + type;
                        value = xmlEncode(param.Value);
                        includeNS = true;
                        break;
                    default:
                        type = schemaNSPrefix + type;
                        value = param.Value;
                        includeNS = true;
                        break;
                }

                params += "<a:KeyValuePairOfstringanyType>" +
                    "<" + genericNSPrefix + ":key>" + param.Key + "</" + genericNSPrefix + ":key>" +
                    "<" + genericNSPrefix + ":value i:type='" + type + "'";
                if (includeNS) params += " xmlns:" + schemaNSPrefix + "='" + typeNS + "'";
                params += ">" + value + "</" + genericNSPrefix + ":value>" +
                    "</a:KeyValuePairOfstringanyType>";
            }
        }

        return "<a:Parameters xmlns:" + genericNSPrefix + "='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>" +
            params + "</a:Parameters>";
    };

    var soapExecute = function (requestXml) {
        return "<Execute xmlns='http://schemas.microsoft.com/xrm/2011/Contracts/Services' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>" +
            requestXml + "</Execute>";
    };

    var soapEnvelope = function (message) {
        return "<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>" +
            "<s:Body>" + message + "</s:Body>" +
            "</s:Envelope>";
    };

    var soapActionRequest = function (action, inputParams) {
        return "<request xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts'>" +
            soapParams(inputParams, 'b', 'c') +
            "<a:RequestId i:nil='true' />" +
            "<a:RequestName>" + action + "</a:RequestName>" +
            "</request>";
    };

    var soapExecuteWorkflowRequest = function (workflowId, recordId) {
        return "<request i:type='b:ExecuteWorkflowRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' xmlns:b='http://schemas.microsoft.com/crm/2011/Contracts'>" +
            soapParams([{ Key: "EntityId", Type: DataTypes.Guid, Value: recordId },
            { Key: "WorkflowId", Type: DataTypes.Guid, Value: workflowId }], 'c', 'd') +
            "<a:RequestId i:nil='true' />" +
            "<a:RequestName>ExecuteWorkflow</a:RequestName>" +
            "</request>";
    };

    var execCrmSoapRequest = function (soapMessage) {
        //SD - Start 
        //web-avoid-crm2011-service-soap
        //https://community.dynamics.com/crm/f/microsoft-dynamics-crm-forum/311096/is-organization-service-is-going-to-be-deprecated
        //The URL '/xrmservices/2011/organization.svc/web' targets a deprecated Dynamics CRM 2011 SOAP endpoint. 
        //The Dynamics CRM 2011 SOAP endpoints were deprecated in Dynamics CRM 2016(v8.0) and will soon be considered unsupported.
        //Replace with the preferred Web API endpoint '/api/data/[version]' which implements the OData 4.0 messaging protocol.
        //var orgServicePath = "/xrmservices/2011/organization.svc/web";
        var orgServicePath = "/api/data/v9.1";
        if (serverUrl == null) {
            //SD - Start
            //web-use-client-context
            serverUrl = Xrm.Utility.getGlobalContext().getClientUrl();
            //serverUrl = Xrm.Page.context.getClientUrl();
            //SD - End
            //serverUrl += "/XRMServices/2011/Organization.svc/web";
            serverUrl += orgServicePath;
            serverUrl = serverUrl.replace("//XRMServices", "/XRMServices");
        }
        //   if (serverUrl == null) {
        //          serverUrl = Xrm.Page.context.getClientUrl();
        //          serverUrl += "/XRMServices/2011/Organization.svc/web";
        //          serverUrl = serverUrl.replace("//XRMServices", "/XRMServices");
        //}
        //SD - End

        var options = {
            url: serverUrl,
            type: "POST",
            dataType: "xml",
            data: soapMessage,
            processData: false,
            global: false,
            beforeSend: function (xhr) {
                xhr.setRequestHeader('SOAPAction', 'http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute');
                xhr.setRequestHeader("Accept", "application/json, text/xml */*");
                xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            }
        };

        return $.ajax(options);
        return result;
    };

    var callAction = function (action, inputParams) {
        var dfd = $.Deferred();

        execCrmSoapRequest(soapEnvelope(soapExecute(soapActionRequest(action, inputParams))))
            .done(function (a, b, xhr) {
                var result = getValues(xhr.responseXML);
                dfd.resolve(result, b, xhr);
            })
            .fail(function (err) {
                dfd.reject(err);
            });

        return dfd.promise();
    };

    var getValues = function (xmlData) {
        var XmlToEntity = function (node) {
            try {
                //ToDo: This code needs to be validated
                var entity = {
                    logicalName: node.getElementsByTagName("a:LogicalName")[0].text(),
                    id: node.getElementsByTagName("a:Id")[0].text(),
                    attributes: getValues(node.getElementsByTagName("a:Attributes")[0])
                };
            } catch (err) {
                return null;
            }
            try {
                var formattedValuesNode = node.getElementsByTagName("a:FormattedValues");
                if (formattedValuesNode != null && formattedValuesNode.length > 0) {
                    entity.formattedValues = getValues(formattedValuesNode);
                }
            } catch (err) { }
            return entity;
        };

        var XmlToEntities = function (node) {
            var xmlEntities = node.getElementsByTagName("a:Entity");
            var entities = [];
            for (var i = 0; i < xmlEntities.length; i++) {
                entities[i] = XmlToEntity(xmlEntities[i]);
            }
            return entities;
        };

        var kvps = xmlData.getElementsByTagName("a:KeyValuePairOfstringanyType");
        //SD - Start
        //web-use-strict-equality-operators
        if (typeof kvps === "undefined" || kvps === null || kvps.length === 0) {
            kvps = xmlData.getElementsByTagName("KeyValuePairOfstringanyType");
        }
        if (typeof kvps === "undefined" || kvps === null || kvps.length === 0) {
            kvps = [];
        } else {
            kvps = kvps[0].parentNode.childNodes;
        }
        //SD - End
        var result = {};
        for (var i = 0; i < kvps.length; i++) {
            var key = $(kvps[i].childNodes[0]).text();
            var valueObj = $(kvps[i].childNodes[1]);
            var typeNode = valueObj.attr("i:type");
            // continue if no type (like null values)
            //SD: web-use-strict-equality-operators: Only one line below - Added one extra equality operator to make == tpo ===
            if (typeof typeNode == "undefined" || typeNode === null) continue;
            // get the type from the node
            var type = valueObj.attr("i:type").toLowerCase();
            type = type.substring(type.indexOf(":") + 1);

            // setup value variable.
            var value = "";
            //SD: web-use-strict-equality-operators: Only one line below - Added one extra equality operator to make == tpo ===
            if (type === "aliasedvalue") {
                for (var j = 0; j < valueObj[0].childNodes.length; j++) {
                    //SD: web-use-strict-equality-operators: Only one line below - Added one extra equality operator to make == tpo ===
                    if (valueObj[0].childNodes[j].tagName === "a:Value") {
                        valueObj = $(valueObj.childNodes[j]);
                        break;
                    }
                }
                // reset type using the aliasedvalue result
                type = valueObj.attr("i:type").toLowerCase();
                type = type.substring(type.indexOf(":") + 1);
            }
            switch (type) {
                case "entity":
                    value = XmltoEntity(valueObj);
                    break;
                case "entitycollection":
                    value = XmlToEntities(valueObj[0]);
                    break;
                case "entityreference":
                    value = {
                        id: $(valueObj[0].childNodes[0]).text(),
                        entityType: $(valueObj[0].childNodes[1]).text()
                    };
                    if (valueObj[0].childNodes[2]) value.name = $(valueObj[0].childNodes[2]).text();
                    break;
                case "datetime":
                    value = new Date(valueObj.text());
                    break;
                case "decimal":
                case "double":
                case "int":
                case "money":
                case "optionsetvalue":
                    value = Number(valueObj.text());
                    break;
                case "boolean":
                    //SD: web-use-strict-equality-operators: Only one line below - Added one extra equality operator to make == tpo ===
                    value = valueObj.text().toLowerCase() === "true";
                    break;
                default: //string
                    value = valueObj.text();
                    break;
            }

            result[key] = value;
        }
        return result;
    };

    var callWorkflow = function (workflowId, recordId) {
        return execCrmSoapRequest(soapEnvelope(soapExecute(soapExecuteWorkflowRequest(workflowId, recordId))));
    };

    return {
        DataType: DataTypes,
        ExecuteAction: callAction,
        ExecuteWorkflow: callWorkflow
    };
}();

/*
var Initialize = function () {

    var requestName = "Your_actionname",

    var requestParams =
        [{
            Key: "ParentEntityReference",
            Type: Va.Udo.Crm.Scripts.Process.DataType.EntityReference,
            Value: { id: entityId, entityType: entityName }
        }]

    MCS.Scripts.Process.ExecuteAction(requestName, requestParams)
        .done(function (response) { onResponse(response, requestName) })
        .fail(function (err) {
            $('#loadingGifDiv').hide();
            if (Debug == false)
                $('#notFoundDiv').text("An error occurred while attempting to process this request. Please refresh the page and try again. If this error persists, please contact the application support team.");
            else
                $('#notFoundDiv').text(err.responseText);

            $('#notFoundDiv').show();
        });
};

function onResponse(responseObject, requestName) {
    $('#loadingGifDiv').hide();
    if (responseObject.DataIssue != false || responseObject.Timeout != false || responseObject.Exception != false) {
        $('#notFoundDiv').text(responseObject.ResponseMessage);
        $('#notFoundDiv').show();
    } else {
        //Do whatever
    }
}
*/