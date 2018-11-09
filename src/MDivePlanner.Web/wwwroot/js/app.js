// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

var app = {
    init: function () {
        var context = this;

        context.overwatchLevels();

        $("#SubmitDiveParams").click((e) => {
            e.preventDefault();

            $.ajax({
                url: window.location.origin + "/App/SetParams",
                type: "post",
                data: $("#SubmitDiveParamsForm").serialize(),
                success: function (result) {
                    $("#DiveParamsContainer").html(result);

                    context.overwatchLevels();

                    var markInvalid = elem => {
                        if ($(elem).val().toLowerCase() == "false") {
                            var checkbox = $(elem).parent().find("input[type=checkbox]");
                            if (checkbox.is(':checked'))
                                checkbox.addClass("input-validation-error");
                        }
                    };

                    $(".levels .levelsTable input.levelValid").each((i, elem) => { markInvalid(elem); });
                    $(".decoLevels .levelsTable input.levelValid").each((i, elem) => { markInvalid(elem); });

                    var valid = $("#DiveParamsValid", result).val() == "true";
                    if (valid)
                        context.onGotResult();
                }
            });

            return false;
        });
    },

    onGotResult: function () {
        var canvas = $("#OutputCanvas")[0];

        try {
            $("table.result-table tbody").html("");
            $("#divePlanResultErrorsOutput").text($("#divePlanResultErrors").val());
            var diveResVal = $("#divePlanResult").val();
            var diveResult = JSON.parse(diveResVal);

            var diveInfo = "Max Depth: " + diveResult.maxDepth.toFixed(1) + "m - " + Math.ceil(diveResult.bottomTime) +
                "/" + Math.ceil(diveResult.totalTime) + " (bottom/total) mins";
            var hasDeco = false;

            for (var i = 0; i < diveResult.planPoints.length; i++) {
                if (DivePlanPointType.HasPoint(diveResult.planPoints[i], DivePlanPointType.deco)) {
                    hasDeco = true;
                    break;
                }
            }

            var table1 = "<tr><td>" + diveInfo +
                "</td><td>" + (hasDeco ? "With Deco" : "Without deco") +
                "</td><td>" + diveResult.maxPpO.toFixed(2) + " ata" +
                "</td><td>" + this.getConsumedGases(diveResult) + "</td></tr>";
                
            var table2 = "<tr><td>" + diveResult.oxygenCns.toFixed(1) + "%" +
                "</td><td>" + diveResult.maxEND.toFixed(1) + " m" +
                "</td><td>" + Math.round(diveResult.maxNoDecoDepthTime.time) + " mins" +
                "</td><td>" + Math.round(diveResult.totalTime - diveResult.bottomTime) + " mins" +
                "</td><td>" + Math.round(diveResult.fullDesaturationTime / 60) + " hours</td></tr>";

            $("table.result-table.table1 tbody").html(table1);
            $("table.result-table.table2 tbody").html(table2);

            graph.init(canvas, diveResult, false);
        }
        catch (x) {
            $("#divePlanResultErrorsOutput").text(x);
        }
        //var diveResult = new DiveResult();
        //diveResult.depth = direParams.depth;
        //diveResult.totalTime = direParams.diveTime;
    },

    getConsumedGases: function (diveResult) {
        var text = "";

        for (var i = 0; i < diveResult.consumedBottomGases.length; i++) {
            var gas = diveResult.consumedBottomGases[i];
            text += gas.gas.name + "  " + gas.amount + " ltrs";
            if (i < (diveResult.consumedBottomGases.length - 1))
                text += ", ";
        }

        if (diveResult.consumedDecoGases != null && diveResult.consumedDecoGases.length > 0) {
            text += ", ";
            for (var i = 0; i < diveResult.consumedDecoGases.length; i++) {
                var gas = diveResult.consumedDecoGases[i];
                text += gas.gas.name + "  " + gas.amount + " ltrs";
                if (i < (diveResult.consumedDecoGases.length - 1))
                    text += ", ";
            }
        }

        return text;
    },

    overwatchLevels: function () {
        var enableDisableLevels = checkbox => {
            var checked = checkbox.checked;
            $(checkbox).closest("tr").find("input[type=text]").each((ind, elem) => {
                $(elem).prop('disabled', !checked);
            });
        }

        $(".levels table.levelsTable input[type=checkbox]").each((ind, elem) => {
            enableDisableLevels(elem);
        });

        $(".levels table.levelsTable input[type=checkbox]").click(function () {
            enableDisableLevels(this);
        });

        $(".decoLevels table.levelsTable input[type=checkbox]").each((ind, elem) => {
            enableDisableLevels(elem);
        });

        $(".decoLevels table.levelsTable input[type=checkbox]").click(function () {
            enableDisableLevels(this);
        });
    }
};

$(function () {
    app.init();
});