"use strict"
$(function () {

    var datepicker = $("#BirthDate");
    var date = new Date();
    var minDate = new Date();
    var maxDate = new Date();
    minDate.setYear(date.getFullYear() - 18);
    maxDate.setYear(date.getFullYear() - 4);

    // set mask
    datepicker.kendoMaskedTextBox({
        mask: "00/00/0000"
    });

    // set date picker
    datepicker.kendoDatePicker({
        min: minDate,
        max: maxDate,
        format: "dd/MM/yyyy",
    });

    // adjust classes to show fiel correctly
    datepicker.closest(".k-datepicker").add(datepicker).removeClass("k-textbox");

    // bind change event to element
    datepicker.change(function (e) {
        var dt = datepicker.data("kendoDatePicker");
        var eTargetValue = kendo.parseDate(e.target.value, "dd/MM/yyyy");

        if (eTargetValue > maxDate) {
            dt.value(maxDate);
            alert('Date must be in fixed range');
        }
        if (eTargetValue < minDate) {
            dt.value(minDate);
            alert('Date must be in fixed range');
        }
        var nowYear = new Date().getFullYear();
        var nowMonth = new Date().getMonth();
        var nowDay = new Date().getDate();
        var selectedYear = eTargetValue.getFullYear();
        var selectedMonth = eTargetValue.getMonth();
        var selectedDay = eTargetValue.getDate();
        var yearOld = nowYear - selectedYear;

        // now set year group ddl value
        $("#YearId").data("kendoDropDownList").select(0);
        var bfound = false;
        yearGroups.forEach(function (d, i) {
            if (bfound == true) return true;
            if (yearOld >= d.From && yearOld <= d.To) {
                bfound = true;
                $("#YearId").data("kendoDropDownList").select(i + 1);

                if (d.From == 4) {
                    fromYear = 4;
                }
                else {
                    fromYear = null;
                }
                var freq = $("#FreqId").data("kendoDropDownList");
                freq.dataSource.read();
                ///////////

                if (yearOld == d.To && yearGroups.length > i) { // or 6, or 11
                    if ((selectedMonth < nowMonth) || (selectedMonth == nowMonth && selectedDay <= nowDay)) {
                        $("#YearId").data("kendoDropDownList").select(i + 2);
                    }
                }

                ///////////
                return false;
            }
        });
        if (yearOld < 6) yearOld = 6;
        if (yearOld > 17) yearOld = 17;
        // set class no
        $("#ClassNo").val(yearOld - 5);
    });

    var refreshGroupList = function () {
        if (disableFullGroup == 0) return;
        if ($("#YearId").val() != "" && $("#FreqId").val() != "") {
            var group = $("#GroupId").data("kendoDropDownList");
            group.dataSource.read();
        }
    };

    $("#YearId").kendoDropDownList({
        change: function () {
            refreshGroupList();

            var selected = $("#YearId").data("kendoDropDownList").select() - 1;

            if (selected > -1 && yearGroups[selected].From == 4) {
                fromYear = 4;
            }
            else {
                fromYear = null;
            }
            var freq = $("#FreqId").data("kendoDropDownList");
            freq.dataSource.read();
        }
    });
    $("#FreqId").kendoDropDownList({
        change: refreshGroupList,
        autoBind: false,
        dataSource: {
            transport: {
                read: "/Child/GetFreqList",
                parameterMap: function (data, type) {
                    if (type = "read") {
                        return { 'fromYear': fromYear };
                    }
                }
            },
        },
        dataTextField: "FrequencyDescription",
        dataValueField: "FreqId",
        dataBound: function (result) {
            var freq = $("#FreqId").data("kendoDropDownList");
            if (freqId != null) {
                var id = -1;
                freq.dataItems().forEach(function (t, e) {
                    if (id == -1 && t.FreqId == freqId) {
                        id = e;
                    }
                    return true;
                });
                if (id == -1) id = 0;
                freq.select(id);
                freqId = null;
            }
            else {
                freq.select(0);
            }
            // freq.enable(fromYear == null);
        },
    });
    //$("#SchoolId").kendoDropDownList();
    $("#GroupId").kendoDropDownList({
        autoBind: false,
        dataSource: {
            transport: {
                read: "/Child/GetGroupList",
                //   type: "post",
                parameterMap: function (data, type) {
                    if (type = "read") {
                        var values = {};
                        values["YearId"] = $("#YearId").val();
                        values["FreqId"] = $("#FreqId").val();
                        values["ChildId"] = $("#ChildId").val();
                        if ($("#EduLevel").length > 0) {
                            values["eduLevel"] = $("#EduLevel").val();
                        }
                        return values;
                    }
                }
            },
            //requestEnd: function (e, x) {

            //}

        },
        dataTextField: "GroupName",
        dataValueField: "GroupId",
        change: function () {
            var g = $("#GroupId").data("kendoDropDownList")
            if (g.value() != "" && g.value() != "0") {
                $("#btnGoNext").val(saveText);
                $("#btnGoNext").removeClass("btn-default");
                $("#btnGoNext").addClass("btn-success");
            }
            else {
                $("#btnGoNext").val(nextText);
                $("#btnGoNext").addClass("btn-default");
                $("#btnGoNext").removeClass("btn-success");
            }
        },
        dataBound: function (result) {
            var freq = $("#GroupId").data("kendoDropDownList");
            freq.select(0);
        },
    });

    var groups = $("#GroupId").data('kendoDropDownList');
    groups.enable(disableFullGroup);

    $("#btnGoNext").click(function () {
        var childId = $("#ChildId").val();
        if (childId == undefined || childId == null || childId == "") {
            $("#ChildId").val(0);
        }
    });

    $("#useComplete").kendoAutoComplete({
        dataSource: {
            serverFiltering: true,
            transport: {
                read: parentAutoCompletePath,
            }
        },
        animation: {
            close: {
                effects: "fadeOut zoom:out",
                duration: 100
            },
            open: {
                effects: "fadeIn zoom:in",
                duration: 100
            }
        },
        dataTextField: "Name",
        dataValueField: 'Id',
        filter: "contains",
        minLength: 3,
        //placeholder: "Filter by full name",
        select: function (e) {
            $("#UserId").val(e.dataItem.Id);
            // filter by
            //e.dataItem.FullName
            //    $("#filterForm").submit();
        }
        //separator: ", "
    });

    if ($("#EduLevel").length > 0) {
        $("#EduLevel").on('change', refreshGroupList);
    }

    $("form").on('submit', function (e) {
        if (disableFullGroup == 0) {
            groups.enable(1);
        }
        if ($(this).valid()) {
            $("#btnGoNext").prop("disabled", true);
        }
    });
    //$("form").bind('ajax:complete', function () {

    //});
});

//var submitionBegin = function (res) {
//    $("#btnGoNext").prop("disabled", true);
//};
//var submitionComplete = function (res) {
//    $("#btnGoNext").prop("disabled", false);
//};