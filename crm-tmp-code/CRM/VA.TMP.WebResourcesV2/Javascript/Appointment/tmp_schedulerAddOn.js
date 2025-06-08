var oldValue = Xrm.Page.getAttribute("resources").getValue();
checkSFT = function (formContext) {
    if (formContext.getAttribute("cvt_telehealthmodality").getValue() == 1) {
        if (typeof window.top.SFT === 'undefined') {
            window.top.SFT = formContext.getAttribute("cvt_relatedproviderid").getValue();
        }
    }
}

fireResourceOnChange =  formContext => {
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
        if (listofDirtyAttri.includes("cvt_relatedproviderid") && JSON.stringify(window.top.SFT) != JSON.stringify(formContext.getAttribute("cvt_relatedproviderid").getValue())) {
            formContext.getAttribute("cvt_relatedproviderid").fireOnChange();
        }
        else {
            //setTimeout(fireResourceOnChange, 1000, formContext);
            var fireResourceTimeout = setTimeout(function () {
                fireResourceOnChange();
                clearTimeout(fireResourceTimeout);
            }, 1000, formContext);
        }
    }

    if (listofDirtyAttri.includes("resources") && JSON.stringify(oldValue) != JSON.stringify(formContext.getAttribute("resources").getValue())) {
        oldValue = formContext.getAttribute("resources").getValue();
        formContext.getAttribute("resources").fireOnChange();
    } else {
        //setTimeout(fireResourceOnChange, 1000, formContext);
        var fireResourceTimeout = setTimeout(function () {
            fireResourceOnChange();
            clearTimeout(fireResourceTimeout);
        }, 1000, formContext);
    }

}