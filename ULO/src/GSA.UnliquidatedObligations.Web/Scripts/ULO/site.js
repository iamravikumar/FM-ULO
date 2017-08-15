function popupConfirmHide() {
    $('#popup-confirm').popup('hide');
};

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