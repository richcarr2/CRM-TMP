// JavaScript source code


function Onload(context) {
    debugger;
    var formContext = context.getFormContext();
    var req = {};
    var action = "tmp_GetPatientFlags"; // Action Name

    var foundRole = hasRole("TMP Scheduler");

    if (foundRole == true) {
        var target = { entityType: "contact", id: formContext.data.entity.getId() };

        req.entity = target;
        req.getMetadata = function () {
            return {
                boundParameter: "entity",
                parameterTypes: {
                    "entity": {
                        typeName: "mscrm.contact",
                        structuralProperty: 5
                    }
                },
                operationType: 0,
                operationName: "tmp_GetPatientFlags"
            };
        };

        Xrm.WebApi.online.execute(req).then(
            function (data) {
                data.json().then(
                    function (response) {
                        var result = JSON.parse(response.outputJSON);
                        if (result != null && result.Data != null) {

                            var message = "";
                            for (var i = 0; i < result.Data.length; i++) {
                                message += '\u2022  ' + result.Data[i].Content + '\n';
                                formContext.ui.setFormNotification(result.Data[i].Content, "WARNING", "msg" + i.toString());
                            }

                            if (message != "") {
                                var alertStrings = { confirmButtonLabel: "Yes", text: message, title: "Flag Notification" };
                                var alertOptions = { height: 120, width: 260 };
                                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                                    function (success) {
                                        console.log("Alert dialog closed");
                                    },
                                    function (error) {
                                        console.log(error.message);
                                    }
                                );
                            }
                        }
                    }
                );

            },
            function (error) {
                debugger;
                var errMsg = error.message;
            }
        );
    }
}

function hasRole(rolename) {
    var hasthisrole = false;
    var roles = Xrm.Utility.getGlobalContext().userSettings.roles;
    roles.forEach(function (item, index, arr) {
        if (item.name == rolename) hasthisrole = true;
    }

    );
    return hasthisrole;
}
