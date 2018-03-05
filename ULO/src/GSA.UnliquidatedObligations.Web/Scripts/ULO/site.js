function debugAlert(msg) {
    //alert("debugAlert: "+msg);
}

function popupConfirmHide() {
    $('#popup-confirm').popup('hide');
};

function toastAppearDisappear(element) {
    if (element.is(':visible')) {
        element.delay(4000).hide(500);
    } else {
        element.fadeIn(500).delay(4000).hide(500);
    }
}

function showToast(itemText) {
    var toast = $('#universal-alert');
    var toastText = $('#universal-alert-item-text');
    toastText.text(itemText);
    toastAppearDisappear(toast);
}

$(document).ready(function () {

    $(".confirm-on-click").each(function () {
        var oldEvents = this.onclick;
        this.onclick = null;
        $(this).click(function (event) {

            var j = $(this);

            if (this.text) {
                $('#popup-confirm-action-button').text(this.text);
            } else {
                $('#popup-confirm-action-button').text("Continue");
            }

            var heading = j.attr("confirmHeading");
            if (heading != null) {
                $('#popup-confirm-heading').text(heading);
            }

            var message = j.attr("confirmMessage");
            $('#popup-confirm-message').text(message==null||message==""?"Are you sure?":message);

            if (!confirm(message)) {
                event.preventDefault();
                event.stopImmediatePropagation();
            }
        });
    });
});

function allSelectorClicked() {
    //alert("allSelectorClicked_v1");
    var all = $("#allSelector")[0];
    all.indeterminate = false;
    $(".itemSelector").each(function () {
        var selector = this;
        //alert(selector + "<=this this.checked=>"+selector.checked);
        selector.checked = all.checked;
    });
    selectionCountChanged();
}

function selectorClicked() {
    //alert("selectorClicked_v5");
    var checkedCount = 0;
    var uncheckedCount = 0;
    $(".itemSelector").each(function () {
        var selector = this;
        //alert(selector + "<=this this.checked=>"+selector.checked);
        if (selector.checked) {
            ++checkedCount;
        }
        else {
            ++uncheckedCount;
        }
    });
    //alert("selectorClicked: checkedCount=" + checkedCount + "; uncheckedCount=" + uncheckedCount);
    var all = $("#allSelector")[0];
    all.indeterminate = false;
    all.checked = false;
    if (checkedCount > 0 && uncheckedCount == 0) {
        all.checked = true;
    }
    else if (checkedCount > 0 && uncheckedCount > 0) {
        all.indeterminate = true;
    }
    selectionCountChanged();
}

function selectionCountChanged() {
    var checkedCount = $(".itemSelector:checked").length;
    //alert("checkedCount=" + checkedCount);
    $(".selectionCount0,.selectionCount1,.selectionCountMultiple").attr("disabled", "disabled");
    var sel;
    if (checkedCount == 0) {
        sel = ".selectionCount0";
    }
    else if (checkedCount == 0) {
        sel = ".selectionCount1";
    }
    else {
        sel = ".selectionCountMultiple";
    }
    $(sel).removeAttr("disabled");
}

function getSelections(attrName) {
    var selections = [];
    $(".itemSelector:checked").each(function () {
        var selector = this;
        var val = $(selector).attr(attrName);
        selections[selections.length] = val;
    });
    return selections;
}

function getCommonReassignees(workflowIds, onSuccess) {
    $.ajax({
        type: "POST",
        url: "/rfr/getCommonReassignees",
        data: JSON.stringify(workflowIds),
        success: function (result) {
            onSuccess(result);
        },
        error: function (xhr, status, p3, p4) {
            var err = "Error " + " " + status + " " + p3 + " " + p4;
            if (xhr.responseText && xhr.responseText[0] == "{")
                err = JSON.parse(xhr.responseText).Message;
            console.log(err);
        }
    });
}
