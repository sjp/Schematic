$(document).ready(function() {
    var defaultTheme = {
        "ImageBackgroundColor": "#FFFFFF",
        "EdgeColor": "#000000",
        "TableForegroundColor": "#000000",
        "HeaderForegroundColor": "#000000",
        "FooterForegroundColor": "#000000",
        "PrimaryKeyHeaderForegroundColor": "#000000",
        "UniqueKeyHeaderForegroundColor": "#000000",
        "ForeignKeyHeaderForegroundColor": "#000000",
        "HighlightedTableForegroundColor": "#000000",
        "HighlightedHeaderForegroundColor": "#000000",
        "HighlightedFooterForegroundColor": "#000000",
        "HighlightedPrimaryKeyHeaderForegroundColor": "#000000",
        "HighlightedUniqueKeyHeaderForegroundColor": "#000000",
        "HighlightedForeignKeyHeaderForegroundColor": "#000000",
        "TableBackgroundColor": "#FFFFFF",
        "HeaderBackgroundColor": "#BFE3C6",
        "FooterBackgroundColor": "#BFE3C6",
        "PrimaryKeyHeaderBackgroundColor": "#EFEBA8",
        "UniqueKeyHeaderBackgroundColor": "#B8D0DD",
        "ForeignKeyHeaderBackgroundColor": "#E5E5E5",
        "HighlightedTableBackgroundColor": "#FFFFFF",
        "HighlightedHeaderBackgroundColor": "#7DDE90",
        "HighlightedFooterBackgroundColor": "#7DDE90",
        "HighlightedPrimaryKeyHeaderBackgroundColor": "#D7CD28",
        "HighlightedUniqueKeyHeaderBackgroundColor": "#8FB3C7",
        "HighlightedForeignKeyHeaderBackgroundColor": "#B0B0B0",
        "TableBorderColor": "#000000",
        "HeaderBorderColor": "#000000",
        "FooterBorderColor": "#000000",
        "PrimaryKeyHeaderBorderColor": "#000000",
        "UniqueKeyHeaderBorderColor": "#000000",
        "ForeignKeyHeaderBorderColor": "#000000",
        "HighlightedTableBorderColor": "#000000",
        "HighlightedHeaderBorderColor": "#000000",
        "HighlightedFooterBorderColor": "#000000",
        "HighlightedPrimaryKeyHeaderBorderColor": "#000000",
        "HighlightedUniqueKeyHeaderBorderColor": "#000000",
        "HighlightedForeignKeyHeaderBorderColor": "#000000"
    };

    var currentTheme = {
        "EdgeColor": "#000000",
        "ImageBackgroundColor": "#FFFFFF",
        "TableForegroundColor": "#000000",
        "HeaderForegroundColor": "#000000",
        "FooterForegroundColor": "#000000",
        "PrimaryKeyHeaderForegroundColor": "#000000",
        "UniqueKeyHeaderForegroundColor": "#000000",
        "ForeignKeyHeaderForegroundColor": "#000000",
        "HighlightedTableForegroundColor": "#000000",
        "HighlightedHeaderForegroundColor": "#000000",
        "HighlightedFooterForegroundColor": "#000000",
        "HighlightedPrimaryKeyHeaderForegroundColor": "#000000",
        "HighlightedUniqueKeyHeaderForegroundColor": "#000000",
        "HighlightedForeignKeyHeaderForegroundColor": "#000000",
        "TableBackgroundColor": "#FFFFFF",
        "HeaderBackgroundColor": "#BFE3C6",
        "FooterBackgroundColor": "#BFE3C6",
        "PrimaryKeyHeaderBackgroundColor": "#EFEBA8",
        "UniqueKeyHeaderBackgroundColor": "#B8D0DD",
        "ForeignKeyHeaderBackgroundColor": "#E5E5E5",
        "HighlightedTableBackgroundColor": "#FFFFFF",
        "HighlightedHeaderBackgroundColor": "#7DDE90",
        "HighlightedFooterBackgroundColor": "#7DDE90",
        "HighlightedPrimaryKeyHeaderBackgroundColor": "#D7CD28",
        "HighlightedUniqueKeyHeaderBackgroundColor": "#8FB3C7",
        "HighlightedForeignKeyHeaderBackgroundColor": "#B0B0B0",
        "TableBorderColor": "#000000",
        "HeaderBorderColor": "#000000",
        "FooterBorderColor": "#000000",
        "PrimaryKeyHeaderBorderColor": "#000000",
        "UniqueKeyHeaderBorderColor": "#000000",
        "ForeignKeyHeaderBorderColor": "#000000",
        "HighlightedTableBorderColor": "#000000",
        "HighlightedHeaderBorderColor": "#000000",
        "HighlightedFooterBorderColor": "#000000",
        "HighlightedPrimaryKeyHeaderBorderColor": "#000000",
        "HighlightedUniqueKeyHeaderBorderColor": "#000000",
        "HighlightedForeignKeyHeaderBorderColor": "#000000"
    };

    var themeToSvgClassMapping = {
        "ImageBackgroundColor": ".dot-image-bg-col",
        "EdgeColor": ".dot-edge-col",
        "TableForegroundColor": ".dot-table-fg-col",
        "HeaderForegroundColor": ".dot-header-fg-col",
        "FooterForegroundColor": ".dot-footer-fg-col",
        "PrimaryKeyHeaderForegroundColor": ".dot-primary-key-header-fg-col",
        "UniqueKeyHeaderForegroundColor": ".dot-unique-key-header-fg-col",
        "ForeignKeyHeaderForegroundColor": ".dot-foreign-key-header-fg-col",
        "HighlightedTableForegroundColor": ".dot-hl-table-fg-col",
        "HighlightedHeaderForegroundColor": ".dot-hl-header-fg-col",
        "HighlightedFooterForegroundColor": ".dot-hl-footer-fg-col",
        "HighlightedPrimaryKeyHeaderForegroundColor": ".dot-hl-primary-key-header-fg-col",
        "HighlightedUniqueKeyHeaderForegroundColor": ".dot-hl-unique-key-header-fg-col",
        "HighlightedForeignKeyHeaderForegroundColor": ".dot-hl-foreign-key-header-fg-col",
        "TableBackgroundColor": ".dot-table-bg-col",
        "HeaderBackgroundColor": ".dot-header-bg-col",
        "FooterBackgroundColor": ".dot-footer-bg-col",
        "PrimaryKeyHeaderBackgroundColor": ".dot-primary-key-header-bg-col",
        "UniqueKeyHeaderBackgroundColor": ".dot-unique-key-header-bg-col",
        "ForeignKeyHeaderBackgroundColor": ".dot-foreign-key-header-bg-col",
        "HighlightedTableBackgroundColor": ".dot-hl-table-bg-col",
        "HighlightedHeaderBackgroundColor": ".dot-hl-header-bg-col",
        "HighlightedFooterBackgroundColor": ".dot-hl-footer-bg-col",
        "HighlightedPrimaryKeyHeaderBackgroundColor": ".dot-hl-primary-key-header-bg-col",
        "HighlightedUniqueKeyHeaderBackgroundColor": ".dot-hl-unique-key-header-bg-col",
        "HighlightedForeignKeyHeaderBackgroundColor": ".dot-hl-foreign-key-header-bg-col",
        "TableBorderColor": ".dot-table-border-col",
        "HeaderBorderColor": ".dot-header-border-col",
        "FooterBorderColor": ".dot-footer-border-col",
        "PrimaryKeyHeaderBorderColor": ".dot-primary-key-header-border-col",
        "UniqueKeyHeaderBorderColor": ".dot-unique-key-header-border-col",
        "ForeignKeyHeaderBorderColor": ".dot-foreign-key-header-border-col",
        "HighlightedTableBorderColor": ".dot-hl-table-border-col",
        "HighlightedHeaderBorderColor": ".dot-hl-header-border-col",
        "HighlightedFooterBorderColor": ".dot-hl-footer-border-col",
        "HighlightedPrimaryKeyHeaderBorderColor": ".dot-hl-primary-key-header-border-col",
        "HighlightedUniqueKeyHeaderBorderColor": ".dot-hl-unique-key-header-border-col",
        "HighlightedForeignKeyHeaderBorderColor": ".dot-hl-foreign-key-header-border-col"
    };

    var themeToSvgPropertyMapping = {
        "ImageBackgroundColor": "polygon.fill",
        "EdgeColor": "edge.fill",
        "TableForegroundColor": "text.fill",
        "HeaderForegroundColor": "text.fill",
        "FooterForegroundColor": "text.fill",
        "PrimaryKeyHeaderForegroundColor": "text.fill",
        "UniqueKeyHeaderForegroundColor": "text.fill",
        "ForeignKeyHeaderForegroundColor": "text.fill",
        "HighlightedTableForegroundColor": "text.fill",
        "HighlightedHeaderForegroundColor": "text.fill",
        "HighlightedFooterForegroundColor": "text.fill",
        "HighlightedPrimaryKeyHeaderForegroundColor": "text.fill",
        "HighlightedUniqueKeyHeaderForegroundColor": "text.fill",
        "HighlightedForeignKeyHeaderForegroundColor": "text.fill",
        "TableBackgroundColor": "polygon.fill",
        "HeaderBackgroundColor": "polygon.fill",
        "FooterBackgroundColor": "polygon.fill",
        "PrimaryKeyHeaderBackgroundColor": "polygon.fill",
        "UniqueKeyHeaderBackgroundColor": "polygon.fill",
        "ForeignKeyHeaderBackgroundColor": "polygon.fill",
        "HighlightedTableBackgroundColor": "polygon.fill",
        "HighlightedHeaderBackgroundColor": "polygon.fill",
        "HighlightedFooterBackgroundColor": "polygon.fill",
        "HighlightedPrimaryKeyHeaderBackgroundColor": "polygon.fill",
        "HighlightedUniqueKeyHeaderBackgroundColor": "polygon.fill",
        "HighlightedForeignKeyHeaderBackgroundColor": "polygon.fill",
        "TableBorderColor": "polygon.stroke",
        "HeaderBorderColor": "polygon.stroke",
        "FooterBorderColor": "polygon.stroke",
        "PrimaryKeyHeaderBorderColor": "polygon.stroke",
        "UniqueKeyHeaderBorderColor": "polygon.stroke",
        "ForeignKeyHeaderBorderColor": "polygon.stroke",
        "HighlightedTableBorderColor": "polygon.stroke",
        "HighlightedHeaderBorderColor": "polygon.stroke",
        "HighlightedFooterBorderColor": "polygon.stroke",
        "HighlightedPrimaryKeyHeaderBorderColor": "polygon.stroke",
        "HighlightedUniqueKeyHeaderBorderColor": "polygon.stroke",
        "HighlightedForeignKeyHeaderBorderColor": "polygon.stroke"
    };

    var generateJson = function() {
        var result = "{";

        var hasFirstLine = false;
        for (var key in defaultTheme) {
            if (hasFirstLine)
                result += ",";
            hasFirstLine = true;

            var val = currentTheme[key];
            if (!val)
                val = defaultTheme[key];

            result += "\n    \"" + key + "\": \"" + val + "\"";
        }

        return result + "\n}";
    };

    var renderJson = function() {
        var jsonText = generateJson();
        $("#generated-output").text(jsonText);
    };

    var updateGraph = function() {
        for (var key in defaultTheme) {
            var val = currentTheme[key];
            if (!val)
                val = defaultTheme[key];

            var propertyMap = themeToSvgPropertyMapping[key];
            if (propertyMap === "polygon.fill") {
                var polygonSelectorFill = "svg polygon" + themeToSvgClassMapping[key];
                $(polygonSelectorFill).attr("fill", val);
            }
            if (propertyMap === "polygon.stroke") {
                var polygonSelectorStroke = "svg polygon" + themeToSvgClassMapping[key];
                $(polygonSelectorStroke).attr("stroke", val);
            }
            if (propertyMap === "text.fill") {
                var textSelectorFill = "svg text" + themeToSvgClassMapping[key];
                $(textSelectorFill).attr("fill", val);
            }
            if (propertyMap === "edge.fill") {
                var pathSelectorStroke = "svg path" + themeToSvgClassMapping[key];
                $(pathSelectorStroke).attr("stroke", val);
                var edgePolygonSelector = "svg polygon" + themeToSvgClassMapping[key];
                $(edgePolygonSelector).attr("fill", val);
                $(edgePolygonSelector).attr("stroke", val);
            }
        }
    };

    var updateProperty = function(propName, val) {
        currentTheme[propName] = val;
        updateGraph();
        renderJson();
    };

    $("#image-bg-col").on("input", function () { updateProperty("ImageBackgroundColor", $(this).val()); });
    $("#edge-col").on("input", function () { updateProperty("EdgeColor", $(this).val()); });

    $("#table-fg-col").on("input", function() { updateProperty("TableForegroundColor", $(this).val()); });
    $("#header-fg-col").on("input", function() { updateProperty("HeaderForegroundColor", $(this).val()); });
    $("#footer-fg-col").on("input", function() { updateProperty("FooterForegroundColor", $(this).val()); });
    $("#primary-key-header-fg-col").on("input", function() { updateProperty("PrimaryKeyHeaderForegroundColor", $(this).val()); });
    $("#unique-key-header-fg-col").on("input", function() { updateProperty("UniqueKeyHeaderForegroundColor", $(this).val()); });
    $("#foreign-key-header-fg-col").on("input", function() { updateProperty("ForeignKeyHeaderForegroundColor", $(this).val()); });
    $("#hl-table-fg-col").on("input", function() { updateProperty("HighlightedTableForegroundColor", $(this).val()); });
    $("#hl-header-fg-col").on("input", function() { updateProperty("HighlightedHeaderForegroundColor", $(this).val()); });
    $("#hl-footer-fg-col").on("input", function() { updateProperty("HighlightedFooterForegroundColor", $(this).val()); });
    $("#hl-primary-key-header-fg-col").on("input", function() { updateProperty("HighlightedPrimaryKeyHeaderForegroundColor", $(this).val()); });
    $("#hl-unique-key-header-fg-col").on("input", function() { updateProperty("HighlightedUniqueKeyHeaderForegroundColor", $(this).val()); });
    $("#hl-foreign-key-header-fg-col").on("input", function() { updateProperty("HighlightedForeignKeyHeaderForegroundColor", $(this).val()); });

    $("#table-bg-col").on("input", function() { updateProperty("TableBackgroundColor", $(this).val()); });
    $("#header-bg-col").on("input", function() { updateProperty("HeaderBackgroundColor", $(this).val()); });
    $("#footer-bg-col").on("input", function() { updateProperty("FooterBackgroundColor", $(this).val()); });
    $("#primary-key-header-bg-col").on("input", function() { updateProperty("PrimaryKeyHeaderBackgroundColor", $(this).val()); });
    $("#unique-key-header-bg-col").on("input", function() { updateProperty("UniqueKeyHeaderBackgroundColor", $(this).val()); });
    $("#foreign-key-header-bg-col").on("input", function() { updateProperty("ForeignKeyHeaderBackgroundColor", $(this).val()); });
    $("#hl-table-bg-col").on("input", function() { updateProperty("HighlightedTableBackgroundColor", $(this).val()); });
    $("#hl-header-bg-col").on("input", function() { updateProperty("HighlightedHeaderBackgroundColor", $(this).val()); });
    $("#hl-footer-bg-col").on("input", function() { updateProperty("HighlightedFooterBackgroundColor", $(this).val()); });
    $("#hl-primary-key-header-bg-col").on("input", function() { updateProperty("HighlightedPrimaryKeyHeaderBackgroundColor", $(this).val()); });
    $("#hl-unique-key-header-bg-col").on("input", function() { updateProperty("HighlightedUniqueKeyHeaderBackgroundColor", $(this).val()); });
    $("#hl-foreign-key-header-bg-col").on("input", function() { updateProperty("HighlightedForeignKeyHeaderBackgroundColor", $(this).val()); });

    $("#table-border-col").on("input", function() { updateProperty("TableBorderColor", $(this).val()); });
    $("#header-border-col").on("input", function() { updateProperty("HeaderBorderColor", $(this).val()); });
    $("#footer-border-col").on("input", function() { updateProperty("FooterBorderColor", $(this).val()); });
    $("#primary-key-header-border-col").on("input", function() { updateProperty("PrimaryKeyHeaderBorderColor", $(this).val()); });
    $("#unique-key-header-border-col").on("input", function() { updateProperty("UniqueKeyHeaderBorderColor", $(this).val()); });
    $("#foreign-key-header-border-col").on("input", function() { updateProperty("ForeignKeyHeaderBorderColor", $(this).val()); });
    $("#hl-table-border-col").on("input", function() { updateProperty("HighlightedTableBorderColor", $(this).val()); });
    $("#hl-header-border-col").on("input", function() { updateProperty("HighlightedHeaderBorderColor", $(this).val()); });
    $("#hl-footer-border-col").on("input", function() { updateProperty("HighlightedFooterBorderColor", $(this).val()); });
    $("#hl-primary-key-header-border-col").on("input", function() { updateProperty("HighlightedPrimaryKeyHeaderBorderColor", $(this).val()); });
    $("#hl-unique-key-header-border-col").on("input", function() { updateProperty("HighlightedUniqueKeyHeaderBorderColor", $(this).val()); });
    $("#hl-foreign-key-header-border-col").on("input", function() { updateProperty("HighlightedForeignKeyHeaderBorderColor", $(this).val()); });
});