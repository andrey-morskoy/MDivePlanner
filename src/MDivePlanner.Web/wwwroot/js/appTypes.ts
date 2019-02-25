export enum DivePlanPointType {
    startDive = 2,
    endDive = 4,
    ascent = 8,
    descent = 16,
    bottom = 32,
    finalAscent = 64,
    deco = 128,
    safeStop = 256
};

export enum DiveResultBlockType {
    withDeco = 0,
    depthTime = 1,
    mainGas = 2,
    decoGases = 3,
    cns = 4,
    end = 5,
    consumedGas = 6,
    maxPpO = 7,
    fullDesaturation = 8,
    noDecoTime = 9,
    ascentTime = 10
}

export class Color {
    private static _hexDigits: Array<string> = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f"];
    private _color: string = "";

    constructor(r: number, g: number, b: number) {
        this.set(r, g, b);
    }

    get color(): string {
        return this._color;
    }

    public set(r: number, g: number, b: number): void {
        r = Color.checkVal(r);
        g = Color.checkVal(g);
        b = Color.checkVal(b);

        this._color = `#${Color.hex(r)}${Color.hex(g)}${Color.hex(b)}`;
    }

    public getColor(): string {
        return this._color;
    }

    public static fromRgb(r: number, g: number, b: number): string {
        r = this.checkVal(r);
        g = this.checkVal(g);
        b = this.checkVal(b);

        return `#${this.hex(r)}${this.hex(g)}${this.hex(b)}`;
    }

    public static from(all: number): string {
        all = this.checkVal(all);

        return `#${this.hex(all)}${this.hex(all)}${this.hex(all)}`;
    }

    private static hex(val: number): string {
        const hexMax = 16;
        return isNaN(val) ? "00" : Color._hexDigits[(val - val % hexMax) / hexMax] + Color._hexDigits[val % hexMax];
    }

    private static checkVal(val: number): number {
        const max = 255;
        if (val < 0)
            val = 0;
        if (val > max)
            val = max;
        return val;
    }
}

export class Utils {
    private constructor() { }

    public static HasPoint(point: any, pointType: DivePlanPointType): boolean {
        return (point.type & pointType) == pointType;
    }

    // returns "hh:mm"
    public static getTimeHrsStr(timeMins: number): string {
        let hrsStr = Math.floor(timeMins / 60).toString();
        if (hrsStr.length == 1)
            hrsStr = "0" + hrsStr;

        let minsStr = (timeMins % 60).toString();
        if (minsStr.length == 1)
            minsStr = "0" + minsStr;

        return `${hrsStr}:${minsStr}`;
    }

    // returns "mm:ss"
    public static getTimeMinsStr(timeMins: number): string {
        let totalMins = Math.floor(timeMins).toString();
        if (totalMins.length == 1)
            totalMins = "0" + totalMins;

        let secsPart = timeMins - Math.floor(timeMins);
        let secs = "00"
        if (secsPart > Number.EPSILON) {
            secs = Math.round(secsPart * 60.0).toString();
            if (secs.length == 1)
                secs = "0" + secs;
        }

        return `${totalMins}:${secs}`;
    }
}