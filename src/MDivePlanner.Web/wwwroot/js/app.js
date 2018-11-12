// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

var app = {
    init: function () {
        var context = this;

        context.overwatchLevels();

        $("#SubmitDiveParams").click((e) => {
            e.preventDefault();

            this.apiCall("/app/params", $("#SubmitDiveParamsForm").serialize(), "", "post", result => {
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
                if (valid) {
                    $("table.result-table tbody").html("");

                    this.apiCall("/app/result", null, "json", "get", result => {
                        context.fillResultTable(result);
                        context.onGotResult(result);
                    });
                }
            });
            
            return false;
        });
    },

    apiCall: function (url, data, dataType, method, resultCallback) {
        $.ajax({
            url: window.location.origin + url,
            type: method,
            data: data,
            dataType: dataType,
            success: function (result) {
                resultCallback(result);
            }
        });
    },

    onGotResult: function (result) {
        var canvas = $("#OutputCanvas")[0];

        try {
            $("#divePlanResultErrorsOutput").text($("#divePlanResultErrors").val());
            if (result.noData != null) {
            }
            else 
                graph.init(canvas, result, false);
        }
        catch (x) {
            $("#divePlanResultErrorsOutput").text(x);
        }
    },

    fillResultTable: function (result) {
        var diveResult = result;
        if (diveResult.noData) {
        }
        else {
            var findBlock = (id) => {
                var block = null;
                if (diveResult.diveResultBlocks == null)
                    return null;

                for (var i = 0; i < diveResult.diveResultBlocks.length; i++) {
                    if (diveResult.diveResultBlocks[i].type == id) {
                        block = diveResult.diveResultBlocks[i];
                        break;
                    }
                }
                return block;
            };

            var getClasses = (block) => {
                return " class ='" + (block.warning ? "warning" : "") + " " +
                    (block.dangerous ? "dangerous" : "") + " " +
                    (block.important ? "important" : "") + "'";
            };

            var decoWarningBlock = findBlock(DiveResultBlockType.withDeco);
            var diveInfoBlock = findBlock(DiveResultBlockType.depthTime);
            var ppOBlock = findBlock(DiveResultBlockType.maxPpO);
            var cnsBlock = findBlock(DiveResultBlockType.cns);
            var endBlock = findBlock(DiveResultBlockType.end);

            var table1 = "<tr><td" + getClasses(diveInfoBlock) + ">" + diveInfoBlock.text +
                "</td><td" + getClasses(decoWarningBlock) + ">" + decoWarningBlock.text +
                "</td><td" + getClasses(ppOBlock) + ">" + ppOBlock.text +
                "</td><td>" + this.getConsumedGases(diveResult) + "</td></tr>";

            var table2 = "<tr><td" + getClasses(cnsBlock) + ">" + cnsBlock.text +
                "</td><td" + getClasses(endBlock) + ">" + endBlock.text +
                "</td><td>" + Math.round(diveResult.maxNoDecoDepthTime.time) + " mins" +
                "</td><td>" + Math.round(diveResult.totalTime - diveResult.bottomTime) + " mins" +
                "</td><td>" + Math.round(diveResult.fullDesaturationTime / 60) + " hours</td></tr>";

            $("table.result-table.table1 tbody").html(table1);
            $("table.result-table.table2 tbody").html(table2);
        }
    },

    getConsumedGases: function (diveResult) {
        var text = "";

        for (var i = 0; i < diveResult.consumedBottomGases.length; i++) {
            var gas = diveResult.consumedBottomGases[i];
            text += "<span>" + gas.gas.name + ":</span>  " + Math.ceil(gas.amount) + " ltrs";
            if (i < (diveResult.consumedBottomGases.length - 1))
                text += ", ";
        }

        if (diveResult.consumedDecoGases != null && diveResult.consumedDecoGases.length > 0) {
            text += ", ";
            for (var i = 0; i < diveResult.consumedDecoGases.length; i++) {
                var gas = diveResult.consumedDecoGases[i];
                text += "<span>" + gas.gas.name + ":</span>  " + Math.ceil(gas.amount) + " ltrs";
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