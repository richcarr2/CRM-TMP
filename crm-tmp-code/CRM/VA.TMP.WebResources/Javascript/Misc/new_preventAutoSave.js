function preventAutoSave(econtext) {
    //SD - Start
    //web-use-strict-mode
    "use strict";
    //SD - End
    var eventArgs = econtext.getEventArgs();
    if (eventArgs.getSaveMode() === 70 || eventArgs.getSaveMode() === 2) {
        eventArgs.preventDefault();
    }
}  