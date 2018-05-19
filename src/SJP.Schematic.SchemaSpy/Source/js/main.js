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
            autofocus: true
        });
    }
});