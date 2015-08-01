using System.Collections.Generic;

namespace Autodash.Core.UI.Models
{
    public class EditSuiteVm : CreateSuiteVm
    {
        public string Id { get; set; }
    }

    public class UpdateSuiteTestsVm
    {
        public string Id { get; set; }
        public List<string> Tests { get; set; }
    }
}