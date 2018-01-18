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
        format: "MM/dd/yyyy",
    });

    // adjust classes to show fiel correctly
    datepicker.closest(".k-datepicker").add(datepicker).removeClass("k-textbox");

    // bind change event to element
    datepicker.change(function (e) {
        var dt = datepicker.data("kendoDatePicker");
        //var current = new Date(e.target.value);
        var current = kendo.parseDate(e.target.value, "dd/MM/yyyy");
        if (current > maxDate) {
            dt.value(maxDate);
            alert('Date must be in fixed range');
        }
        if (current < minDate) {
            dt.value(minDate);
            alert('Date must be in fixed range');
        }
        var nowYear = new Date().getFullYear();
        var selectedYear = current.getFullYear();
        var yearOld = nowYear - selectedYear;

        // now set year group ddl value
        $("#YearId")[0].selectedIndex = 0;
        yearGroups.some(function (d, i) {
            if (yearOld >= d.From && yearOld <= d.To) {
                $("#YearId")[0].selectedIndex = i + 1;
                if (d.From == 4) {
                    $("#FreqId").val(freq46Id);
                }
                return true;
            }
        });
        if (yearOld < 6) yearOld = 6;
        // set class no
        $("#ClassNo").val(yearOld - 5);
    });


    $("#YearId").on('change', function (e) {
        var sel = e.target.selectedIndex;
        if (sel > 0) {
            if (yearGroups[sel - 1].From == 4) {
                $("#FreqId").val(freq46Id);
            }
        }
    });
});