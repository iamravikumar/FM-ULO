/*! site.js */
function debugAlert(msg) {
    console.debug(msg);
    //    alert("debugAlert: "+msg);
}

function debugLambda(name, f) {
    var header = "";
    for (var i = 0; i < arguments.length; i++) {
        var a = arguments[i];
        var json = JSON.stringify(a);
        header += "\n\t" + i+": ["+json+"]";
    } 
    header = name + (header === "" ? " " : "\n") + "vvvvvvvvvvvvvvvvvvvvvvv";
    debugAlert(header);
    try {
        var ret = f();
        debugAlert(name + " ^^^^^^^^^^^^^^^^^^^^^^^");
        return ret;
    }
    catch (e) {
        debugAlert(name + "^^^^^^^^^^^^^^^^^^^^^^^\nERROR\n" + e);
        throw e;
    }
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

function appendStalenessData(obj) {
    obj = obj || {};
    obj.StalenessWorkflowRowVersionString = $("input[name='StalenessWorkflowRowVersionString']").val();
    obj.StalenessEditingBeganAtUtc = $("input[name='StalenessEditingBeganAtUtc']").val();
    obj.StalenessWorkflowId = $("input[name='StalenessWorkflowId']").val();
    //alert(JSON.stringify(obj));
    return obj;
}

function itemCountString(itemCount, itemTypeString) {
    if (itemCount == 1) {
        return "1 " + itemTypeString;
    }
    return itemCount + " " + itemTypeString + "s";
}

function setupFancyMultiselect(el)
{
    $(el).find("select").not(".no-select-all").not(".standard").multiselect({
        maxHeight: 320,
        buttonWidth: '175px',
        numberDisplayed: 1,
        includeSelectAllOption: true
    });

    $(el).find("select.no-select-all").not(".standard").multiselect({
        maxHeight: 320,
        buttonWidth: '175px',
        numberDisplayed: 1
    });
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
            $('#popup-confirm-message').text(message == null || message == "" ? "Are you sure?" : message);

            if (!confirm(message)) {
                event.preventDefault();
                event.stopImmediatePropagation();
            }
        });
    });

    $(".advanced-search-settings,.form-group").each(function (n, el) {
        setupFancyMultiselect(el);
    });

    $("form").submit(function (a) {
        var formEl = a.target;
        var isPost = formEl.method == "post";
        $(formEl).find("select").each(function (n, el) {
            if (el.multiple) {
                var enabledCnt = 0;
                var selectedCnt = 0;
                var jel = $(el);
                jel.find("option").each(function (fdsfdsafsdafs, opt) {
                    enabledCnt += opt.disabled ? 0 : 1;
                    selectedCnt += opt.selected ? 1 : 0;
                });
                if (enabledCnt == selectedCnt && selectedCnt > 15 && !isPost) {
                    jel.find("option").each(function (fdsfdsafsdafs, opt) {
                        if (!opt.disabled) {
                            opt.selected = false;
                        }
                    });
                }
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
    $(".selectionCount0,.selectionCount1,.selectionCountPositive,.selectionCountMultiple").attr("disabled", "disabled");
    var sel;
    if (checkedCount == 0) {
        sel = ".selectionCount0";
    }
    else if (checkedCount == 1) {
        sel = ".selectionCount1,.selectionCountPositive";
    }
    else {
        sel = ".selectionCountMultiple,.selectionCountPositive";
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

function markViewed(ids, view) {
    for (var z = 0; z < ids.length; ++z) {
        var id = parseInt(ids[z]);
        ids[z] = id;
        var jel = $("tr[data-id='" + id + "']");
        if (view) {
            jel.addClass("viewed");
        }
        else {
            jel.removeClass("viewed");
        }
    }
    markAsViewedAjax(ids, view);
    return false;
}

function markAsViewedAjax(workflowIds, viewed, onSuccess) {
    $.ajax({
        type: "POST",
        url: "/ulo/mark",
        data: JSON.stringify({ workflowIds: workflowIds, viewed: viewed }),
        success: function (result) {
            if (onSuccess != null) {
                onSuccess(result);
            }
        },
        error: standardAjaxErrorHandler
    });
}

function getCommonReassignees(workflowIds, onSuccess) {
    $.ajax({
        type: "POST",
        url: "/rfr/getCommonReassignees",
        data: JSON.stringify(workflowIds),
        success: function (result) {
            if (onSuccess != null) {
                onSuccess(result);
            }
        },
        error: standardAjaxErrorHandler
    });
}

function getNotes(uloId, onSuccess) {
    $.ajax({
        type: "GET",
        url: "/ulos/" + uloId + "/notes",
        success: function (result) {
            if (onSuccess != null) {
                onSuccess(result);
            }
        },
        error: standardAjaxErrorHandler
    });
}

function standardAjaxErrorHandler(xhr, status, p3, p4) {
    var err = "Error " + " " + status + " " + p3 + " " + p4;
    if (xhr.responseText && xhr.responseText[0] == "{")
        err = JSON.parse(xhr.responseText).Message;
    console.log(err);
}
