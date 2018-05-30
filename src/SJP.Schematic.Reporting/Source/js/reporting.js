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

$(document).ready(function () {
    $('.database_objects').each(function (i, el) {
        var table = $(el).DataTable({
            lengthChange: false,
            paging: true,
            pageLength: 50
        });

        dataTableExportButtons(table);
    });

    var codeElement = document.getElementById("sql-script-codemirror");
    if (codeElement) {
        CodeMirror.fromTextArea(codeElement, {
            lineNumbers: true,
            mode: 'text/x-sql',
            indentWithTabs: true,
            smartIndent: true,
            lineNumbers: true,
            matchBrackets: true,
            autofocus: true,
            readOnly: true
        });
    }

    var viz = new Viz();
    $('script[type="text/vnd.graphviz"]').each(function (i, el) {
        var graphDefinition = el.textContent;
        var containerSelector = $(el).attr("data-graph-id");
        var svgSelector = containerSelector + "-svg";

        var containerElement = document.getElementById(containerSelector);
        viz.renderSVGElement(graphDefinition)
            .then(function(element) {
                element.setAttribute("id", svgSelector);
                containerElement.appendChild(element);
                svgPanZoom(element, {
                    controlIconsEnabled: true,
                    minZoom: 0.1,
                    maxZoom: 10.0
                });
            });
    });

    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        var target = $(e.target).attr("href"); // activated tab
        var svgSelector = target + "-svg";

        var containerId = target.substr(1);
        var graphDefinition = document.querySelector('script[data-graph-id="' + containerId + '"]').textContent;
        
        var containerElement = document.getElementById(containerId);
        while (containerElement.firstChild)
            containerElement.removeChild(containerElement.firstChild);

        viz.renderSVGElement(graphDefinition)
        .then(function(element) {
            element.setAttribute("id", svgSelector);
            containerElement.appendChild(element);
            svgPanZoom(element, {
                controlIconsEnabled: true,
                minZoom: 0.1,
                maxZoom: 10.0
            });
        });
    });
});