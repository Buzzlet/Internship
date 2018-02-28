function sendJsonWithAjax(url, data, containerId) {
    return $.ajax({
        url: url,
        method: "POST",
        data: data,
        success: function (result) {
            if (result.error) {
                toastr.error(result.message, "Error");
                if (containerId !== "" && result.data !== null) {
                    $(containerId).html(result.data);
                }
                return false;
            }
            else {
                if (typeof result.message !== "undefined" && result.message !== "") {
                    toastr.success(result.message, "Success");
                }
                if (containerId !== "" && result.data !== null) {
                    $(containerId).html(result.data);
                }
                return true;
            }
        },
        error: function (request, status, error) {
            toastr.error(error, "Error");
            return false;
        }
    });
}

$.fn.toggleButton = function() {
    this.each(function(index, item) {
        var element = $(item);
        if (element.prop("disabled")) element.enableButton();
        else element.disableButton();
    });
};

$.fn.enableButton = function () {
    this.each(function (index, item) {
        var element = $(item);
        element.prop("disabled", false);

        if (element.data("spinner")) {
            var icon = $("i", element);

            icon.removeClass("fa-spin fa-spinner");
        }
    });
};

$.fn.disableButton = function () {
    this.each(function (index, item) {
        var element = $(item);
        element.prop("disabled", true);

        if (element.data("spinner")) {
            var icon = $("i", element);

            icon.addClass("fa-spin fa-spinner");
        }
    });
};