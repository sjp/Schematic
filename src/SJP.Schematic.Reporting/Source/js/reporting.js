$(document).ready(function () {
    $('.table-db-objects').each(function (_, el) {
        $(el).DataTable({
            lengthChange: false,
            paging: true,
            pageLength: 50,
            scrollX: true
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

    var svgPzEventsHandler = {
        destroy: function () { this.hammer.destroy(); },
        haltEventListeners: ['touchstart', 'touchend', 'touchmove', 'touchleave', 'touchcancel'],
        init: function (options) {
            var instance = options.instance;
            var initialScale = 1;
            var pannedX = 0;
            var pannedY = 0;

            // Init Hammer
            // Listen only for pointer and touch events
            this.hammer = Hammer(options.svgElement, {
                inputClass: Hammer.SUPPORT_POINTER_EVENTS ? Hammer.PointerEventInput : Hammer.TouchInput
            });
            // Enable pinch
            this.hammer.get('pinch').set({ enable: true });
            // Handle double tap
            this.hammer.on('doubletap', function (__) { instance.zoomIn(); });
            // Handle pan
            this.hammer.on('panstart panmove', function (ev) {
                // On pan start reset panned variables
                if (ev.type === 'panstart') {
                    pannedX = 0;
                    pannedY = 0;
                }
                // Pan only the difference
                instance.panBy({ x: ev.deltaX - pannedX, y: ev.deltaY - pannedY });
                pannedX = ev.deltaX;
                pannedY = ev.deltaY;
            });
            // Handle pinch
            this.hammer.on('pinchstart pinchmove', function (ev) {
                // On pinch start remember initial zoom
                if (ev.type === 'pinchstart') {
                    initialScale = instance.getZoom();
                    instance.zoomAtPoint(initialScale * ev.scale, { x: ev.center.x, y: ev.center.y });
                }
                instance.zoomAtPoint(initialScale * ev.scale, { x: ev.center.x, y: ev.center.y });
            });
            // Prevent moving the page on some devices when panning over SVG
            options.svgElement.addEventListener('touchmove', function (e) { e.preventDefault(); });

            this.hammer = Hammer(options.svgElement);
            this.hammer.on('doubletap', function (__) { options.instance.zoomIn(); });
        }
    };

    var svgPzOptions = {
        controlIconsEnabled: true,
        customEventsHandler: svgPzEventsHandler,
        maxZoom: 10.0,
        minZoom: 0.1,
        zoomEnabled: true
    };

    $('a[data-toggle="pill"]').on('shown.bs.tab', function (e) {
        var target = $(e.target).attr('href'); // activated tab
        var objElementSelector = target + ' > svg';

        svgPanZoom(objElementSelector, svgPzOptions);
    });

    $('.tab-pane.active svg').each(function (_, e) { svgPanZoom(e, svgPzOptions); });
});