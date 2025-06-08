?// JScript source code

///// FEEDBACK ENTITY /////

// Show the 'Passed Retest' field if the Feedback Status Reason = 'Retest'

function showPassedRetest(executionContext) {
    var formContext = executionContext.getFormContext();
    var sStatusCode = formContext.getAttribute("statuscode").getSelectedOption().text;

    if (sStatusCode !== null) {

        if (sStatusCode === "Retest") {
            var sPassedRetest = formContext.ui.controls.get("mcs_passedretest");
            sPassedRetest.setVisible(true);
        }
    }
}