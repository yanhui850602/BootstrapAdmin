﻿$(function () {
    var bsa = new BootstrapAdmin({
        url: Dicts.url,
        dataEntity: new DataEntity({
            map: {
                Id: "dictID",
                Category: "dictCate",
                Name: "dictName",
                Code: "dictCode",
                Define: "dictDefine"
            }
        })
    });

    $('table').smartTable({
        url: Dicts.url,
        sortName: 'Category',
        queryParams: function (params) { return $.extend(params, { category: $('#txt_dict_cate').val(), name: $("#txt_dict_name").val(), define: $("#txt_dict_define").val() }); },
        columns: [
            { checkbox: true },
            { title: "编辑", field: "Id", events: bsa.idEvents(), formatter: BootstrapAdmin.idFormatter },
            { title: "字典标签", field: "Category", sortable: true },
            { title: "字典名称", field: "Name", sortable: true },
            { title: "字典代码", field: "Code", sortable: true },
            { title: "字典分类", field: "Define", sortable: true, formatter: function (value, row, index) { return value == "0" ? "系统使用" : "自定义"; } }
        ]
    });

    // autocomplete
    $.bc({
        url: "api/Category", swal: false, method: 'get',
        callback: function (result) {
            var data = result.map(function (ele, index) { return ele.Category; });
            $('#txt_dict_cate').typeahead({
                source: data,
                autoSelect: true
            });
        }
    });
});