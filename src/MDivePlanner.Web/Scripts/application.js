import { DiveResultBlockType } from './appTypes.js';
class Application {
    constructor() {
        this.overwatchLevelTables();
        $("#SubmitDiveParams").click((e) => {
            e.preventDefault();
            var context = this;
            this.apiCall("/app/params", $("#SubmitDiveParamsForm").serialize(), "", "post", result => {
                $("#DiveParamsContainer").html(result);
                this.overwatchLevelTables();
                var markInvalid = elem => {
                    if ($(elem).val().toString().toLowerCase() == "false") {
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
                        context.diveResult = result;
                        context.fillResultTable();
                        alert(1);
                        debugger;
                        //   context.fillResultTable(result);
                        //   context.onGotResult(result);
                    });
                }
            });
        });
    }
    overwatchLevelTables() {
        var enableDisableLevels = checkbox => {
            $(checkbox).closest("tr").find("input[type=text]").each((ind, elem) => {
                $(elem).prop('disabled', !checkbox.checked);
            });
        };
        $(".levels table.levelsTable input[type=checkbox]").each((ind, elem) => {
            enableDisableLevels(elem);
        }).click(function () {
            enableDisableLevels(this);
        });
        $(".decoLevels table.levelsTable input[type=checkbox]").each((ind, elem) => {
            enableDisableLevels(elem);
        }).click(function () {
            enableDisableLevels(this);
        });
    }
    fillResultTable() {
        let diveResult = this.diveResult;
        if (diveResult == null || diveResult.noData) {
            let html = $("#divePlanResultErrorsOutput").html();
            $("#divePlanResultErrorsOutput").html(html + "<br />no data from server");
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
                "</td><td>" + /*this.getConsumedGases(diveResult)*/ +"</td></tr>";
            var table2 = "<tr><td" + getClasses(cnsBlock) + ">" + cnsBlock.text +
                "</td><td" + getClasses(endBlock) + ">" + endBlock.text +
                "</td><td>" + Math.round(diveResult.maxNoDecoDepthTime.time) + " mins" +
                "</td><td>" + Math.round(diveResult.totalTime - diveResult.bottomTime) + " mins" +
                "</td><td>" + Math.round(diveResult.fullDesaturationTime / 60) + " hours</td></tr>";
            $("table.result-table.table1 tbody").html(table1);
            $("table.result-table.table2 tbody").html(table2);
        }
    }
    apiCall(url, data, dataType, method, resultCallback) {
        $.ajax({
            url: window.location.origin + url,
            type: method,
            data: data,
            dataType: dataType,
            success: function (result) {
                resultCallback(result);
            }
        });
    }
}
let application = new Application();
//# sourceMappingURL=application.js.map