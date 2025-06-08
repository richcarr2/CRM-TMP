var oldValue = Xrm.Page.getAttribute("resources").getValue();
var triggered = false;
checkSFT = function () {
    if (Xrm.Page.getAttribute("cvt_telehealthmodality").getValue() == 1) {
        if (typeof window.top.SFT === 'undefined') {
            window.top.SFT = Xrm.Page.getAttribute("cvt_relatedproviderid").getValue();

        }
    }
}

fireResourceOnChange = formContext => {
    checkSFT();
    formContext = Xrm.Page;
    var allAttributes = formContext.data.entity.attributes.get();
    var listofDirtyAttri = [];
    if (allAttributes != null) {
        for (var i in allAttributes) {
            if (allAttributes[i].getIsDirty()) {
                listofDirtyAttri.push(allAttributes[i].getName());
            }
        }
    }

    if (typeof window.top.SFT !== 'undefined') {
        if (listofDirtyAttri.includes("cvt_relatedproviderid") && JSON.stringify(window.top.SFT) != JSON.stringify(Xrm.Page.getAttribute("cvt_relatedproviderid").getValue())) {
            formContext.getAttribute("cvt_relatedproviderid").fireOnChange();
        }
        else {
            if (triggered === true) {
                setTimeout(fireResourceOnChange, 1000);
            }
        }
    }

    if (listofDirtyAttri.includes("resources") && JSON.stringify(oldValue) != JSON.stringify(Xrm.Page.getAttribute("resources").getValue())) {
        oldValue = Xrm.Page.getAttribute("resources").getValue();
        formContext.getAttribute("resources").fireOnChange();
        triggered = true;
        setTimeout(fireResourceOnChange, 1000);
    } else {
        if (triggered === false) {
            setTimeout(fireResourceOnChange, 1000);
        }
    }


}