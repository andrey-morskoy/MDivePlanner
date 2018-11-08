var graph =
{
    _context: null,
    _margin: 60,
    _marginTop: 35,
    _canvasHeight: 0,
    _canvasWidth: 0,
    _colorDiveBottom: "#00dc00",
    _colorDiveDeco: "#ff5300",
    _colorDiveAscent: "#c8d22d",
    _colorDiveSafeStop: "#f5be03",
    _colorCelingDepth: "#64b4b4",
    _colorGeneralLabel: "#505050",
    _diveResult: null,

    init: function (canvas, diveResult) {
        this._diveResult = diveResult;
        this._context = canvas.getContext("2d");
        this._canvasHeight = canvas.height;
        this._canvasWidth = canvas.width;

        this._context.fillStyle = "#fbfbfd";
        this._context.fillRect(0, 0, canvas.width, canvas.height);
        this._context.fillStyle = "black";

        var drawData = this.drawBase();
        this.drawCelingDepthPoints(drawData.timeKoef, drawData.depthKoef);
        this.drawDiveProfileGrapth();
        this.drawMarks(drawData.timeKoef, drawData.depthKoef);

    },

    drawMarks: function (timeKoef, depthKoef) {
        var ctx = this._context;
        var diveRes = this._diveResult;

        ctx.beginPath();
        ctx.lineWidth = 1;
        ctx.strokeStyle = "#aaaaaa";

        var graphHeight = this._canvasHeight - this._margin - this._marginTop;
        var graphWidth = this._canvasWidth - 2 * this._margin;

        var bottomPoints = 0;
        var startAscentPoint = null;

        for (var i = 0; i < diveRes.planPoints.length; i++) {
            var point = diveRes.planPoints[i];

            var x = 1 + this._margin + point.absoluteTime * timeKoef;
            var y = this._marginTop + point.depth * depthKoef;

            if (DivePlanPointType.HasPoint(point, DivePlanPointType.startDive)) {
                var lastPoint = diveRes.planPoints[diveRes.planPoints.length - 1];
                var lastPntX = 1 + this._margin + lastPoint.absoluteTime * timeKoef;

                ctx.beginPath();
                ctx.moveTo(this._margin, this._marginTop);
                ctx.lineTo(lastPntX, this._marginTop);
                ctx.stroke();
            }

            if (DivePlanPointType.HasPoint(point, DivePlanPointType.bottom) &&
               !DivePlanPointType.HasPoint(point, DivePlanPointType.finalAscent)) {
                if (bottomPoints % 2 == 0) {
                    ctx.beginPath();
                    ctx.moveTo(this._margin, y);
                    ctx.lineTo(x, y);

                    ctx.moveTo(x, y);
                    ctx.lineTo(x, this._marginTop + graphHeight);
                    ctx.stroke();
                }
                else {
                    ctx.beginPath();
                    ctx.moveTo(x, y);
                    ctx.lineTo(x, this._marginTop + graphHeight);
                    ctx.stroke();
                }
                ++bottomPoints;
            }

            if (DivePlanPointType.HasPoint(point, DivePlanPointType.finalAscent) ||
                DivePlanPointType.HasPoint(point, DivePlanPointType.endDive)) {
                ctx.moveTo(x, y);
                ctx.lineTo(x, this._marginTop + graphHeight);
                ctx.stroke();
            }

            if (DivePlanPointType.HasPoint(point, DivePlanPointType.finalAscent) && startAscentPoint == null) {
                startAscentPoint = point;
            }

            if ((DivePlanPointType.HasPoint(point, DivePlanPointType.deco) || DivePlanPointType.HasPoint(point, DivePlanPointType.safeStop)) &&
                !DivePlanPointType.HasPoint(point, DivePlanPointType.finalAscent)) {

                ctx.beginPath();
                ctx.moveTo(this._margin, y);
                ctx.lineTo(x, y);

                ctx.moveTo(x, y);
                ctx.lineTo(x, this._marginTop + graphHeight);
                ctx.stroke();

                ctx.beginPath();
                ctx.fillStyle = this._colorDiveDeco;
                ctx.fillRect(this._margin - 2, y - 2, 4, 4);

                var isSafeStop = DivePlanPointType.HasPoint(point, DivePlanPointType.safeStop);
                var timeText = this.getTimeStr(point.duration);
                var text = Math.round(point.depth) + "m " + (isSafeStop ? " (safety stop)" : "") + " [" + timeText + "]";
                var textX = this._margin + startAscentPoint.absoluteTime * 0.25 * timeKoef;

                ctx.fillStyle = this._colorGeneralLabel;
                ctx.fillText(text, textX, y - 15);
                ctx.fillText(timeText, x, y - 15);

            }
        }

        //draw NDL
        if (diveRes.dynamicNoDecoDepthTime != null) {
            diveRes.dynamicNoDecoDepthTime

            var noDecoX = 1 + this._margin + diveRes.dynamicNoDecoDepthTime.time * timeKoef;
            var y = this._marginTop + diveRes.dynamicNoDecoDepthTime.depth * depthKoef;

            ctx.beginPath();
            ctx.moveTo(noDecoX, y);
            ctx.lineTo(noDecoX, this._marginTop + graphHeight);
            ctx.stroke();

            ctx.fillStyle = this._colorDiveDeco;
            ctx.fillRect(noDecoX - 3, y - 3, 6, 6);

            ctx.fillStyle = this._colorGeneralLabel;
            ctx.fillText("NDL", noDecoX - 11, y - 16);
        }
    },

    drawDiveProfileGrapth: function () {
        var ctx = this._context;
        var diveRes = this._diveResult;
        var prevPoint = null;
        var startEndPointRadius = 5;

        var graphHeight = this._canvasHeight - this._margin - this._marginTop;
        var graphWidth = this._canvasWidth - 2 * this._margin;

        //debugger;

        var depthKoef = graphHeight / (diveRes.maxDepth + 5.0);
        var timeKoef = graphWidth / this.getMaxDiveTime();

        ctx.beginPath();
        ctx.lineWidth = 3;
        ctx.strokeStyle = this._colorDiveBottom;

        for (var i = 0; i < diveRes.planPoints.length; i++) {
            var point = diveRes.planPoints[i];
           
            var x = 1 + this._margin + point.absoluteTime * timeKoef;
            var y = this._marginTop + point.depth * depthKoef;

            if (prevPoint != null) {
                if (DivePlanPointType.HasPoint(prevPoint.divePoint, DivePlanPointType.deco) || 
                    DivePlanPointType.HasPoint(prevPoint.divePoint, DivePlanPointType.safeStop)) {
                    var decoLineY = graphHeight + this._marginTop - 2;

                    if (ctx.strokeStyle == this._colorDiveDeco || ctx.strokeStyle == this._colorDiveSafeStop) {
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

            if (DivePlanPointType.HasPoint(point, DivePlanPointType.deco)) {
                ctx.strokeStyle = this._colorDiveDeco;
            }
            else if (DivePlanPointType.HasPoint(point, DivePlanPointType.safeStop)) {
                ctx.strokeStyle = this._colorDiveSafeStop
            }

            if (DivePlanPointType.HasPoint(point, DivePlanPointType.finalAscent)) {
                ctx.strokeStyle = this._colorDiveAscent;
            }

            prevPoint = {
                divePoint: point, pointXY: { x: x, y: y }
            };
        }

        ctx.stroke();

        // start & end dive flat points
        ctx.beginPath();
        ctx.strokeStyle = this._colorDiveBottom;
        ctx.fillStyle = this._colorDiveBottom;
        ctx.arc(1 + this._margin, this._marginTop, startEndPointRadius, 0, 2 * Math.PI);
        ctx.fill();

        ctx.beginPath();
        ctx.fillStyle = this._colorDiveBottom;
        ctx.arc(prevPoint.pointXY.x, prevPoint.pointXY.y, startEndPointRadius, 0, 2 * Math.PI);
        ctx.fill();

    },

    drawCelingDepthPoints: function (timeKoef, depthKoef) {
        var ctx = this._context;
        var diveRes = this._diveResult;
        var lastPoint = null;
        var prevPoint = { x: 0, y: 0 };

        if (diveRes.ceilingDepthPoints == null)
            return;

        ctx.beginPath();
        ctx.lineWidth = 2;
        ctx.strokeStyle = this._colorCelingDepth;

        // draw m-values
        for (var i = 0; i < diveRes.ceilingDepthPoints.length; i++) {
            var point = diveRes.ceilingDepthPoints[i];

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
    },

    getMaxDiveTime: function () {
        return this._diveResult.totalTime + 5;
    },

    drawBase: function () {
        //debugger;
        var graphZeroHeight = this._canvasHeight - this._margin;

        var ctx = this._context;

        ctx.beginPath();
        ctx.lineWidth = 2;
        ctx.strokeStyle = 'black';
        ctx.font = "12.7px Arial";
        ctx.fillStyle = "black";
        ctx.textAlign = "start";
        ctx.textBaseline = "top";

        // axes
        var graphHeight = graphZeroHeight - this._marginTop;
        var graphWidth = this._canvasWidth - 2 * this._margin;

        ctx.moveTo(this._margin, this._marginTop);
        ctx.lineTo(this._margin, graphZeroHeight);

        ctx.moveTo(this._margin, graphZeroHeight);
        ctx.lineTo(this._canvasWidth - this._margin, graphZeroHeight);

        ctx.fillStyle = "#505050";

        // Y-line
        var points = this._diveResult.maxDepth + 5;
        var depthKoef = graphHeight / points;
        var startY = 0.0;

        for (var i = 0; i < points; i++) {
            var y = this._marginTop + i * depthKoef;

            if (i % 5 == 0 && (y - startY) > 25) {
                var lineSize = 8;
                ctx.moveTo(this._margin - lineSize, y);
                ctx.lineTo(this._margin + 1, y);

                var text = i.toString() + " m";
                var textSize = ctx.measureText(text);
                ctx.fillText(text, this._margin - lineSize - textSize.width - 5, y - 6);

                startY = y;
            } else {
                var lineSize = 5;
                ctx.moveTo(this._margin - lineSize, y);
                ctx.lineTo(this._margin + 1, y);
            }
        }

        // X-line
        points = this._diveResult.totalTime + 5;
        var timeKoef = graphWidth / points;
        var startX = 0.0;

        for (var i = 0; i < points; i++) {
            var x = this._margin + i * timeKoef;

            if (i % 5 == 0 && (x - startX) > 45) {
                var lineSize = 9;
                ctx.moveTo(x, graphZeroHeight - 1);
                ctx.lineTo(x, graphZeroHeight + lineSize);

                var text = utils.getTimeStr(i);
                var textSize = ctx.measureText(text);

                ctx.fillText(text, x - textSize.width / 2, graphZeroHeight + lineSize + 3);

                startX = x;
            } else {
                var lineSize = 5;
                ctx.moveTo(x, graphZeroHeight - 1);
                ctx.lineTo(x, graphZeroHeight + lineSize);
            }
        }

        // axes labels
        var label = "Depth";
        var textSize = ctx.measureText(label);
        ctx.fillText(label, this._margin - textSize.width / 2, this._marginTop - 23);

        label = "Time";
        var textSize = ctx.measureText(label);
        ctx.fillText(label, this._margin + graphWidth + 9, this._marginTop + graphHeight - 6);

        ctx.stroke();

        return { timeKoef: timeKoef, depthKoef: depthKoef };
    },

    getTimeStr: function (mins) {
        var totalMins = Math.floor(mins).toString();
        if (totalMins.length == 1)
            totalMins = "0" + totalMins;

        var secsPart = mins - Math.floor(mins);
        var secs = "00"
        if (secsPart > Number.EPSILON) {
            secs = Math.round(secsPart * 60.0).toString();
            if (secs.length == 1)
                secs = "0" + secs;
        }

        return totalMins + ":" + secs;
    }
};