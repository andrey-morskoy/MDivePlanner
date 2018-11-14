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
export function HasPoint(point, pointType) {
    return (point.type & pointType) == pointType;
}
//# sourceMappingURL=appTypes.js.map