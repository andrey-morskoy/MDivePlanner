var utils = {
    // returns "hh::mm"
    getTimeStr: function (timeMins) {
        var hrsStr = Math.floor(timeMins / 60).toString();
        if (hrsStr.length == 1)
            hrsStr = "0" + hrsStr;

        var minsStr = (timeMins % 60).toString();
        if (minsStr.length == 1)
            minsStr = "0" + minsStr;

        return hrsStr + ":" + minsStr;
    }
};