$(function() {
    var pageUrl = window.location.href.substr(window.location.href.lastIndexOf("/") + 1);
    $("#navbar-collapse ul li a").each(function() {
        if ($(this).attr("href") == pageUrl || $(this).attr("href") == '')
            $(this).parent().addClass("active");
    })
});

function dataTableExportButtons(table) {
    var tableContainerId = $(table.table().container()).attr('id');
    
    new $.fn.dataTable.Buttons(table, {
        buttons: [
            {
                extend:    'copyHtml5',
                text:      '<i class="fa fa-files-o"></i> Copy',
                titleAttr: 'Copy'
            },
            {
                extend:    'csvHtml5',
                text:      '<i class="fa fa-file-text-o"></i> CSV',
                titleAttr: 'CSV'
            }
        ]
    });
    
    table.buttons().container()
        .appendTo('#' + tableContainerId + ' .col-sm-6:first-of-type');
}

