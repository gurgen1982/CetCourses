$(document).ready(function () {
    $("#filterFullName").kendoAutoComplete({
        dataSource: {
            serverFiltering: true,
            transport: {
                read: fullNameUrl,
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
        dataTextField: "FullName",
        filter: "contains",
        minLength: 3,
        placeholder: "Filter by full name",
        //select: function (e) {
        //    // filter by
        //    //e.dataItem.FullName
        ////    $("#filterForm").submit();
        //}
        //separator: ", "
    });

    $("#filterById").kendoAutoComplete({
        dataSource: {
            serverFiltering: true,
            transport: {
                read: idUrl,
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
        dataTextField: "ID",
        filter: "contains",
        minLength: 3,
        placeholder: "Filter by child Id",
    });

    $("#filterById, #filterFullName, #Groups, #YearGroup, #InactiveList").change(function () {
        $("#page").val(1);
        $("#filterForm").submit();
    });
});