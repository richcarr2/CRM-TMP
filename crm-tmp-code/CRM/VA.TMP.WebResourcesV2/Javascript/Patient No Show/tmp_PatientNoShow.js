// JavaScript source code
  function PatientNoShowAlert (primaryControl) {
    //alert("Use this panel to mark an appointment as No-Show.");

    var confirmStrings = { text: "Use this panel to mark an appointment as No-Show.", title: "Mark as No-Show." };
    var confirmOptions = { height: 200, width: 450 };
    window.parent.Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
        function (success) {
            if (success.confirmed)
            {
                //var NoShow = formContext.getAttribute
                debugger;
                var formContext = primaryControl;
                formContext.getAttribute("tmp_noshow").setValue(true);
                formContext.getAttribute("statuscode").setValue(917290010);
                formContext.data.save();
            }
                //console.log("Dialog closed using OK button.");
            else
                console.log("Dialog closed using Cancel button or X.");

        });

};
function PatientNoshowButtonVisibility(primaryControl, enable) {
    debugger;
    enable = false;
    var formContext = primaryControl;
    if (formContext.getAttribute('scheduledstart')) {
        var dateFieldValue = formContext.getAttribute('scheduledstart').getValue();
        if (dateFieldValue !== null) {
            let today = new Date();
            var hours = dateFieldValue.getHours();
            var minutes = dateFieldValue.getMinutes();
            //var ampm = hours >= 12 ? 'pm' : 'am';
            hours = hours % 12;
            hours = hours ? hours : 12;
            minutes = minutes < 10 ? '0' + minutes : minutes;
            // var offset = 11;

            let h = today.getHours();
            h = h % 12;
            let m = today.getMinutes();
            if (dateFieldValue < today)
                enable = true;
            if (dateFieldValue == today) {
                if (hours == h) {
                    if (minutes < m)
                        enable = true;
                }
            }

                   
        }
    }
    return enable;  
}