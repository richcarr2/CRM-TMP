function ExecuteWorkflow() {
    //show indication to user
    Xrm.Utility.showProgressIndicator("Executing Workflow....");
    var serverURL = Xrm.Page.context.getClientUrl();
    var contactID = Xrm.Page.data.entity.getId().substring(1, 37);
    //workflow id that we captured above
    var workflowID = "87DA1F2A-6FE7-420D-A3FF-9E1C8BD4A4AF";

    var data = {
        "EntityId": serviceappointment
    };

    var req = new XMLHttpRequest();
    req.open("POST", serverURL + "/api/data/v8.2/workflows(" + workflowID + ")/Microsoft.Dynamics.CRM.ExecuteWorkflow", true);
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.onreadystatechange = function () {
        if (this.readyState == 4
        /* complete */
        ) {
            req.onreadystatechange = null;
            if (this.status == 200) {
                Xrm.Utility.closeProgressIndicator();
            } else {
                var error = JSON.parse(this.response).error;
                alert(error.message);
                Xrm.Utility.closeProgressIndicator();
            }
        }
    };
    req.send(JSON.stringify(data));
}