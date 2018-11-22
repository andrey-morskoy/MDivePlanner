import { Color } from './appTypes.js';
export class DiveGraph {
    constructor(canvas, diveResult) {
        this._context = null;
        this._diveResult = null;
        this._showGasesGraph = false;
        this._margin = 60;
        this._marginTop = 35;
        this._canvasHeight = 0;
        this._canvasWidth = 0;
        this._colorDiveBottom = new Color(0, 220, 0);
        this._colorDiveDeco = new Color(255, 83, 0);
        this._colorDiveAscent = new Color(200, 210, 45);
        this._colorDiveSafeStop = new Color(245, 190, 3);
        this._colorCelingDepth = new Color(100, 180, 180);
        this._colorGeneralLabel = new Color(80, 80, 80);
        this._diveResult = diveResult;
        this._context = canvas.getContext("2d");
        this._canvasHeight = canvas.height;
        this._canvasWidth = canvas.width;
        this._showGasesGraph = false;
        this._context.fillStyle = Color.fromRgb(251, 251, 253);
        this._context.fillRect(0, 0, canvas.width, canvas.height);
        this._context.fillStyle = Color.fromRgb(0, 0, 0);
    }
}
//# sourceMappingURL=graph.js.map