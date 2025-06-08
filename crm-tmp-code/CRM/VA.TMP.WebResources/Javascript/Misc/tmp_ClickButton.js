function ClickCustomButton(primaryControl) {
    var formContext = primaryControl;
    setStatusStatusReason(formContext, “account”, formContext.data.entity.getId(), 1, 614020000); // these need to be changed as per your context
}
function setStatusStatusReason(executionContext, entityLogicalName, recordId, statecode, statuscode) {
    var id = recordId.replace(“{ “, “”).replace(“} ”, “”);
var data = { “statecode”: statecode, “statuscode”: statuscode };
Xrm.WebApi.updateRecord(entityLogicalName, id, data).then(
    function success(result) { executionContext.data.refresh(true); },
    function (error) { executionContext.data.refresh(true); }
);
}
