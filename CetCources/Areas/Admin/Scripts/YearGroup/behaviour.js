"use strict"

$(function () {
     $("input:submit").on('click', function (e) {
         var anyChecked = $("input[id^=chk_]:checked");
        if (anyChecked.length==0) {
            e.preventDefault();
            e.stopPropagation();
            alert("Select at least one day of week");
        }
        if ($("#From").val() == 4 && anyChecked.length > 1) {
            e.preventDefault();
            e.stopPropagation();
            alert("Select only one day for " + $("#From").val() + "-" + $("#To").val() + " years old kids");
        }
    });
});

jQuery.validator.addMethod('isgreaterthan',
     function (value, element, params) {
         var from = parseInt($("#" + element.attributes['data-val-isgreaterthan-comparewith'].nodeValue).val());
         if (!/Invalid|NaN/.test(value) && !/Invalid|NaN/.test(from)) {
             return parseInt(value) > from;
         }
         return isNaN(value) && isNaN(from) || (parseInt(value) > from);
     }, '');

jQuery.validator.unobtrusive.adapters.add('isgreaterthan',
    {},
    function (options) {
        options.rules['isgreaterthan'] = options.element.id;
        options.messages['isgreaterthan'] = options.message;
    });