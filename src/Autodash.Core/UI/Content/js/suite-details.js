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
            var last10runs = data.SuiteRuns.slice(0, 10);
            drawChart(mapRunsForLast10Runs(last10runs));
        });
    }

    function mapRunsForLast10Runs(suiteRuns){
        var i = 0, l = suiteRuns, run = null, data = [];

        for (; i < l; i++) {
            run = suiteRuns[i];
            data.push([
                i + 1, { v: run.DurationMinutes, f: run.DurationMinutes + " mins"}, "color:" + (run.Result.Passed ? "green" : "red")
            ]);
        }
    }

    function drawChart(runs) {

        var data = new google.visualization.DataTable();
        data.addColumn('number', 'Run #');
        data.addColumn('number', 'Took');
        data.addColumn({ type: 'string', role: 'style' });

        data.addRows([
            [1, { v: 16, f: "16 mins" }, "color:green"],
            [2, { v: 15, f: "15 mins" }, "color:red"],
            [3, { v: 17, f: "17 mins" }, "color:red"],
            [4, { v: 20, f: "20 mins" }, "color:green"],
            [5, { v: 18, f: "18 mins" }, "color:green"],
            [6, { v: 19, f: "19 mins" }, "color:red"],
            [7, { v: 25, f: "25 mins" }, "color:green"],
            [8, { v: 35, f: "35 mins" }, "color:green"],
            [9, { v: 19, f: "19 mins" }, "color:red"],
            [10, { v: 20, f: "20 mins" }, "color:green"]
        ]);

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
                title: 'Time (mins)'
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
    google.setOnLoadCallback(drawChart);
});