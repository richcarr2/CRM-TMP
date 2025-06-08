if (typeof (MCS) === "undefined") { MCS = {}; }

// Create Namespace container for functions in this library;
MCS.ParticipatingSiteOnLoad = {};
window.top.MCS = MCS;

MCS.ParticipatingSiteOnLoad.filterSites = function (executionContext) {
	//console.log("begin filterSites function");
	var formContext = executionContext.getFormContext();

	var _spGuid = "";
	var _spName = "";
	var _psSide = "";
	var _grpAppt = false;
	var _proFac = "";
	var _patFac = "";
	var _inter = false;



	var SiteFilterDeferred = MCS.ParticipatingSiteOnLoad.getValuesOnChange(executionContext);
	$.when(SiteFilterDeferred).done(function (returnData) {
		if (returnData.data !== null) {
			_spGuid = returnData.data.spGuid.replace("{", "").replace("}", "");
			_spName = returnData.data.spName;
			_inter = returnData.data.interfacility;

			if (returnData.data._grpAppt === true) {
				_grpAppt = true;
			}

			if (returnData.data.providerfacility !== null) {
				_proFac = returnData.data.providerfacility;
			}

			if (returnData.data.patientfacility !== null) {
				_patFac = returnData.data.patientfacility;
			}

			if (returnData.data.interfacility === true) {
				_inter = true;
			}

			if (returnData.data.psSide === 917290000) {
				_psSide = "pro";
			} else {
				_psSide = "pat";
			}
		}

		//this is where we determine which filter to call:
		//console.log("begin filter logic");

		//console.log("Interfacility: " + _inter);
		//console.log("Group: " + _grpAppt);
		//console.log("Side: " + _psSide);

		if (!_grpAppt) {
			//kconsole.log("in INDIVIDUAL Appointment Branch.  Next: Checking for Side");
			if (!_inter)//not an interfacility appointment
			{
				if (_psSide === "pro") {
					//console.log("calling PROVIDER-INDIVIDUAL filter!");
					//alert("calling PROVIDER-INDIVIDUAL filter!\n" + _proFac);
					formContext.getControl("cvt_site").addPreSearch(function () {
						MCS.ParticipatingSiteOnLoad.addCustomSiteFilter(_proFac, formContext);
					});
				}
				else {
					//console.log("calling PATIENT-INDIVIDUAL filter!\n" + _patFac);
					//alert("calling PATIENT-INDIVIDUAL filter!");
					formContext.getControl("cvt_site").addPreSearch(function () {
						MCS.ParticipatingSiteOnLoad.addCustomSiteFilter(_proFac, formContext);
					});
				}
			}
			else//it is an interfacility appointment
			{
				//                formContext.getControl("cvt_site").removePreSearch(function () {
				//                    MCS.ParticipatingSiteOnLoad.addCustomSiteFilter(_proFac, formContext);
				//                });
				//                formContext.getControl("cvt_site").removePreSearch(function () {
				//                    MCS.ParticipatingSiteOnLoad.clearCustomSiteFilter(formContext);
				//                });
				//                    
				if (_psSide === "pro") {
					//console.log("calling PROVIDER-INDIVIDUAL filter!");
					//alert("calling PROVIDER-INDIVIDUAL filter!\n" + _proFac);
					formContext.getControl("cvt_site").addPreSearch(function () {
						MCS.ParticipatingSiteOnLoad.addCustomSiteFilter(_proFac, formContext);
					});
				}
				else {
					//console.log("NO PATIENT-INDIVIDUAL filter!\n This is an INDIVIDUAL INTERFACILITY APPOINTMENT");
					formContext.getControl("cvt_site").addPreSearch(function () {
						MCS.ParticipatingSiteOnLoad.clearCustomSiteFilter(formContext);
					});

					formContext.getControl("cvt_site").removePreSearch(function () {
						MCS.ParticipatingSiteOnLoad.addCustomSiteFilter(_proFac, formContext);
					});
					//formContext.getControl("cvt_site").removePreSearch(MCS.ParticipatingSiteOnLoad.addCustomSiteFilter(_proFac, formContext));


				}
			}

		}
		else {
			//console.log("in GROUP Appointment Branch.  Next: Checking for Side");
			//console.log("for group appointments it doesn't matter if its inter- or intra-facility.  The filters are the same.")
			if (_psSide === "pro") {
				//console.log("calling PROVIDER-GROUP filter!");
				formContext.getControl("cvt_site").addPreSearch(function () {
					MCS.ParticipatingSiteOnLoad.addCustomSiteFilter(_proFac, formContext);
				});
			}
			else {
				//console.log("calling PATIENT-GROUP filter!");
				formContext.getControl("cvt_site").addPreSearch(function () {
					MCS.ParticipatingSiteOnLoad.addCustomSiteFilter(_patFac, formContext);
				});
			}
		}
		//console.log("end filter logic");
	});
	//console.log("end filterSites function");
};


MCS.ParticipatingSiteOnLoad.addCustomSiteFilter = function (facilityId, formContext) {
	var functionName = "MCS.ParticipatingSiteOnLoad.addCustomSiteFilter";
	try {
		fetchXml = "<filter type='and'><condition attribute='mcs_facilityid' value='" + facilityId + "' uitype='mcs_facility' operator='eq'/></filter>"
		formContext.getControl("cvt_site").addCustomFilter(fetchXml);
	}
	catch (e) {
		//throw erro
		//console.log(e.message);
		throw new Error(e.message);
	}
};

MCS.ParticipatingSiteOnLoad.clearCustomSiteFilter = function (formContext) {
	var functionName = "MCS.ParticipatingSiteOnLoad.clearCustomSiteFilter";
	try {
		fetchXml = "<filter type='and'><condition attribute='mcs_facilityid' value='11111111-aaaa-bbbb-cccc-222222222222' uitype='mcs_facility' operator='ne'/></filter>"
		formContext.getControl("cvt_site").addCustomFilter(fetchXml);
	}
	catch (e) {
		//throw error
		//console.log(e.message);
		throw new Error(e.message);
	}
};

MCS.ParticipatingSiteOnLoad.getValuesOnChange = function (executionContext) {
	//console.log("begin getValuesOnChange function");
	var deferred = $.Deferred();
	var returnData = {
		success: true,
		data: {
		}
	};

	var formContext = executionContext.getFormContext();
	var _spGuid = formContext.getAttribute("cvt_resourcepackage").getValue()[0].id;
	var _spName = formContext.getAttribute("cvt_resourcepackage").getValue()[0].name;
	var _psSide = formContext.getAttribute("cvt_locationtype").getValue();

	returnData.data.spGuid = _spGuid;
	returnData.data.spName = _spName;
	returnData.data.psSide = _psSide;

	Xrm.WebApi.retrieveRecord('cvt_resourcepackage', _spGuid, "?$select=cvt_groupappointment,_cvt_providerfacility_value,_cvt_patientfacility_value,cvt_intraorinterfacility").then(
		function success(result) {
			//console.log("got Scheduling Package!!")
			if (result !== null) {
				if (result.cvt_groupappointment) {
					returnData.data._grpAppt = true;
				} else {
					returnData.data._grpAppt = false;
				}

				if (result._cvt_providerfacility_value !== null) {
					returnData.data.providerfacility = result._cvt_providerfacility_value;
				}

				if (result._cvt_patientfacility_value !== null) {
					returnData.data.patientfacility = result._cvt_patientfacility_value;
				}

				if (result.cvt_intraorinterfacility === 917290001) {
					returnData.data.interfacility = true;
				}
				else {
					returnData.data.interfacility = false;
				}
			}
			deferred.resolve(returnData);
		},
		function (error) {
			//console.log("failed to retrieve Scheduling Package, Unable to filter sites: ");
			returnData.success = false;
			deferred.resolve(returnData);
		}
	);

	//console.log("end getValuesOnChange function");
	return deferred.promise();

}
