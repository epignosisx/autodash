function TestExplorer(suiteId, selectedTestsVm) {
    var self = this;
    self.selectedTestsVm = selectedTestsVm;
    self.suiteId = suiteId;
    self.unitTestCollections = ko.observableArray([]);
    self.query = ko.observable();
    self.error = ko.observable();
    self.tags = ko.observableArray([]);

    self.totalTests = ko.computed(function () {
        var index = 0;
        ko.utils.arrayForEach(self.unitTestCollections(), function(coll) {
            ko.utils.arrayForEach(coll.tests, function () {
                index++;
            });
        });
        return index;
    });

    self.addTest = function(testName) {
        self.selectedTestsVm.addTest(testName);
    };

    self.removeTest = function(testName) {
        self.selectedTestsVm.removeTest(testName);
    }

    self.containsTest = function(testName) {
        return self.selectedTestsVm.containsTest(testName);
    }

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

function SelectedTests(suiteId) {
    var self = this;
    self.suiteId = suiteId;
    self.tests = ko.observableArray([]);

    self.addTest = function (testName) {
        if (!self.containsTest(testName)) {
            self.tests.push(testName);
            self.update();
        }
    }

    self.containsTest = function(testName){
        var arr = self.tests(), temp;
        for (var i = 0; i < arr.length; i++) {
            temp = arr[i];
            if (temp === testName) {
                return true;
            }
        }
        return false;
    }

    self.removeTest = function(testName) {
        if (self.containsTest(testName)) {
            self.tests.remove(testName);
        }
    }

    self.update = function() {
        $.ajax({
            url: "/suites/tests/update",
            type: "POST",
            data: JSON.stringify({ id: suiteId, tests: self.tests() })
        });
    }

    self.fetch = function() {
        $.getJSON("/suites/" + self.suiteId + "/tests", function(response) {
            self.tests(response.Data.Tests);
        });
    };

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