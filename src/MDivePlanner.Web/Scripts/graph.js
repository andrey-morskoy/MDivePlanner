import { Color, Utils, DivePlanPointType } from './appTypes.js';
export class DiveGraph {
    constructor(canvas) {
        this._context = null;
        this._canvas = null;
        this._graphLineWidth = 4;
        this._margin = 60;
        this._marginTop = 35;
        this._colorDiveBottom = new Color(0, 220, 0);
        this._colorDiveDeco = new Color(255, 83, 0);
        this._colorDiveAscent = new Color(200, 210, 45);
        this._colorDiveSafeStop = new Color(245, 190, 3);
        this._colorCelingDepth = new Color(100, 180, 180);
        this._colorGeneralLabel = new Color(80, 80, 80);
        this._colorBackground = new Color(251, 251, 253);
        this._initialSize = { width: 0, height: 0 };
        this._canvasHeight = 0;
        this._canvasWidth = 0;
        this._graphHeight = 0;
        this._graphWidth = 0;
        this._showGasesGraph = false;
        this._diveResult = null;
        this._canvas = canvas;
        this._context = canvas.getContext("2d");
        this._canvasHeight = canvas.height;
        this._canvasWidth = canvas.width;
        this._initialSize.width = canvas.width;
        this._initialSize.height = canvas.height;
        this._showGasesGraph = false;
        this._graphHeight = this._canvasHeight - this._margin - this._marginTop;
        this._graphWidth = this._canvasWidth - 2 * this._margin;
        let graphSwitchBtn = $("#GraphSwitch");
        graphSwitchBtn.css("left", canvas.offsetLeft + canvas.width - graphSwitchBtn.outerWidth() - 5);
        graphSwitchBtn.css("top", canvas.offsetTop + 5);
        var graph = this;
        graphSwitchBtn.unbind("click").click(function (a) {
            var showGases = !graph._showGasesGraph;
            graph.draw(graph._diveResult, showGases);
            $(this).text(showGases ? "Profile" : "Gases");
        });
    }
    setSize(x, y) {
        this._canvas.width = this._initialSize.width * x;
        this._canvas.height = this._initialSize.height * y;
        this._canvasHeight = this._canvas.height;
        this._canvasWidth = this._canvas.width;
        this._graphHeight = this._canvasHeight - this._margin - this._marginTop;
        this._graphWidth = this._canvasWidth - 2 * this._margin;
        this._context.fillStyle = this._colorBackground.color;
        this._context.fillRect(0, 0, this._canvasWidth, this._canvasHeight);
        this._context.fillStyle = Color.from(0);
        let graphSwitchBtn = $("#GraphSwitch");
        graphSwitchBtn.css("left", this._canvas.offsetLeft + this._canvas.width - graphSwitchBtn.outerWidth() - 5);
        graphSwitchBtn.css("top", this._canvas.offsetTop + 5);
        let koefs = this.drawBase();
        this.drawCelingDepthPoints(koefs.depthKoef, koefs.timeKoef);
        if (this._showGasesGraph)
            this.drawGasesGrapth(koefs.depthKoef, koefs.timeKoef);
        else {
            this.drawDiveProfileGrapth(koefs.depthKoef, koefs.timeKoef);
            this.drawMarks(koefs.depthKoef, koefs.timeKoef);
        }
    }
    draw(diveResult, showGasesGrapth) {
        this._diveResult = diveResult;
        this._showGasesGraph = showGasesGrapth;
        this.reset();
        $("#GraphSwitch").prop("disabled", false);
        let koefs = this.drawBase();
        this.drawCelingDepthPoints(koefs.depthKoef, koefs.timeKoef);
        if (showGasesGrapth)
            this.drawGasesGrapth(koefs.depthKoef, koefs.timeKoef);
        else {
            this.drawDiveProfileGrapth(koefs.depthKoef, koefs.timeKoef);
            this.drawMarks(koefs.depthKoef, koefs.timeKoef);
        }
    }
    reset() {
        // clear background
        this._context.fillStyle = this._colorBackground.color;
        this._context.fillRect(0, 0, this._canvasWidth, this._canvasHeight);
        this._context.fillStyle = Color.from(0);
        let graphSwitchBtn = $("#GraphSwitch");
        graphSwitchBtn.css("left", this._canvas.offsetLeft + this._canvas.width - graphSwitchBtn.outerWidth() - 5);
        graphSwitchBtn.css("top", this._canvas.offsetTop + 5);
        graphSwitchBtn.prop("disabled", true);
        graphSwitchBtn.text("Gases");
    }
    drawGasesGrapth(depthKoef, timeKoef) {
        let ctx = this._context;
        let diveRes = this._diveResult;
        let prevPoint = null;
        let colValue = 50;
        let lineColor = Color.fromRgb(0, colValue, colValue);
        ctx.beginPath();
        ctx.lineWidth = 5;
        ctx.strokeStyle = this._colorDiveBottom.color;
        ctx.beginPath();
        ctx.lineWidth = 0;
        ctx.fillStyle = lineColor;
        ctx.arc(this._margin, this._marginTop, 8, 0, 2 * Math.PI);
        ctx.fill();
        for (let point of diveRes.planPoints) {
            let x = 1 + this._margin + point.absoluteTime * timeKoef;
            let y = this._marginTop + point.depth * depthKoef;
            var drawed = false;
            if (prevPoint != null) {
                for (let gasSwitch of diveRes.gasSwitches) {
                    if (gasSwitch.absoluteTime >= prevPoint.divePoint.absoluteTime && gasSwitch.absoluteTime <= point.absoluteTime) {
                        var gsX = 1 + this._margin + gasSwitch.absoluteTime * timeKoef;
                        var gsY = this._marginTop + gasSwitch.depth * depthKoef;
                        var prevLineColor = lineColor;
                        ctx.beginPath();
                        ctx.strokeStyle = prevLineColor;
                        ctx.moveTo(prevPoint.pointXY.x, prevPoint.pointXY.y);
                        ctx.lineTo(gsX, gsY);
                        ctx.stroke();
                        colValue += 40;
                        lineColor = Color.fromRgb(0, colValue, colValue);
                        ctx.beginPath();
                        ctx.strokeStyle = lineColor;
                        ctx.moveTo(gsX, gsY);
                        ctx.lineTo(x, y);
                        ctx.stroke();
                        ctx.beginPath();
                        ctx.lineWidth = 0;
                        ctx.fillStyle = lineColor;
                        ctx.arc(gsX, gsY, 8, 0, 2 * Math.PI);
                        ctx.fill();
                        drawed = true;
                        break;
                    }
                }
                if (!drawed) {
                    // draw level
                    ctx.beginPath();
                    ctx.strokeStyle = lineColor;
                    ctx.moveTo(prevPoint.pointXY.x, prevPoint.pointXY.y);
                    ctx.lineTo(x, y);
                    ctx.stroke();
                }
            }
            prevPoint = {
                divePoint: point, pointXY: { x: x, y: y }
            };
        }
        ctx.stroke();
        for (let gasSwitch of diveRes.gasSwitches) {
            let x = 1 + this._margin + gasSwitch.absoluteTime * timeKoef;
            let y = this._marginTop + gasSwitch.depth * depthKoef;
            ctx.beginPath();
            ctx.fillStyle = Color.from(0);
            ctx.fillText(gasSwitch.gas.name, x + 9, y + 3);
        }
    }
    drawDiveProfileGrapth(depthKoef, timeKoef) {
        var ctx = this._context;
        var diveRes = this._diveResult;
        var prevPoint = null;
        ctx.beginPath();
        ctx.lineWidth = this._graphLineWidth;
        ctx.strokeStyle = this._colorDiveBottom.color;
        for (let point of diveRes.planPoints) {
            let x = 1 + this._margin + point.absoluteTime * timeKoef;
            let y = this._marginTop + point.depth * depthKoef;
            if (prevPoint != null) {
                if (Utils.HasPoint(prevPoint.divePoint, DivePlanPointType.deco) ||
                    Utils.HasPoint(prevPoint.divePoint, DivePlanPointType.safeStop)) {
                    var decoLineY = this._graphHeight + this._marginTop - 2;
                    if (ctx.strokeStyle == this._colorDiveDeco.color || ctx.strokeStyle == this._colorDiveSafeStop.color) {
                        // x-axis deco line time 
                        ctx.beginPath();
                        ctx.moveTo(prevPoint.pointXY.x, decoLineY);
                        ctx.lineTo(x, decoLineY);
                        ctx.stroke();
                    }
                }
                // draw level
                ctx.beginPath();
                ctx.moveTo(prevPoint.pointXY.x, prevPoint.pointXY.y);
                ctx.lineTo(x, y);
                ctx.stroke();
            }
            if (Utils.HasPoint(point, DivePlanPointType.deco)) {
                ctx.strokeStyle = this._colorDiveDeco.color;
            }
            else if (Utils.HasPoint(point, DivePlanPointType.safeStop)) {
                ctx.strokeStyle = this._colorDiveSafeStop.color;
            }
            if (Utils.HasPoint(point, DivePlanPointType.finalAscent)) {
                ctx.strokeStyle = this._colorDiveAscent.color;
            }
            prevPoint = {
                divePoint: point, pointXY: { x: x, y: y }
            };
        }
    }
    drawMarks(depthKoef, timeKoef) {
        let ctx = this._context;
        let diveRes = this._diveResult;
        let bottomPoints = 0;
        let startAscentPoint = null;
        ctx.beginPath();
        ctx.lineWidth = 1;
        ctx.strokeStyle = Color.from(170);
        // trace lines for stops, levels
        for (let point of diveRes.planPoints) {
            var x = 1 + this._margin + point.absoluteTime * timeKoef;
            var y = this._marginTop + point.depth * depthKoef;
            if (Utils.HasPoint(point, DivePlanPointType.startDive)) {
                var lastPoint = diveRes.planPoints[diveRes.planPoints.length - 1];
                var lastPntX = 1 + this._margin + lastPoint.absoluteTime * timeKoef;
                ctx.beginPath();
                ctx.moveTo(this._margin, this._marginTop);
                ctx.lineTo(lastPntX, this._marginTop);
                ctx.stroke();
            }
            if (Utils.HasPoint(point, DivePlanPointType.bottom) &&
                !Utils.HasPoint(point, DivePlanPointType.finalAscent)) {
                if (bottomPoints % 2 == 0) {
                    ctx.beginPath();
                    ctx.moveTo(this._margin, y);
                    ctx.lineTo(x, y);
                    ctx.moveTo(x, y);
                    ctx.lineTo(x, this._marginTop + this._graphHeight);
                    ctx.stroke();
                }
                else {
                    ctx.beginPath();
                    ctx.moveTo(x, y);
                    ctx.lineTo(x, this._marginTop + this._graphHeight);
                    ctx.stroke();
                }
                ++bottomPoints;
            }
            if (Utils.HasPoint(point, DivePlanPointType.finalAscent) ||
                Utils.HasPoint(point, DivePlanPointType.endDive)) {
                ctx.moveTo(x, y);
                ctx.lineTo(x, this._marginTop + this._graphHeight);
                ctx.stroke();
            }
            if (Utils.HasPoint(point, DivePlanPointType.finalAscent) && startAscentPoint == null) {
                startAscentPoint = point;
            }
            if ((Utils.HasPoint(point, DivePlanPointType.deco) || Utils.HasPoint(point, DivePlanPointType.safeStop)) &&
                !Utils.HasPoint(point, DivePlanPointType.finalAscent)) {
                ctx.beginPath();
                ctx.moveTo(this._margin, y);
                ctx.lineTo(x, y);
                ctx.moveTo(x, y);
                ctx.lineTo(x, this._marginTop + this._graphHeight);
                ctx.stroke();
                ctx.beginPath();
                ctx.fillStyle = this._colorDiveDeco.color;
                ctx.fillRect(this._margin - 2, y - 2, 4, 4);
                var isSafeStop = Utils.HasPoint(point, DivePlanPointType.safeStop);
                var timeText = Utils.getTimeMinsStr(point.duration);
                var text = `${Math.round(point.depth)} m ${isSafeStop ? " (safety stop)" : ""}  [${timeText}]`;
                var textX = this._margin + startAscentPoint.absoluteTime * 0.25 * timeKoef;
                ctx.fillStyle = this._colorGeneralLabel.color;
                ctx.fillText(text, textX, y - 15);
                ctx.fillText(timeText, x, y - 15);
            }
        }
        // NDL
        this.drawNdl(depthKoef, timeKoef);
        // start & end dive flat points
        const startEndPointRadius = 5;
        ctx.beginPath();
        ctx.strokeStyle = this._colorDiveBottom.color;
        ctx.fillStyle = this._colorDiveBottom.color;
        ctx.arc(1 + this._margin, this._marginTop, startEndPointRadius, 0, 2 * Math.PI);
        ctx.fill();
        ctx.beginPath();
        ctx.fillStyle = this._colorDiveBottom.color;
        var x = 1 + this._margin + Math.ceil(diveRes.planPoints[diveRes.planPoints.length - 1].absoluteTime * timeKoef);
        ctx.arc(x, this._marginTop, startEndPointRadius, 0, 2 * Math.PI);
        ctx.fill();
        // gas switches
        this.drawGasSwitches(depthKoef, timeKoef);
        // levels info
        this.drawLevelsInfo(depthKoef, timeKoef);
    }
    drawLevelsInfo(depthKoef, timeKoef) {
        let ctx = this._context;
        let diveRes = this._diveResult;
        if (diveRes.levelsInfo != null) {
            ctx.beginPath();
            let prevFont = ctx.font;
            ctx.font = "11.7px Arial";
            for (let level of diveRes.levelsInfo) {
                var x = 1 + this._margin + level.timeReached * timeKoef;
                var y = this._marginTop + level.depth * depthKoef;
                var text = `${level.gas.name}  END: ${level.end.toFixed(1)}m  PpO: ${level.ppO.toFixed(2)}`;
                ctx.fillStyle = Color.from(0);
                ctx.fillText(text, x + 4, y + 8);
            }
            ctx.font = prevFont;
        }
    }
    drawGasSwitches(depthKoef, timeKoef) {
        let ctx = this._context;
        let diveRes = this._diveResult;
        if (diveRes.gasSwitches != null) {
            let image = new Image();
            let that = this;
            image.onload = () => {
                for (let gasSwitch of diveRes.gasSwitches) {
                    let x = 1 + that._margin + gasSwitch.absoluteTime * timeKoef;
                    let y = that._marginTop + gasSwitch.depth * depthKoef;
                    let lineY = that._graphHeight + that._marginTop;
                    ctx.beginPath();
                    let prevFont = ctx.font;
                    ctx.font = "11.7px Arial";
                    if (gasSwitch.isDeco) {
                        ctx.strokeStyle = ctx.fillStyle = "#007d00";
                        ctx.moveTo(x, y);
                        ctx.lineTo(x, lineY);
                        ctx.moveTo(1 + that._margin, y);
                        ctx.lineTo(x, y);
                        ctx.stroke();
                    }
                    ctx.fillStyle = "#009600";
                    ctx.arc(x, y, 4, 0, 2 * Math.PI);
                    ctx.fill();
                    if (gasSwitch.isDeco) {
                        let text = "Deco Gas (" + Math.round(gasSwitch.gas.ppO * 100.0) + "% O2, ppO " + gasSwitch.ppO.toFixed(2) + ")";
                        let textSize = ctx.measureText(text);
                        let textStart = { x: x - textSize.width - 12, y: y - 14 };
                        ctx.drawImage(image, textStart.x - image.width - 1, y - image.height);
                        ctx.fillStyle = ctx.strokeStyle;
                        ctx.fillText(text, textStart.x, textStart.y);
                    }
                    ctx.font = prevFont;
                }
            };
            image.src = document.URL + "images/deco_tank.png";
        }
    }
    drawNdl(depthKoef, timeKoef) {
        let ctx = this._context;
        let diveRes = this._diveResult;
        if (diveRes.dynamicNoDecoDepthTime != null && diveRes.dynamicNoDecoDepthTime.time > 0) {
            var noDecoX = 1 + this._margin + diveRes.dynamicNoDecoDepthTime.time * timeKoef;
            var y = this._marginTop + diveRes.dynamicNoDecoDepthTime.depth * depthKoef;
            ctx.beginPath();
            ctx.moveTo(noDecoX, y);
            ctx.lineTo(noDecoX, this._marginTop + this._graphHeight);
            ctx.stroke();
            ctx.fillStyle = this._colorDiveDeco.color;
            ctx.fillRect(noDecoX - 3, y - 3, 6, 6);
            var prevFont = ctx.font;
            ctx.font = "11.7px Arial";
            ctx.fillStyle = this._colorGeneralLabel.color;
            ctx.fillText("NDL", noDecoX - 11, y - 16);
            ctx.font = prevFont;
        }
    }
    drawCelingDepthPoints(depthKoef, timeKoef) {
        let ctx = this._context;
        let diveRes = this._diveResult;
        let lastPoint = null;
        let prevPoint = { x: 0, y: 0 };
        if (!diveRes.ceilingDepthPoints)
            return;
        ctx.beginPath();
        ctx.lineWidth = 2;
        ctx.strokeStyle = this._colorCelingDepth.color;
        // draw m-values
        for (let point of diveRes.ceilingDepthPoints) {
            var x = 1 + this._margin + point.time * timeKoef;
            var y = this._marginTop + point.depth * depthKoef;
            lastPoint = { x: x, y: y };
            if (prevPoint.x > 0) {
                ctx.moveTo(prevPoint.x, prevPoint.y);
                ctx.lineTo(lastPoint.x, lastPoint.y);
            }
            prevPoint = lastPoint;
        }
        ctx.moveTo(prevPoint.x, prevPoint.y);
        ctx.lineTo(lastPoint.x, lastPoint.y);
        ctx.stroke();
    }
    drawBase() {
        let graphZeroHeight = this._canvasHeight - this._margin;
        let ctx = this._context;
        ctx.beginPath();
        ctx.lineWidth = 2;
        ctx.strokeStyle = Color.from(0);
        ctx.fillStyle = Color.from(80);
        ctx.font = "12.7px Arial";
        ctx.textAlign = "start";
        ctx.textBaseline = "top";
        // axes
        ctx.moveTo(this._margin, this._marginTop);
        ctx.lineTo(this._margin, graphZeroHeight);
        ctx.moveTo(this._margin, graphZeroHeight);
        ctx.lineTo(this._canvasWidth - this._margin, graphZeroHeight);
        // Y-line
        let points = this._diveResult.maxDepth + 5;
        let depthKoef = this._graphHeight / points;
        let startY = 0.0;
        for (let i = 0; i < points; i++) {
            let y = this._marginTop + i * depthKoef;
            if (i % 5 == 0 && (y - startY) > 25) {
                const lineSize = 8;
                ctx.moveTo(this._margin - lineSize, y);
                ctx.lineTo(this._margin + 1, y);
                let text = i.toString() + " m";
                let textSize = ctx.measureText(text);
                ctx.fillText(text, this._margin - lineSize - textSize.width - 5, y - 6);
                startY = y;
            }
            else {
                const lineSize = 5;
                ctx.moveTo(this._margin - lineSize, y);
                ctx.lineTo(this._margin + 1, y);
            }
        }
        // X-line
        points = this._diveResult.totalTime + 5;
        let timeKoef = this._graphWidth / points;
        let startX = 0.0;
        for (let i = 0; i < points; i++) {
            let x = this._margin + i * timeKoef;
            if (i % 5 == 0 && (x - startX) > 45) {
                const lineSize = 9;
                ctx.moveTo(x, graphZeroHeight - 1);
                ctx.lineTo(x, graphZeroHeight + lineSize);
                let text = Utils.getTimeHrsStr(i);
                let textSize = ctx.measureText(text);
                ctx.fillText(text, x - textSize.width / 2, graphZeroHeight + lineSize + 3);
                startX = x;
            }
            else {
                const lineSize = 5;
                ctx.moveTo(x, graphZeroHeight - 1);
                ctx.lineTo(x, graphZeroHeight + lineSize);
            }
        }
        // axes labels
        let label = "Depth";
        let textSize = ctx.measureText(label);
        ctx.fillText(label, this._margin - textSize.width / 2, this._marginTop - 23);
        label = "Time";
        textSize = ctx.measureText(label);
        ctx.fillText(label, this._margin + this._graphWidth + 9, this._marginTop + this._graphHeight - 6);
        ctx.stroke();
        return { depthKoef, timeKoef };
    }
}
//# sourceMappingURL=Graph.js.map