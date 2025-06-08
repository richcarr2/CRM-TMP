function checkipadsite(formContext) {

    var currentView = formContext.getViewSelector().getCurrentView().name;

    if (currentView.includes("Phone Book")) {
        var notification = {
            type: 2,
            level: 3,
            //warning
            showCloseButton: true,
            message: "This page contains live links which should only be clicked when providing patient care. DO NOT TEST the links"
        }

        Xrm.App.addGlobalNotification(notification).then(
        function success(result) {
            console.log("Notification created with ID: " + result);

            // Wait for 7 seconds and then clear the notification
            // window.setTimeout(function () {
            // Xrm.App.clearGlobalNotification(result); }, 7000);
        },
        function (error) {
            console.log(error.message);
        });
    }
    return true;
};

function showmessage(formContext) {
    //alert('');
};