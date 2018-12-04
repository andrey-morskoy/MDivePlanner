import { DiveResultBlockType } from './appTypes.js';
import { DiveGraph } from './graph.js';
class Application {
    constructor() {
        this._newDiveId = "new-dive";
        this._grapth = new DiveGraph($("#OutputCanvas")[0]);
        this._grapth.reset();
        let context = this;
        this.overwatchLevelTables();
        $("#SubmitDiveParams").click(e => {
            e.preventDefault();
            context.sessionCheck();
            context.calculateDive();
        });
        $("#SaveDive").click(e => {
            e.preventDefault();
            context.sessionCheck();
            context.saveDive();
        });
        $("#SavedDives").on("change", function () {
            context.sessionCheck();
            if ($(this).val() == context._newDiveId)
                context.startNewDive();
            else
                context.loadDive(Number.parseInt($(this).val().toString()));
        });
        $("#ResetDives").click(e => {
            e.preventDefault();
            context.sessionCheck();
            context.resetDives();
        });
    }
    sessionCheck() {
        this.apiCall("/app/session", null, "", "get", result => {
            if (result.newSession === true) {
                alert("Current session has been expired. All data is being reset.");
                document.location.reload(true);
            }
        });
    }
    resetDives() {
        this.apiCall("/app/newdive?resetAll=true", null, "", "get", result => {
            $("#DiveParamsContainer").html(result);
            this.overwatchLevelTables();
            $("table.result-table.table1 tbody").html("");
            $("table.result-table.table2 tbody").html("");
            this._grapth.reset();
            $("#SavedDives").html("");
            $("#SavedDives").append($('<option>', {
                value: this._newDiveId,
                text: "New Dive",
                selected: 'selected'
            }));
        });
    }
    loadDive(id) {
        this.apiCall("/app/loaddive?id=" + id, null, "", "get", result => {
            $("#DiveParamsContainer").html(result);
            this.overwatchLevelTables();
            this.calculateDive();
        });
    }
    startNewDive() {
        this.apiCall("/app/newdive", null, "", "get", result => {
            $("#DiveParamsContainer").html(result);
            this.overwatchLevelTables();
            $("table.result-table.table1 tbody").html("");
            $("table.result-table.table2 tbody").html("");
            this._grapth.reset();
        });
    }
    saveDive() {
        let context = this;
        this.apiCall("/app/dive", null, "", "post", result => {
            if (result.diveName) {
                var options = $("#SavedDives option");
                let found = false;
                for (let opt of options) {
                    if ($(opt).attr("value") == result.diveId) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    options.removeAttr('selected');
                    $("#SavedDives").append($('<option>', {
                        value: result.diveId,
                        text: result.diveName,
                        selected: 'selected'
                    }));
                }
            }
            else {
                alert("There is no current dive to save");
            }
        });
    }
    calculateDive() {
        let context = this;
        let diveId = $("#SavedDives").val();
        let idParam = "";
        if (diveId != this._newDiveId)
            idParam = "?id=" + diveId;
        this.apiCall("/app/calculate" + idParam, $("#SubmitDiveParamsForm").serialize(), "", "post", result => {
            $("#DiveParamsContainer").html(result);
            this.overwatchLevelTables();
            let markInvalid = elem => {
                if ($(elem).val().toString().toLowerCase() == "false") {
                    let checkbox = $(elem).parent().find("input[type=checkbox]");
                    if (checkbox.is(':checked'))
                        checkbox.addClass("input-validation-error");
                }
            };
            $(".levels .levelsTable input.levelValid").each((i, elem) => { markInvalid(elem); });
            $(".decoLevels .levelsTable input.levelValid").each((i, elem) => { markInvalid(elem); });
            $("table.result-table.table1 tbody").html("");
            $("table.result-table.table2 tbody").html("");
            let valid = $("#DiveParamsValid", result).val() == "true";
            if (valid) {
                this.apiCall("/app/result", null, "json", "get", result => {
                    context.fillResultTable(result);
                    context.onGotResult(result, diveId.toString());
                });
            }
            else {
                this._grapth.reset();
            }
        });
    }
    onGotResult(result, id) {
        let errorOutput = $("#divePlanResultErrorsOutput");
        try {
            let serverError = $("#divePlanResultErrors").val().toString();
            errorOutput.text(serverError);
            if (result.noData) {
                errorOutput.text("No data from server");
            }
            else {
                if (id != this._newDiveId) {
                    $("#SavedDives :selected").text(result.description);
                }
                this._grapth.draw(result, false);
            }
        }
        catch (x) {
            errorOutput.text(x);
        }
    }
    overwatchLevelTables() {
        let enableDisableLevels = checkbox => {
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
    fillResultTable(diveResult) {
        if (diveResult == null || diveResult.noData) {
            let html = $("#divePlanResultErrorsOutput").html();
            $("#divePlanResultErrorsOutput").html(html + "<br />no data from server");
        }
        else {
            let findBlock = (id) => {
                return diveResult.diveResultBlocks.find(elem => { return elem.type == id; });
            };
            let getClasses = (block) => {
                return ` class ='${block.warning ? "warning" : ""} ${block.dangerous ? "dangerous" : ""} ${block.important ? "important" : ""}'`;
            };
            let decoWarningBlock = findBlock(DiveResultBlockType.withDeco);
            let diveInfoBlock = findBlock(DiveResultBlockType.depthTime);
            let ppOBlock = findBlock(DiveResultBlockType.maxPpO);
            let cnsBlock = findBlock(DiveResultBlockType.cns);
            let endBlock = findBlock(DiveResultBlockType.end);
            let table1 = `<tr><td ${getClasses(diveInfoBlock)}>${diveInfoBlock.text}` +
                `</td><td ${getClasses(decoWarningBlock)}>${decoWarningBlock.text}` +
                `</td><td ${getClasses(ppOBlock)}>${ppOBlock.text}` +
                `</td><td> ${this.getConsumedGases(diveResult)}</td></tr>`;
            let table2 = `<tr><td ${getClasses(cnsBlock)}> ${cnsBlock.text}` +
                `</td><td ${getClasses(endBlock)}> ${endBlock.text}` +
                `</td><td> ${Math.round(diveResult.maxNoDecoDepthTime.time)} mins` +
                `</td><td> ${Math.round(diveResult.totalTime - diveResult.bottomTime)} mins` +
                `</td><td> ${Math.round(diveResult.fullDesaturationTime / 60)} hours</td></tr>`;
            $("table.result-table.table1 tbody").html(table1);
            $("table.result-table.table2 tbody").html(table2);
        }
    }
    getConsumedGases(diveResult) {
        let addGases = (gases) => {
            let res = new Array();
            for (let gas of gases)
                res.push(`<span> ${gas.gas.name}:</span> ${Math.ceil(gas.amount)} ltrs`);
            return res.join(", ");
        };
        let text = addGases(diveResult.consumedBottomGases);
        if (diveResult.consumedDecoGases && diveResult.consumedDecoGases.length > 0) {
            text += ", " + addGases(diveResult.consumedDecoGases);
        }
        return text;
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
$(function () {
    var application = new Application();
});
//# sourceMappingURL=application.js.map