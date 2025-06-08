//If the SDK namespace object is not defined, create it.
//SD: web-use-strict-equality-operators
if (typeof (MCS) === "undefined") { MCS = {}; }
// Create Namespace container for functions in this library;
MCS.mcs_Service_Activity = {};
//DO NOT MANUALLY ADD CODE TO THIS FILE AS THIS FILE IS COMPLETELY RE-WRITTEN FROM CRM Rules! EVERY TIME THIS ENTITY IS DEPLOYED
MCS.mcs_Service_Activity.FORM_TYPE_CREATE = 1;
MCS.mcs_Service_Activity.FORM_TYPE_UPDATE = 2;
MCS.mcs_Service_Activity.FORM_TYPE_READ_ONLY = 3;
MCS.mcs_Service_Activity.FORM_TYPE_DISABLED = 4;

MCS.mcs_Service_Activity.GetTSAData = function () {
	//SD - Start
	//web-use-strict-mode
	"use strict";
	//SD - End
	/***********************************************************************
	/** 
	/** Description: GetTSAData - Gets the TSA Data and populates fields on Service Activity
	/** 
	/** 
	***********************************************************************/
	//SD web-use-client-context
	//'Xrm.Page' references or accesses a deprecated API in the client context object model. Replace this call with the following client context API: 'ExecutionContext.getFormContext'
	var mcs_relatedtsa = ExecutionContext.getFormContext().getAttribute("mcs_relatedtsa").getValue();
	//var mcs_relatedtsa= Xrm.Page.getAttribute("mcs_relatedtsa").getValue();
	if (mcs_relatedtsa != null) {
		MCS.mcs_Service_Activity.getmcs_relatedtsaLookupData(mcs_relatedtsa[0].id);
	}
	else {
		//SD web-use-client-context
		//'Xrm.Page' references or accesses a deprecated API in the client context object model. Replace this call with the following client context API: 'ExecutionContext.getFormContext'
		ExecutionContext.getFormContext().getAttribute("serviceid").setValue(null);
		ExecutionContext.getFormContext().getAttribute("new_capacity").setValue(null);
		//Xrm.Page.getAttribute("serviceid").setValue(null);
		//Xrm.Page.getAttribute("new_capacity").setValue(null);
	}
}
MCS.mcs_Service_Activity.getmcs_relatedtsaLookupData = function (mcs_relatedtsa) {
	MCS.mcs_Service_Activity.retrievemcs_relatedtsaData(mcs_relatedtsa);
}
MCS.mcs_Service_Activity.retrievemcs_relatedtsaData = function (mcs_relatedtsa) {
	var retrievemcs_relatedtsaInfoReq = new XMLHttpRequest();
	var request = MCS.GlobalFunctions._getServerUrl("ODATA") + "/mcs_servicesSet?$select=mcs_RelatedServiceId,mcs_Capacity&$filter=mcs_servicesId eq guid'" + mcs_relatedtsa + "'";
	retrievemcs_relatedtsaInfoReq.open("GET", request, true);
	retrievemcs_relatedtsaInfoReq.setRequestHeader("Accept", "application/json");
	retrievemcs_relatedtsaInfoReq.setRequestHeader("Content-Type", "application/json; charset=utf-8");
	retrievemcs_relatedtsaInfoReq.onreadystatechange = function () { MCS.mcs_Service_Activity.retrievemcs_relatedtsaInfoReqCallBack(this); };
	retrievemcs_relatedtsaInfoReq.send();
}
MCS.mcs_Service_Activity.retrievemcs_relatedtsaInfoReqCallBack = function (retrievemcs_relatedtsaInfoReq) {
	//SD: web-use-strict-equality-operators
	if (retrievemcs_relatedtsaInfoReq.readyState === 4 /* complete */) {
		if (retrievemcs_relatedtsaInfoReq.status === 200) {
			//Success
			var retrievemcs_relatedtsaInfo = window.JSON.parse(retrievemcs_relatedtsaInfoReq.responseText).d;
			if (retrievemcs_relatedtsaInfo.results[0] != null) {
				//check to see if the control is on the form before you try to setvalue on it
				//SD web-use-client-context
				//'Xrm.Page' references or accesses a deprecated API in the client context object model. Replace this call with the following client context API: 'ExecutionContext.getFormContext'
				if (ExecutionContext.getFormContext().getAttribute("serviceid") != null) {
					//if (Xrm.Page.getAttribute("serviceid") != null){
					if (retrievemcs_relatedtsaInfo.results[0].mcs_RelatedServiceId != null) {
						if (retrievemcs_relatedtsaInfo.results[0].mcs_RelatedServiceId.Id != null) {
							var olookup = new Object();
							olookup.id = retrievemcs_relatedtsaInfo.results[0].mcs_RelatedServiceId.Id;
							olookup.entityType = retrievemcs_relatedtsaInfo.results[0].mcs_RelatedServiceId.LogicalName;
							olookup.name = retrievemcs_relatedtsaInfo.results[0].mcs_RelatedServiceId.Name;
							var olookupValue = new Array();
							olookupValue[0] = olookup;
							//SD web-use-client-context
							//'Xrm.Page' references or accesses a deprecated API in the client context object model. Replace this call with the following client context API: 'ExecutionContext.getFormContext'
							var currentAttribute = ExecutionContext.getFormContext().getAttribute("serviceid");
							//var currentAttribute = Xrm.Page.getAttribute("serviceid");
							if (currentAttribute != null) {
								var currentAttributeValue = currentAttribute.getValue();
								//SD: web-use-strict-equality-operators
								if (currentAttributeValue !== null) {
									if (currentAttributeValue[0].id != olookup.id) {
										//SD web-use-client-context
										ExecutionContext.getFormContext().getAttribute("serviceid").setValue(olookupValue);
										//Xrm.Page.getAttribute("serviceid").setValue(olookupValue);
									}
								} else {
									//SD web-use-client-context
									ExecutionContext.getFormContext().getAttribute("serviceid").setValue(olookupValue);
									//Xrm.Page.getAttribute("serviceid").setValue(olookupValue);
								}
							}
						} else {
							//SD web-use-client-context
							ExecutionContext.getFormContext().getAttribute("serviceid").setValue(null);
							//Xrm.Page.getAttribute("serviceid").setValue(null);
						}
						//SD web-use-client-context
						ExecutionContext.getFormContext().getAttribute("serviceid").fireOnChange();
						//Xrm.Page.getAttribute("serviceid").fireOnChange();
					} else {
						//SD web-use-client-context
						ExecutionContext.getFormContext().getAttribute("serviceid").setValue(null);
						//Xrm.Page.getAttribute("serviceid").setValue(null);
					}
				}
				//check to see if the control is on the form before you try to setvalue on it
				//SD web-use-client-context
				if (ExecutionContext.getFormContext().getAttribute("new_capacity") != null) {
					//if (Xrm.Page.getAttribute("new_capacity") != null){
					if (retrievemcs_relatedtsaInfo.results[0].mcs_Capacity != null) {
						var mcs_Capacitytst = retrievemcs_relatedtsaInfo.results[0].mcs_Capacity;
						if (mcs_Capacitytst != null) {
							//SD web-use-client-context
							ExecutionContext.getFormContext().getAttribute("new_capacity").setValue(parseFloat(mcs_Capacitytst));
							//Xrm.Page.getAttribute("new_capacity").setValue(parseFloat(mcs_Capacitytst));
						} else {
							//SD web-use-client-context
							ExecutionContext.getFormContext().getAttribute("new_capacity").setValue(null);
							//Xrm.Page.getAttribute("new_capacity").setValue(null);
						}
						//SD web-use-client-context
						ExecutionContext.getFormContext().getAttribute("new_capacity").fireOnChange();
						//Xrm.Page.getAttribute("new_capacity").fireOnChange();
					} else {
						//SD web-use-client-context
						ExecutionContext.getFormContext().getAttribute("new_capacity").setValue(null);
						//Xrm.Page.getAttribute("new_capacity").setValue(null);
					}
				}
			} else {
				alert('No records were returned from the copy function, please contact your administrator')
			}
		} else {
			//Failure
			MCS.GlobalFunctions.errorHandler(retrievemcs_relatedtsaInfoReq);
		}
	}
};
MCS.mcs_Service_Activity.EnableTSA = function () {
	/***********************************************************************
	/** 
	/** Description: Enable TSA - once site is picked, show this field
	/** 
	/** 
	***********************************************************************/
	var mcs_relatedsite = null
	//SD web-use-client-context
	var mcs_relatedsiteattribute = ExecutionContext.getFormContext().getAttribute("mcs_relatedsite");
	//var mcs_relatedsiteattribute = Xrm.Page.getAttribute("mcs_relatedsite");
	if (mcs_relatedsiteattribute != null) {
		//SD web-use-client-context
		mcs_relatedsite = ExecutionContext.getFormContext().getAttribute("mcs_relatedsite").getValue();
		//mcs_relatedsite = Xrm.Page.getAttribute("mcs_relatedsite").getValue();
	}

	var mcs_relatedsiteSLU = null;
	if (mcs_relatedsite != null) {
		mcs_relatedsiteSLU = mcs_relatedsite[0].name;
	}



	if (mcs_relatedsiteSLU != null) {
		//SD web-use-client-context
		ExecutionContext.getFormContext().ui.controls.get("mcs_relatedtsa").setVisible(true);
		ExecutionContext.getFormContext().ui.controls.get("mcs_relatedtsa").setFocus();
		//Xrm.Page.ui.controls.get("mcs_relatedtsa").setVisible(true);
		//Xrm.Page.ui.controls.get("mcs_relatedtsa").setFocus();
	}
};