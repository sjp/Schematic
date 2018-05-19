$(function() {
    var pageUrl = window.location.href.substr(window.location.href.lastIndexOf("/") + 1);
    $("#navbar-collapse ul li a").each(function() {
        if ($(this).attr("href") == pageUrl || $(this).attr("href") == '')
            $(this).parent().addClass("active");
    })
});

$(function() {
	var images = $('img.diagram');

	images.each(function () {
        var id = $(this).attr('id');
        var selector = '#' + id;
        $(selector).draggable();
	});	 
});