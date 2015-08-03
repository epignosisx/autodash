$(function () {
    var suiteId = $("#Id").val();
    var testSelectionVm = new SelectedTests(suiteId);
    var testExplorerVm = new TestExplorer(suiteId, testSelectionVm);

    var vm = {
        testSelection: testSelectionVm,
        testExplorer: testExplorerVm
    };

    ko.applyBindings(vm, document.body);

    $('[data-toggle="popover"]').popover();

    $("#js-test-tag-explorer").on("click", function (e) {
        e.preventDefault();
        $('#content-tabs a[href="#test-explorer"]').tab("show");
    });

    $("#js-run-suite").on("click", function(e) {
        e.preventDefault();
        $.post("/suites/" + suiteId + "/schedule", function (data) {
            reloadSuiteRunHistory(suiteId).done(function () {
                $("#content-tabs a[href='#suite-runs']").tab('show');
                reloadSuiteRunHistoryPeriodically();
            });
        });
    });

    function reloadSuiteRunHistory(suiteId) {
        return $.get("/suites/" + suiteId + "/suite-run-history?format=html", function (response) {
            $("#suite-runs").html(response);
        });
    }

    function reloadSuiteRunHistoryPeriodically() {
        if (reloadSuiteRunHistoryPeriodically.intervalId) {
            return;//already running
        }

        var id = setInterval(function () {
            reloadSuiteRunHistory(suiteId).done(function () {
                if (!shouldReloadSuiteRunHistory()) {
                    clearInterval(id);
                    updateCharts();
                }
            });
        }, 3000);
        reloadSuiteRunHistoryPeriodically.intervalId = id;
    }

    function shouldReloadSuiteRunHistory() {
        var value = $("#is-suite-incomplete").val();
        return value == "True";
    }

    function updateCharts() {
        $.getJSON("/suites/" + suiteId + "/suite-run-history?format=json", function (data) {
            var last10runs = data.suiteRuns.slice(0, 10);
            var runs = mapRunsForLast10Runs(last10runs);
            drawChart(runs);
        });
    }

    function mapRunsForLast10Runs(suiteRuns){
        var i = 0,
            l = suiteRuns.length,
            run = null,
            data = [];

        suiteRuns.reverse();

        for (; i < l && i < 10; i++) {
            run = suiteRuns[i];
            if (run.result != null) {
                data.push([
                    i + 1, { v: run.durationMinutes, f: run.durationMinutes.toFixed(2) + " mins" }, "color:" + (run.result.passed ? "#5cb85c" : "#d9534f")
                ]);
            }
        }

        return data;
    }

    function drawChart(runs) {

        var data = new google.visualization.DataTable();
        data.addColumn('number', 'Run #');
        data.addColumn('number', 'Took');
        data.addColumn({ type: 'string', role: 'style' });

        data.addRows(runs);

        var options = {
            legend: { position: 'none' },
            title: 'Last 10 suite runs',
            hAxis: {
                textPosition: 'none',
                viewWindow: {
                    min: 0.5,
                    max: 10.5
                }
            },
            vAxis: {
                title: 'Time (mins)',
                minValue: 0,
                viewWindow: {
                    min: 0
                }
            }
        };

        var chart = new google.visualization.ColumnChart(
          document.getElementById('last-ten-chart'));

        chart.draw(data, options);
    }

    if (shouldReloadSuiteRunHistory()) {
        reloadSuiteRunHistoryPeriodically();
    }

    // Load the Visualization API and the piechart package.
    google.load('visualization', '1.0', { 'packages': ['corechart'] });

    // Set a callback to run when the Google Visualization API is loaded.
    google.setOnLoadCallback(updateCharts);
});