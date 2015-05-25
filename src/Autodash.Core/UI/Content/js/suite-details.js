$(function () {
    var vm = new TestExplorer($("#Id").val());
    ko.applyBindings(vm, document.getElementById("test-explorer-container"));

    $("#js-test-tag-explorer").on("click", function (e) {
        e.preventDefault();
        $('#content-tabs a[href="#test-explorer"]').tab("show");
    });
});