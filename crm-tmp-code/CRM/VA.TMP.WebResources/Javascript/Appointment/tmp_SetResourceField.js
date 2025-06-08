window.top.setResourceField = function (id, name, type) {
    debugger;
    if (Xrm.Page.getAttribute("resources").getValue() == null) {
        var partlistData = new Array();

        partlistData[0] = new Object();

        partlistData[0].id = id;

        partlistData[0].name = name;

        partlistData[0].entityType = type;


        Xrm.Page.getAttribute("resources").setValue(partlistData);

    } else {

        var partlistData = new Array();

        partlistData[0] = new Object();

        partlistData[0].id = id;

        partlistData[0].name = name;

        partlistData[0].entityType = type;

        var existingData = Xrm.Page.getAttribute("resources").getValue();
        var dataCheck = new HashTable();
        for (var i = 0; i < existingData.length; i++) {
            dataCheck.setItem(existingData[i].id.toLowerCase(), existingData[i]);
        }
        if (!dataCheck.hasItem("{" + id.toLowerCase() + "}")) {
            existingData.push(partlistData[0]);
            Xrm.Page.getAttribute("resources").setValue(existingData);
        } else {
            dataCheck.removeItem("{" + id.toLowerCase() + "}");
            for (var i = 0; i < dataCheck.items.length; i++) {
                existingData = new Array();
                existingData.push(dataCheck.items[i]);
                Xrm.Page.getAttribute("resources").setValue(existingData)

            }
        }


    }

}

function HashTable() {
    this.length = 0;
    this.items = new Array();
    for (var i = 0; i < arguments.length; i += 2) {
        if (typeof (arguments[i + 1]) != 'undefined') {
            this.items[arguments[i]] = arguments[i + 1];
            this.length++;
        }
    }

    this.removeItem = function (in_key) {
        var tmp_value;
        if (typeof (this.items[in_key]) != 'undefined') {
            this.length--;
            var tmp_value = this.items[in_key];
            delete this.items[in_key];
        }

        return tmp_value;
    }

    this.getItem = function (in_key) {
        return this.items[in_key];
    }

    this.setItem = function (in_key, in_value) {
        if (typeof (in_value) != 'undefined') {
            if (typeof (this.items[in_key]) == 'undefined') {
                this.length++;
            }

            this.items[in_key] = in_value;
        }

        return in_value;
    }

    this.hasItem = function (in_key) {
        return typeof (this.items[in_key]) != 'undefined';
    }
}