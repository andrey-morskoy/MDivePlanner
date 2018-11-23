import { DiveResultBlockType } from './appTypes.js'
import { DiveGraph } from './graph.js'

class Application {
    private readonly _grapth: DiveGraph;

    constructor() {
        this._grapth = new DiveGraph($("#OutputCanvas")[0] as HTMLCanvasElement);
        this._grapth.reset();

        let context: Application = this;
        this.overwatchLevelTables();

        $("#SubmitDiveParams").click((e) => {
            e.preventDefault();
            context.calculateDive();
        });
    }

    private calculateDive() : void {
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

            $("table.result-table.table1 tbody").html("");
            $("table.result-table.table2 tbody").html("");

            let valid = $("#DiveParamsValid", result).val() == "true";
            if (valid) {
                this.apiCall("/app/result", null, "json", "get", result => {
                    context.fillResultTable(result);
                    context.onGotResult(result);
                });
            }
            else {
                this._grapth.reset();
            }
        });
    }

    private onGotResult(result) : void {
        let errorOutput = $("#divePlanResultErrorsOutput");

        try {
            let serverError = $("#divePlanResultErrors").val().toString();
            errorOutput.text(serverError);

            if (result.noData) {
                errorOutput.text("No data from server");
            }
            else {
                this._grapth.draw(result, false);
            }
        }
        catch (x) {
            errorOutput.text(x);
        }
    }

    private overwatchLevelTables() : void {
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

    private fillResultTable(diveResult): void {
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

    private getConsumedGases(diveResult): string {
        let addGases: Function = (gases: Array<any>): string => {
            let res = new Array<string>();
            for (let gas of gases)
                res.push(`<span> ${gas.gas.name}:</span> ${Math.ceil(gas.amount)} ltrs`);
            return res.join(", ");
        }

        let text = addGases(diveResult.consumedBottomGases);

        if (diveResult.consumedDecoGases && diveResult.consumedDecoGases.length > 0) {
            text += ", " + addGases(diveResult.consumedDecoGases);
        }

        return text;
    }

    private apiCall(url: string, data: any, dataType: string, method: string, resultCallback: Function) : void {
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
