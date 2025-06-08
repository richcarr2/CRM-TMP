function typeChanged(executionObject) {
    //comment- added use strict
    "use strict";
    var formContext = executionObject.getFormContext();
    if (formContext.getAttribute("cvt_type").getValue()) formContext.getControl("cvt_patientsiteresourcesrequired").setVisible(true);
    else {
        formContext.getControl("cvt_patientsiteresourcesrequired").setVisible(false);
    }
}

function patientsiteresourcesrequiredChanged(executionObject) {
    var formContext = executionObject.getFormContext();
    if (formContext.getAttribute("cvt_patientsiteresourcesrequired").getValue()) {
        formContext.getControl("mcs_relatedsite").setVisible(true);
        formContext.getAttribute("mcs_relatedsite").setRequiredLevel('required');
        formContext.getControl("mcs_relatedprovidersite").setVisible(false);
        formContext.getAttribute("mcs_relatedprovidersite").setRequiredLevel('none');

    }
    else {
        formContext.getControl("mcs_relatedsite").setVisible(false);
        formContext.getAttribute("mcs_relatedsite").setRequiredLevel('none');
        formContext.getControl("mcs_relatedprovidersite").setVisible(true);
        formContext.getAttribute("mcs_relatedprovidersite").setRequiredLevel('required');

    }

}

function schedulingPackageSelected(executionObject) {

    var formContext = executionObject.getFormContext();

    var selectedPackage = formContext.getAttribute("cvt_relatedschedulingpackage").getValue()[0].id;
    //Comments- Alert was placed for debug purpose- commented
    // alert(selectedPackage);
    var serverUrl = Xrm.Utility.getGlobalContext().getClientUrl();
    var url = serverUrl + "/api/data/v9.0/cvt_resourcepackages(" + selectedPackage + ")?$select=cvt_patientsites,cvt_name,cvt_resourcepackageid";
    //Venkat Comment- changed to XML Http Reqeust object
    //var service = GetRequestObject();
    var service = new XMLHttpRequest();

    if (service != null) {
        service.open("GET", url, false);
        service.setRequestHeader("X-Requested-Width", "XMLHttpRequest");
        service.setRequestHeader("Accept", "application/json, text/javascript, */*");
        service.send(null);

        var results = JSON.parse(service.responseText);
    }
}
//Comment- not been used anywhere- commented
/*  function GetRequestObject(){
       if(window.XMLHttpRequest){
          return new window.XMLHttpRequest;
       }
       else{
          try{
             return new ActiveXObject("MSXML2.XMLHTTP.3.0");
          }
          catch(ex){
              return null;
           }
       } 
    
    }*/
