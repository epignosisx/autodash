using System.Collections;

namespace Autodash.Core.UI.Models
{
    public class SuiteRunDetailsVm
    {
        public SuiteRun SuiteRun { get; set; }
        public Project Project { get; set; }
        public bool DownloadMode { get; set; }
        public bool EmbedResources { get; set; }
    }
}