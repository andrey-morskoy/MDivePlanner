
var DivePlanPointType = {
    startDive: 2,
    endDive: 4,
    ascent: 8,
    descent: 16,
    bottom: 32,
    finalAscent: 64,
    deco: 128,
    safeStop: 256,
    HasPoint: function (point, pointType) {

        return (point.type & pointType) == pointType;
    }
};

var DiveResultBlockType = {
    withDeco: 0,
    depthTime: 1,
    mainGas: 2,
    decoGases: 3,
    cns: 4,
    end: 5,
    consumedGas: 6,
    maxPpO: 7,
    fullDesaturation: 8,
    noDecoTime: 9,
    ascentTime: 10
};


/*
function DivePlanPointType() {
    this.startDive = 2;
    this.endDive = 4;
    this.ascent = 8;
    this.descent = 16;
    this.bottom = 32;
    this.finalAscent = 64;
    this.deco = 128;
    this.safeStop = 256;
};
*/
function DiveResult() {
    this.depth = 0.0;
    this.totoalTime = 0.0;
};