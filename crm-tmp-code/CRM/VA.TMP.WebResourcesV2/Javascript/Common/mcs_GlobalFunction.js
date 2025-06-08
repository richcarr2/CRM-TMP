
/***********************************************************************
/** 
/** MCSGlbal Functions.js
/** Description: Global rules called by form level jscripts 
/** 
***********************************************************************/
//If the MCS namespace object is not defined, create it.
if (typeof(MCS) === "undefined") {
    MCS = {
        __namespace: true
    };
}
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
            } catch(e) {
                try {
                    xhr = new ActiveXObject("Microsoft.XMLHTTP");
                } catch(e) {
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
        else {
            monthString = rawMonth;
        }

        var dateString;
        var rawDate = date.getUTCDate().toString();
        if (rawDate.length === 1) {
            dateString = "0" + rawDate;
        }
        else {
            dateString = rawDate;
        }

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
            else {
                throw new Error("Unable to access the server URL");
            }
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
        req.onreadystatechange = function () {
            runResponse(req);
        };
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
        req.onreadystatechange = function () {
            createResponse.parseResponse(req);
        };
        req.send(request);

    },
    createResponse: function () {
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
        req.onreadystatechange = function () {
            retrieveResponse._parseResponse(req);
        };
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
    updateRequest: function () {},
    updateResponse: function () {},
    deleteRequest: function () {},
    deleteResponse: function () {},
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
                if (attributes[i].value.Name == null) {
                    attribute += "<b:Name i:nil=\"true\"/>";
                }
                else {
                    attribute += "<b:Name>" + attributes[i].value.Name + "</b:Name>";
                }
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
            return {
                allColumns: false,
                columns: []
            };
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
                    if (stringArray) {
                        arrColumns = columns;
                    }
                    else {
                        throw new Error(errorMessage);
                    }
                }
                else {
                    throw new Error(errorMessage);
                }
                break;
            default:
                throw new Error(errorMessage);
                break;
            }
            return {
                allColumns: false,
                columns: arrColumns
            };
        }
    },
    _getColumnSet: function (columnSet) {

        var col = "<columnSet xmlns:b=\"http://schemas.microsoft.com/xrm/2011/Contracts\"";
        col += " xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\">";
        if (columnSet.allColumns === true) {
            col += "<b:AllColumns>true</b:AllColumns>";
        }
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
            catch(e) {};
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
                if (this._error != null) {
                    return this._error;
                }
                else {
                    throw new Error("No error exists.");
                }
            },
            getState: function () {
                return this._state;
            },
            getData: function () {
                if (this._state === "complete") {
                    return this._data;
                }
                else {
                    throw new Error("Data is not ready yet.");
                }
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
        if (createReq.readyState === 4
        /* complete */
        ) {
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
        if (retrieveRecordsReq.readyState === 4
        /* complete */
        ) {
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
                    alert("Error : " + XmlHttpRequest.status + ": " + XmlHttpRequest.statusText + ": " + JSON.parse(XmlHttpRequest.responseText).error.message.value);
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