function TestExplorer(suiteId) {
    var self = this;
    self.suiteId = suiteId;
    self.unitTestCollections = ko.observableArray([]);
    self.query = ko.observable();
    self.error = ko.observable();

    self.submitQuery = function () {
        self.fetch();
    };

    self.fetch = function () {
        $.getJSON("/suites/" + self.suiteId + "/test-explorer", { query: self.query() })
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

    self.fetch();
}