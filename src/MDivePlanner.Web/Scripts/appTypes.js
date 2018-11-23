export var DivePlanPointType;
(function (DivePlanPointType) {
    DivePlanPointType[DivePlanPointType["startDive"] = 2] = "startDive";
    DivePlanPointType[DivePlanPointType["endDive"] = 4] = "endDive";
    DivePlanPointType[DivePlanPointType["ascent"] = 8] = "ascent";
    DivePlanPointType[DivePlanPointType["descent"] = 16] = "descent";
    DivePlanPointType[DivePlanPointType["bottom"] = 32] = "bottom";
    DivePlanPointType[DivePlanPointType["finalAscent"] = 64] = "finalAscent";
    DivePlanPointType[DivePlanPointType["deco"] = 128] = "deco";
    DivePlanPointType[DivePlanPointType["safeStop"] = 256] = "safeStop";
})(DivePlanPointType || (DivePlanPointType = {}));
;
export var DiveResultBlockType;
(function (DiveResultBlockType) {
    DiveResultBlockType[DiveResultBlockType["withDeco"] = 0] = "withDeco";
    DiveResultBlockType[DiveResultBlockType["depthTime"] = 1] = "depthTime";
    DiveResultBlockType[DiveResultBlockType["mainGas"] = 2] = "mainGas";
    DiveResultBlockType[DiveResultBlockType["decoGases"] = 3] = "decoGases";
    DiveResultBlockType[DiveResultBlockType["cns"] = 4] = "cns";
    DiveResultBlockType[DiveResultBlockType["end"] = 5] = "end";
    DiveResultBlockType[DiveResultBlockType["consumedGas"] = 6] = "consumedGas";
    DiveResultBlockType[DiveResultBlockType["maxPpO"] = 7] = "maxPpO";
    DiveResultBlockType[DiveResultBlockType["fullDesaturation"] = 8] = "fullDesaturation";
    DiveResultBlockType[DiveResultBlockType["noDecoTime"] = 9] = "noDecoTime";
    DiveResultBlockType[DiveResultBlockType["ascentTime"] = 10] = "ascentTime";
})(DiveResultBlockType || (DiveResultBlockType = {}));
export class Color {
    constructor(r, g, b) {
        this._color = "";
        this.set(r, g, b);
    }
    get color() {
        return this._color;
    }
    set(r, g, b) {
        r = Color.checkVal(r);
        g = Color.checkVal(g);
        b = Color.checkVal(b);
        this._color = `#${Color.hex(r)}${Color.hex(g)}${Color.hex(b)}`;
    }
    getColor() {
        return this._color;
    }
    static fromRgb(r, g, b) {
        r = this.checkVal(r);
        g = this.checkVal(g);
        b = this.checkVal(b);
        return `#${this.hex(r)}${this.hex(g)}${this.hex(b)}`;
    }
    static from(all) {
        all = this.checkVal(all);
        return `#${this.hex(all)}${this.hex(all)}${this.hex(all)}`;
    }
    static hex(val) {
        const hexMax = 16;
        return isNaN(val) ? "00" : Color._hexDigits[(val - val % hexMax) / hexMax] + Color._hexDigits[val % hexMax];
    }
    static checkVal(val) {
        const max = 255;
        if (val < 0)
            val = 0;
        if (val > max)
            val = max;
        return val;
    }
}
Color._hexDigits = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f"];
export class Utils {
    constructor() { }
    static HasPoint(point, pointType) {
        return (point.type & pointType) == pointType;
    }
    // returns "hh:mm"
    static getTimeHrsStr(timeMins) {
        let hrsStr = Math.floor(timeMins / 60).toString();
        if (hrsStr.length == 1)
            hrsStr = "0" + hrsStr;
        let minsStr = (timeMins % 60).toString();
        if (minsStr.length == 1)
            minsStr = "0" + minsStr;
        return `${hrsStr}:${minsStr}`;
    }
    // returns "mm:ss"
    static getTimeMinsStr(timeMins) {
        let totalMins = Math.floor(timeMins).toString();
        if (totalMins.length == 1)
            totalMins = "0" + totalMins;
        let secsPart = timeMins - Math.floor(timeMins);
        let secs = "00";
        if (secsPart > Number.EPSILON) {
            secs = Math.round(secsPart * 60.0).toString();
            if (secs.length == 1)
                secs = "0" + secs;
        }
        return `${totalMins}:${secs}`;
    }
}
//# sourceMappingURL=appTypes.js.map