using FluentValidation.Results;

namespace Autodash.Core.UI.Models
{
    public class GridConfigVm
    {
        public string HubUrl { get; set; }
        public int MaxParallelTestSuitesRunning { get; set; }
        public string JsonConfig { get; set; }
        public ValidationFailure[] Errors { get; set; }

        public string GridVersion {
            get { return string.IsNullOrEmpty(HubUrl) ? null : HubUrl + "grid"; }
        }

        public string GridConfig
        {
            get { return string.IsNullOrEmpty(HubUrl) ? null : HubUrl + "grid/api/hub"; }
        }

        public string GridConsole
        {
            get { return string.IsNullOrEmpty(HubUrl) ? null : HubUrl + "grid/console"; }
        }

        public GridConfigVm()
        {
            MaxParallelTestSuitesRunning = 1;
            Errors = new ValidationFailure[0];
        }
    }
}