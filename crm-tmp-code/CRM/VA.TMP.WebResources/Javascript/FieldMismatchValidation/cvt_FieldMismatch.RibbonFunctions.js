//If the SDK namespace object is not defined, create it.
if (typeof (MCS) == "undefined")
{ MCS = {}; }
// Create Namespace container for functions in this library;
MCS.mcs_FieldMismatch_Buttons = {};

MCS.mcs_FieldMismatch_Buttons.ChooseImportValue = function (selectedItemCount, selectedEntityTypeName, selectedItemIds, grid) {
    MCS.mcs_FieldMismatch_Buttons.UpdateRecord(selectedItemCount, selectedEntityTypeName, selectedItemIds, 100000001, grid);
};

MCS.mcs_FieldMismatch_Buttons.ChooseTmpValue = function (selectedItemCount, selectedEntityTypeName, selectedItemIds, grid) {
    MCS.mcs_FieldMismatch_Buttons.UpdateRecord(selectedItemCount, selectedEntityTypeName, selectedItemIds, 100000000, grid);
};

MCS.mcs_FieldMismatch_Buttons.UpdateRecord = function (selectedItemCount, selectedEntityTypeName, selectedItemIds, userSelection, grid) {
    if (selectedItemCount == null || selectedItemCount === 0) {
        alert('Please select atleast one record in the grid to perform this action');
        return;
    }
    var error;
    for (var i = 0; i < selectedItemCount ; i++) {
        var selectedItemId = selectedItemIds[i].replace("{", "");
        selectedItemId = selectedItemId.replace("}", "");
        var fieldMismatch = {};
        fieldMismatch.cvt_valuechosen = { Value: userSelection }; //this one
        Xrm.WebApi.updateRecord("cvt_fieldmismatch", selectedItemId, fieldMismatch).then(
            function success(result) {
            },
            function (ex) {
                error += ex;
            }
        );

        //CrmRestKit.Update('cvt_fieldmismatch', selectedItemId, fieldMismatch, false)
        //    .fail(function(ex) { error += ex; })
        //    .done(function() {
        //    });
    }
    grid.refresh();
    if (error != null && error !== '') {
        alert(MCS.cvt_Common.RestError('An following error occured while updating the record(s):' + error));
    }
}