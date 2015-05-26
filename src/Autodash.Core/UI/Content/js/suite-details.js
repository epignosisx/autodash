$(function () {
    var suiteId = $("#Id").val();
    var vm = new TestExplorer(suiteId);
    ko.applyBindings(vm, document.getElementById("test-explorer-container"));

    $("#js-test-tag-explorer").on("click", function (e) {
        e.preventDefault();
        $('#content-tabs a[href="#test-explorer"]').tab("show");
    });

    $("#js-run-suite").on("click", function(e) {
        e.preventDefault();
        $.post("/suites/" + suiteId + "/schedule");
    });
});