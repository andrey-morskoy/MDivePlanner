var graph =
{
    _context: null,
    _margin: 60,
    _marginTop: 30,
    _canvasHeight: 0,
    _canvasWidth: 0,
    _diveResult: null,

    init: function (canvas, diveResult) {
        this._diveResult = diveResult;
        this._context = canvas.getContext("2d");
        this._canvasHeight = canvas.height;
        this._canvasWidth = canvas.width;

        this._context.clearRect(0, 0, canvas.width, canvas.height);

        this.drawBase();
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
        var points = this._diveResult.depth + 5;
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

        ctx.stroke();
    }
};