// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

var app = {
    init: function () {
        var context = this;
        $("#SubmitDiveParams").click(function (e) {
            e.preventDefault();

            $.ajax({
                url: window.location.origin + "/App/SetParams",
                type: "post",
                data: $("#SubmitDiveParamsForm").serialize(),
                success: function (result) {
                    $("#DiveParamsContainer").html(result);

                    var valid = $("#DiveParamsValid", result).val() == "true";
                    if (valid)
                        context.onGotResult();
                        context.onGotResult();
                }
            });

            return false;
        });

        $(".levels button.add").click(function () {
            $("table.levelsTable .body").append("<tr><td>1</td> <td>2</td> <td>3</td> <td>4</td> </tr>");
        });
    },

    onGotResult: function () {
        var canvas = $("#OutputCanvas")[0];

        var diveResult = new DiveResult();
        diveResult.depth = direParams.depth;
        diveResult.totalTime = direParams.diveTime;

        graph.init(canvas, diveResult);
    }
};

$(function () {
    app.init();
});