"use strict"

var DaysToEnable = 0;
var DaysSelected = 0;

var $Days = $("#Sunday, #Monday, #Tuesday, #Wednesday, #Thursday, #Friday, #Saturday");

$(function () {
    if ($("#FreqId").val() != "") {
        DaysToEnable = freqDayCount[$("#FreqId")[0].selectedIndex - 1];
    }
    if (isEditMode === true) {
        DaysSelected = DaysToEnable;
    }

    $("#YearId").change(function (e) {
        ddl.resetValues();
        if (yearIdFor46Group == e.target.value) {
            $("#FreqId").val(freqIdFor46Group);
            DaysToEnable = 1;
        }
        else {
            $("#FreqId").val("");
            DaysToEnable = 0;
        }
        grid.refresh();
    });

    $("#FreqId").change(function (e) {
        DaysToEnable = e.target.selectedIndex == 0 ? 0 : freqDayCount[e.target.selectedIndex - 1];
        ddl.resetValues();
        grid.refresh();
    });

    $Days.each(function (i, e) {
        $(e).change(ddl.dayChanged);
    });
    $("#EduLevel").on('change', function () {
        grid.refresh();
    });
    
    //if (isEditMode === true) {
    ddl.disableOther();
    //}

    $("input:submit").on('click', function (e) {

        if (DaysToEnable != DaysSelected) {
            e.preventDefault();
            e.stopPropagation();
            alert('Please select days and times');
        }

        if ($("#Inactive").is(":checked") &&
            confirm("You are about to disable the group. Children will be removed from the group") == false) {
            e.preventDefault();
            e.stopPropagation();
        }
    });

    $("#btnCompleted").on('click', function (e) {
        if ($("#Inactive").is(":checked")) {
            alert("Cannot upgrade inactive group");
        }
        if (confirm("Are you sure you want to upgrade the group up to the next level?") == true) {
            $.post("/admin/groups/completed", { groupId: $("#GroupId").val() })
                .success(function () {
                    alert("Group's level and all children's level inside the group have been upgrated");
                    window.location = "/admin/groups";
                })
                .error(function (e, r, x) {
                    alert("error accured during updating records");
                });
        }
    });

    grid.init();
});

var ddl = {
    resetValues: function () {
        var enabledDays = [];
        var yearId = $("#YearId")[0].selectedIndex - 1;
        if (yearId > -1) {
            enabledDays = yearGroups[yearId];
        }
        //

        $Days.each(function (i, e) {
            $(e).val("");
            $(e).prop("disabled", "");
            if (enabledDays.indexOf(i) == -1) {
                $(e).prop("disabled", "disabled");
                $(e).val(0);
            }
        });
        DaysSelected = 0;
    },

    disableOther: function () {
        $Days.each(function (i, e) {
            if ($(e).val() == "") {
                $(e).prop("disabled", "disabled");
                $(e).val(0);
            }
        });
    },

    dayChanged: function () {
        grid.refresh();
        DaysSelected = 0;
        $Days.each(function (i, e) {
            if ($(e).val() != null && $(e).val() != "")
                DaysSelected++;
        });

        if (DaysSelected == DaysToEnable) {
            ddl.disableOther();
        }
        else if (DaysSelected < DaysToEnable) {
            var enabledDays = [];
            var yearId = $("#YearId")[0].selectedIndex - 1;
            if (yearId > -1) {
                enabledDays = yearGroups[yearId];
            }
            $Days.each(function (i, e) {
                if ($(e).val() == null) $(e).val("");
                $(e).prop("disabled", "");
                if (enabledDays.indexOf(i) == -1) {
                    $(e).prop("disabled", "disabled");
                    $(e).val(0);
                }
            });
        }
    },
};

var grid = {

    kendo: null,

    refresh: function () {
     //   if ($("#YearId").val() != "") { //$("#FreqId").val() != "" &&
            grid.kendo.dataSource.read();
        //}
        //else {
        //    grid.kendo.dataSource.data([]);
        //}
    },
    init: function () {
        $("#gridChildren").kendoGrid({
            autoBind: false,
            dataSource: {
                transport: {
                    read: "/Admin/Groups/GetMatchedChildrenList",
                    type: "post",
                    parameterMap: function (data, type) {
                        if (type = "read") {
                            var values = {};
                            values = $("form").serializeArray();
                            //values["YearId"] = $("#YearId").val();
                            //values["FreqId"] = $("#FreqId").val();
                            //values["Sunday"] = $("#FreqId").val();
                            return values;
                        }
                    }
                },
            },
            resizable: true,
            selectable: true,
            columns: [
                { field: "HourDesc", title: "Hour" },
                { field: "Sunday", template: function (item) { return grid.cellTemplate(item, "Sunday") }, },
                { field: "Monday", template: function (item) { return grid.cellTemplate(item, "Monday") }, },
                { field: "Tuesday", template: function (item) { return grid.cellTemplate(item, "Tuesday") }, },
                { field: "Wednesday", template: function (item) { return grid.cellTemplate(item, "Wednesday") }, },
                { field: "Thursday", template: function (item) { return grid.cellTemplate(item, "Thursday") }, },
                { field: "Friday", template: function (item) { return grid.cellTemplate(item, "Friday") }, },
                { field: "Saturday", template: function (item) { return grid.cellTemplate(item, "Saturday") }, },
            ],
            dataBound: function () {
                $(".cell-selected").parent().addClass("green-background");
            },
        });

        grid.kendo = $("#gridChildren").data("kendoGrid");

        grid.refresh();

    },

    cellTemplate: function (item, field) {
        var val = item[field];
        if (val == null || val == undefined) val = "";
        if (item.HourDesc == $("#" + field + " option:selected").text()) {
            return "<span class='cell-selected'>" + val + "</span>";
        }
        return val;
    },
};