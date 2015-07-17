$(function() {
    $(document).on("click", ".cancel-suite-run", function (e) {
        e.preventDefault();
        var id = $(this).attr("data-id");
        $.post("/runs/cancel", { id: id }, function (response) {
            window.location.reload();
        });
    });
});
