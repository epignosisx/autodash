var global = (function () {

    function outcomeToBgColor(outcome) {
        if (outcome == 1) {
            return "#5cb85c";
        } else if (outcome == 2) {
            return "#f0ad4e";
        }
        return "#d9534f";
    }

    return {
        outcomeToBgColor: outcomeToBgColor
    };
})();