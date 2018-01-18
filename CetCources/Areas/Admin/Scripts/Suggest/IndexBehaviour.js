"use strict"

$(function () {
    $("#gridGroups").kendoGrid({
        autoBind: true,
        serverSorting: true,
        dataSource: {
            transport: {
                read: getSuggestedGroups,
              //  type: "odata",
            },
            //schema: {
            //    model: {
            //        From: { type: "number" },
            //        Hours: { type: "String" },
            //        ChildCount: { type: "number" },
            //    }
            //},
            pageSize: 1000,
            group: [{ field: "YearGroupName" }, { field: "Day" }],
        },
        groupable: false,
        selectable: "multiple row",
        resizable: true,
        //column: [
        //    //{
        //    //    field: 'YearGroupName',
        //    //    title: 'Year Group Name',
        //    //    width: 0,
        //    //    //attributes: {
        //    //    //    "class": "k-grid-header"
        //    //    //},
        //    //    //groupHeaderTemplate: "aaa",
        //    //    hidden: true
        //    //},
        //    //{
        //    //    field: 'Day',
        //    //    title: 'Day',
        //    //    width: 0,
        //    //    hidden: true,
        //    //},
        //    {
        //        field: 'Hours',
        //    },
        //    {
        //        field: 'ChildCount',
        //    },
        //],
        //dataBinding: function (data) {
        //},
        dataBound: function (o) {
            //var g = $("#gridGroups").data("kendoGrid");
            //for (var i = 0; i < g.columns.length; i++) {
            //    g.showColumn(i);
            //}

            $("#gridGroups").find("tbody > tr:not(.k-grouping-row)").find("td:eq(4)").each(
                function () {
                    var t = $(this); //4
                    var t2 = t.next(); //5
                    var t3 = t2.next(); //6
                    var t4 = t3.next(); //7
                    var t5 = t4.next(); //8

                    t.html("");
                    t2.remove();
                    t3.remove();
                    t4.remove();
                    t5.remove();

                    t.attr("colspan", 5);
                });

            $("#gridGroups tbody > tr.k-grouping-row p").each(function () {
                var t = $(this);
                var content = t.html();
                if (content.indexOf("YearGroupName") != -1) {
                    content = content.replace("YearGroupName:", "");
                    t.html(content);
                }
            });

            $("#gridGroups tbody > tr:not(.k-grouping-row) ").dblclick(function () {
                var kndWindow = $("#kdnWindow").data("kendoWindow");
                kndWindow.open();
                kndWindow.center();
                var grid = $("#gridChildren").data("kendoGrid");
                grid.dataSource.read();
            });
        },
    });

  //  $("div.k-grid-header").hide();

    $("#kdnWindow").kendoWindow({
        animation: {
            open: {
                effects: { fadeIn: {} },
                duration: 200,
                show: true
            },
            close: {
                effects: { fadeOut: {} },
                duration: 200,
                hide: true
            }
        },
        visible: false,
        width: 400,
    });
    $("#gridChildren").kendoGrid({
        dataSource: {
            transport: {
                read: rowChildren,
                parameterMap: function (data, type) {
                    if (type = "read") {
                        var grid = $("#gridGroups").data("kendoGrid");
                        var selectedItem = grid.dataItem(grid.select());
                        var values = {};
                        if (selectedItem != null) {
                            values["hourid"] = selectedItem.HourId;
                            values["day"] = selectedItem.DayOfWeek;
                            values["yearid"] = selectedItem.YearGroupId;
                        }
                        return values;
                    }
                }
                //data: function () {
                //    var grid = $("#gridGroups").data("kendoGrid");
                //    var selectedItem = grid.dataItem(grid.select());
                //    return { 'hourid': selectedItem.HourId };
                //},
            },
            schema: {
                model: {
                    fields: {
                        FullName: { type: "string" },
                        PhoneNumber: { type: "string" },
                    }
                }
            },
            pageSize: 100,
        },
        selectable: "multiple row",
        resizable: true,
        sortable: true,
        column: [
            {
                field: 'FullName',
                title: 'Full Name',
                width: 150,
                attributes: {
                    "class": "k-grid-header"
                },
                template: function (item) { return "asdads";}
            },
            {
                field: 'PhoneNumber',
                title: 'Phone Number',
                width: 100,
            },
        ],
    });
});