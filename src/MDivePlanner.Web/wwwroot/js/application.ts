import { DiveResultBlockType } from './appTypes.js'
import { DiveGraph } from './graph.js'

class Application {
    constructor() {
        let context: Application = this;
        this.overwatchLevelTables();

        $("#SubmitDiveParams").click((e) => {
            e.preventDefault();
            context.calculateDive();
        });
    }

    calculateDive() {
        let context: Application = this;
        this.apiCall("/app/params", $("#SubmitDiveParamsForm").serialize(), "", "post", result => {
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

            let valid = $("#DiveParamsValid", result).val() == "true";
            if (valid) {
                $("table.result-table tbody").html("");

                this.apiCall("/app/result", null, "json", "get", result => {
                    context.fillResultTable(result);
                    context.onGotResult(result);
                });
            }
        });
    }

    onGotResult (result) {
        let canvas = $("#OutputCanvas")[0] as HTMLCanvasElement;
        let errorOutput = $("#divePlanResultErrorsOutput");

        try {
            let serverError = $("#divePlanResultErrors").val().toString();
            errorOutput.text(serverError);

            if (result.noData) {
                errorOutput.text("No data from server");
            }
            else {
                let grapth: DiveGraph = new DiveGraph(canvas, result);
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
        }

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
            let findBlock : Function = (id) => {
                return diveResult.diveResultBlocks.find(elem => { return elem.type == id });
            };

            let getClasses : Function = (block) => {
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

    getConsumedGases (diveResult) {
        let text = "";
        let addGases: Function = (gases: Array<any>) => {
            for (let i = 0; i < gases.length; i++) {
                let gas = gases[i];
                text += `<span> ${gas.gas.name}:</span> ${Math.ceil(gas.amount)} ltrs`;
                if (i < (gases.length - 1))
                    text += ", ";
            }
        }

        addGases(diveResult.consumedBottomGases);

        if (diveResult.consumedDecoGases && diveResult.consumedDecoGases.length > 0) {
            text += ", ";
            addGases(diveResult.consumedDecoGases);
        }

        return text;
    }

    apiCall(url: string, data: any, dataType: string, method: string, resultCallback: Function) {
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
    var application: Application = new Application();
});
