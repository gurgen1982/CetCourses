"use strict"

var currentItem = null;

$(function () {
    $("#listGroup").kendoListView({
        dataSource: {
            transport: {
                read: actionUrl
            },
            requestEnd(result) {
                if (result.response.length == 0) {
                    if (window.location.toString().toLowerCase().indexOf("suggestedgroup") != -1) {
                        window.location = "/child";
                    }
                    else {
                        window.location = "/child/times/0";
                    }
                }
            }
        },
        template: kendo.template($("#groupTemplate").html()),
        selectable: true,
        change: function (e) {
            var lst = $("#listGroup").data("kendoListView");
            currentItem = lst.dataItem(lst.select());
            $("#GroupId").val(currentItem.GroupId);
            $("#btnSubmit").prop('disabled', currentItem == null || currentItem == undefined);
        },
    });

    $("#btnSubmit").prop('disabled', true);
});