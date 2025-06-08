function form_onload(executionContext) {
	return;
	var formContext = executionContext.getFormContext();
	var wrControl = formContext.getControl("WebResource_PatientConsults");
	var consultsTab = formContext.ui.tabs.get("tab_9");
	consultsTab.addTabStateChange(ConsultsTabClicked);
	if (wrControl) {
		wrControl.getContentWindow().then(
			function (contentWindow) {
				contentWindow.setClientApiContext(Xrm, formContext);
			});
	}
}

function ConsultsTabClicked(executionContext) {
	// Consult Tab click   
}

function ProviderChanged(executionContext) {
	debugger;
	var formContext = executionContext.getFormContext();
	if (typeof window.top.SFT !== 'undefined') {
		Xrm.Page.getAttribute("cvt_relatedproviderid").setValue(window.top.SFT);
		delete window.top.SFT
	}

}

StaticVmrLinkValidate = function (executionContext) {
	//Set Default Visibility
	var formContext = executionContext.getFormContext();
	var staticvmrlink = formContext.getAttribute("tmp_staticvmrlink");

	if (staticvmrlink != null) {
		var staticvmrlinkValue = staticvmrlink.getValue();
		if (staticvmrlinkValue != null && staticvmrlinkValue.indexOf(' ') >= 0) {
			var message = "Enter a valid Static VMR Link without spaces.";
			formContext.getControl("tmp_staticvmrlink").setNotification(message);
		} else {
			formContext.getControl("tmp_staticvmrlink").clearNotification();
		}
	}
}