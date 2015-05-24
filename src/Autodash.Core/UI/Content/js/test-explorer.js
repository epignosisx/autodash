var TestExplorer = (function () {
    function TestExplorer($filterBox, $tables) {
        this.$filterBox = $filterBox;
        this.$tables = $tables;
        this.data = [];
        this.init();
    }
    TestExplorer.prototype.init = function () {
        this.processTables();
        this.$filterBox.on("change", $.proxy(this.updateTable, this));
    };
    TestExplorer.prototype.processTables = function(){
        var i = 0, l = this.$tables.length;
        for(; i < l; i++){
            this.processTable(this.$tables[i]);
        }
    };
    TestExplorer.prototype.processTable = function (table) {
        var i = 0, l = table.rows.length, $row;

        for (; i < l; i++) {
            $row = $(table.rows[i]);
            var cells = $row.children();
            var text = $(cells[0]).text() + " " + $(cells[1]).text();
            this.data.push({
                text: text.toLowerCase(),
                row: $row
            });
        }
    };
    TestExplorer.prototype.updateTable = function () {
        var value = this.$filterBox.val(),
            valueNormalized = value.toLowerCase(),
            i = 0, l = this.data.length;

        for (; i < l; i++) {
            var row = this.data[i];
            if (row.text.indexOf(valueNormalized) >= 0) {
                row.row.removeClass("hidden");
            }
            else {
                row.row.addClass("hidden");
            }
        }
    };
    return TestExplorer;
})();

if (jQuery) {
    var pluginName = "testExplorer";
    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, "plugin_" + pluginName)) {
                $.data(this, "plugin_" + pluginName,
                new TestExplorer($(this), options.tables));
            }
        });
    };
}