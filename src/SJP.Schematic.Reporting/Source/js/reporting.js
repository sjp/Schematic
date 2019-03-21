$(document).ready(function () {
    $('.table-db-objects').each(function (_, el) {
        $(el).DataTable({
            lengthChange: false,
            paging: true,
            pageLength: 50
        });
    });

    var codeElement = document.getElementById('sql-script-codemirror');
    if (codeElement) {
        CodeMirror.fromTextArea(codeElement, {
            lineNumbers: true,
            mode: 'text/x-sql',
            indentWithTabs: true,
            smartIndent: true,
            matchBrackets: true,
            autofocus: true,
            readOnly: true,
            theme: 'material'
        });
    }

    $('a[data-toggle="pill"]').on('shown.bs.tab', function (e) {
        var target = $(e.target).attr('href'); // activated tab
        var objElementSelector = target + ' > svg';
        console.log(objElementSelector);

        svgPanZoom(objElementSelector, {
            controlIconsEnabled: true,
            maxZoom: 10.0,
            minZoom: 0.1,
            zoomEnabled: true
        });
    });

    $('.tab-pane.active svg').each(function (_, e) {
        svgPanZoom(e, {
            controlIconsEnabled: true,
            maxZoom: 10.0,
            minZoom: 0.1,
            zoomEnabled: true
        });
    });
});