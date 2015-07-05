function TestExplorer(suiteId) {
    var self = this;
    self.suiteId = suiteId;
    self.unitTestCollections = ko.observableArray([]);
    self.query = ko.observable();
    self.error = ko.observable();
    self.tags = ko.observableArray([]);

    self.submitQuery = function () {
        self.fetch();
    };

    self.fetch = function () {
        return $.getJSON("/suites/" + self.suiteId + "/test-explorer", { query: self.query() })
            .success(self.handleResponse)
            .fail(self.handleError);
    };

    self.handleResponse = function (data) {
        self.unitTestCollections(data.unitTestCollections);
    };

    self.handleError = function (xhr) {
        self.error(xhr.responseJSON.error);
    };

    self.formatTestTags = function (testTags) {
        return testTags.join(", ");
    };

    self.showTreeMap = function () {
        if (self.tags().length) {
            self.handleTagVisualization();
        } else {
            $.getJSON("/suites/" + self.suiteId + "/test-tree-map", function (data) {
                self.tags(data.tags);
                self.handleTagVisualization();
            });
        }
    };

    self.handleTagVisualization = function () {
        $("#test-tags-tree-map-modal").modal("show");
    }

    self.fetch();
}

ko.bindingHandlers.visualizeTag = {
    init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        // This will be called when the binding is first applied to an element
        // Set up any initial state, event handlers, etc. here
        
    },
    update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
        var obs = valueAccessor();
        $(element).html("");
        $("<div class='bg-success-strong'>")
            .css({
                "height": "18px",
                "width": obs.percentage + "%"
            }).appendTo(element);
    }
};