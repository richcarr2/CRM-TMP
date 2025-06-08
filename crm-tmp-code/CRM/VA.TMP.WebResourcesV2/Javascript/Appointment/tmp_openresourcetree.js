SetResourceFieldInterval = function (executionContext) {
    window.CheckForResourceField = function () {
        if (Xrm.Page.getAttribute("serviceid").getValue() != null) {
            try {
                var parameters = {};
                window.clearInterval(window.ResourceFieldinterval);
                parameters.ServiceId = Xrm.Page.getAttribute("serviceid").getValue()[0].id;
                //"87af6027-b572-ea11-a814-001dd801877a";
                parameters.InputSource = "FormAssistant";
                var req = new XMLHttpRequest();
                req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v9.1/_GetServiceTreeData", false);
                req.setRequestHeader("OData-MaxVersion", "4.0");
                req.setRequestHeader("OData-Version", "4.0");
                req.setRequestHeader("Accept", "application/json");
                req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                req.onreadystatechange = function () {
                    if (this.readyState === 4) {
                        req.onreadystatechange = null;
                        if (this.status === 200) {
                            var results = JSON.parse(this.response);

                            window.top.results = results;
                            window.top.results.TreeObject = JSON.parse(window.top.results.TreeObject);
                            // window.showModal('/webresources/tmp_servicetree', null, { edge: 'sunken', center: 'yes', height: '450px', width: '450px' })
                        } else {
                            Xrm.Utility.alertDialog(this.statusText);
                            appInsights.trackException(this.statusText);
                        }
                    }
                };
                req.send(JSON.stringify(parameters));
                var formcontext = executionContext.getFormContect()

            } catch (ex) {
                appInsights.trackException(ex);
            }            //rmcontext.ui.controls.get("IFRAME_ResourceTree").setSrc('/Webresources/tmp_servicetree');
        }

    }
    window.ResourceFieldinterval = window.setInterval(CheckForResourceField, 1000)

}