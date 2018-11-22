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

export function HasPoint(point: any, pointType: DivePlanPointType) {
    return (point.type & pointType) == pointType;
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