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
