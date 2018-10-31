// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

var app = {
    init: function () {
        var context = this;

        context.overwatchLevels();

        $("#SubmitDiveParams").click((e) => {
            e.preventDefault();

            $.ajax({
                url: window.location.origin + "/App/SetParams",
                type: "post",
                data: $("#SubmitDiveParamsForm").serialize(),
                success: function (result) {
                    $("#DiveParamsContainer").html(result);

                    context.overwatchLevels();

                    var markInvalid = elem => {
                        if ($(elem).val().toLowerCase() == "false") {
                            var checkbox = $(elem).parent().find("input[type=checkbox]");
                            if (checkbox.is(':checked'))
                                checkbox.addClass("input-validation-error");
                        }
                    };

                    $(".levels .levelsTable input.levelValid").each((i, elem) => { markInvalid(elem); });
                    $(".decoLevels .levelsTable input.levelValid").each((i, elem) => { markInvalid(elem); });

                    var valid = $("#DiveParamsValid", result).val() == "true";
                    if (valid)
                        context.onGotResult();
                }
            });

            return false;
        });
    },

    onGotResult: function () {
        var canvas = $("#OutputCanvas")[0];

        var diveResult = new DiveResult();
        diveResult.depth = direParams.depth;
        diveResult.totalTime = direParams.diveTime;

        graph.init(canvas, diveResult);
    },

    overwatchLevels: function () {
        var enableDisableLevels = checkbox => {
            var checked = checkbox.checked;
            $(checkbox).closest("tr").find("input[type=text]").each((ind, elem) => {
                $(elem).prop('disabled', !checked);
            });
        }

        $(".levels table.levelsTable input[type=checkbox]").each((ind, elem) => {
            enableDisableLevels(elem);
        });

        $(".levels table.levelsTable input[type=checkbox]").click(function () {
            enableDisableLevels(this);
        });

        $(".decoLevels table.levelsTable input[type=checkbox]").each((ind, elem) => {
            enableDisableLevels(elem);
        });

        $(".decoLevels table.levelsTable input[type=checkbox]").click(function () {
            enableDisableLevels(this);
        });
    }
};

$(function () {
    app.init();
});