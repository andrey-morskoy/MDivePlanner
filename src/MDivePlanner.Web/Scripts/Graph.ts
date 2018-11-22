import { Color } from './appTypes.js'

export class DiveGraph {
    private _context: CanvasRenderingContext2D = null;
    private _diveResult: any = null;
    private _showGasesGraph: boolean = false;
    private _margin: number = 60;
    private _marginTop: number = 35;
    private _canvasHeight: number = 0;
    private _canvasWidth: number =  0;
    private _colorDiveBottom: Color = new Color(0, 220, 0);
    private _colorDiveDeco: Color = new Color(255, 83, 0);
    private _colorDiveAscent: Color = new Color(200, 210, 45);
    private _colorDiveSafeStop: Color = new Color(245, 190, 3);
    private _colorCelingDepth: Color = new Color(100, 180, 180);
    private _colorGeneralLabel: Color = new Color(80, 80, 80);

    constructor(canvas: HTMLCanvasElement, diveResult: any) {
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