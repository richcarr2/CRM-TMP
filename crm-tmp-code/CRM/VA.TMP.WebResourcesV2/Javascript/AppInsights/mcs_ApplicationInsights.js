const devKey = "575cebfe-8828-fc25-9213-00855608c74e";
const nProdKey = "65a6958a-3230-f41a-82be-4ed11913ce2b";
const prodKey = "3b781f72-b051-f088-8d7a-591228844dda";

const devOrgs = "dev2|urs|int|cernerdev|dev3|qa";

var context = parent.Xrm.Utility.getGlobalContext()
var serverUrl = context.getClientUrl();
var orgSplit = serverUrl.split("-");
var org;

//Handle Web Resource to not load when app insights already loaded on parent form
if (window.parent.appInsights === null || window.parent.appInsights === undefined) {
    //debugger;
    if (parent.setEmailRange === undefined)
        parent.setEmailRange = function () { return; }

    var key;
    // this is used to determine what environment is being accessed (app-prod, app-dev, app-test)
    if (orgSplit.length < 3) {
        key = prodKey;
        org = "PROD";
    }
    else {
        orgSplit = orgSplit[2].split(".")[0];
        key = devOrgs.indexOf(orgSplit) > -1 ? devKey : nProdKey;
        org = orgSplit.toUpperCase();
    }

    var appInsights = window.appInsights || function (a) {
        function b(a) { c[a] = function () { var b = arguments; c.queue.push(function () { c[a].apply(c, b) }) } } var c = { config: a }, d = document, e = window; setTimeout(function () { var b = d.createElement("script"); b.src = a.url || "https://az416426.vo.msecnd.net/scripts/a/ai.0.js", d.getElementsByTagName("script")[0].parentNode.appendChild(b) }); try { c.cookie = d.cookie } catch (a) { console.log("app insights cookies failed"); } c.queue = []; for (var f = ["Event", "Exception", "Metric", "PageView", "Trace", "Dependency"]; f.length;) b("track" + f.pop()); if (b("setAuthenticatedUserContext"), b("clearAuthenticatedUserContext"), b("startTrackEvent"), b("stopTrackEvent"), b("startTrackPage"), b("stopTrackPage"), b("flush"), !a.disableExceptionTracking) { f = "onerror", b("_" + f); var g = e[f]; e[f] = function (a, b, d, e, h) { var i = g && g(a, b, d, e, h); return !0 !== i && c["_" + f](a, b, d, e, h), i } } return c
    }({
        instrumentationKey: key,
        endpointUrl: "https://dc.applicationinsights.us/v2/track"
    });

    var UserName;
    var Alias;
    var UserID;
    var ODataPath;
    var formName = "-No Form";

    //var formType = window.Xrm.Page.ui.getFormType();
    var formType = parent.Xrm.Page.ui.getFormType();
    var entityName;

    UserID = context.getUserId().replace("{", "").replace("}", "").toLowerCase();

    ODataPath = serverUrl + "/XRMServices/2011/OrganizationData.svc";
    if (parent.Xrm.Page.ui.formSelector !== null && parent.Xrm.Page.ui.formSelector.getCurrentItem() !== null) {
        formName = parent.Xrm.Page.ui.formSelector.getCurrentItem().getLabel();
    }
    else if (formType === 1 || formType === 5) {
        formName = "Quick Create";
    }

    if (parent.Xrm.Page.data === null) {
        entityName = document.title;
    }
    else {
        entityName = parent.Xrm.Page.data.entity.getEntityName();
    }

    eventName = formName !== null ? entityName + "-" + formName : entityName + "-No Form";

    window.appInsights = appInsights, appInsights.queue
        && 0 === appInsights.queue.length
        && appInsights.trackPageView(eventName, serverUrl,
            {
                User: parent.Xrm.Utility.getGlobalContext().getUserName()
                , AgentId: UserID
                , DomainName: "gcc"
                , Organization: org
                , SessionId: uuidv4()
            });

}
else
    console.log("Not loading app insights as parent page already loaded it");

function uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
