using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodash.Core.UI
{
    public class RazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "Autodash.Core";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Autodash.Core";
            yield return "Autodash.Core.UI";
            yield return "Autodash.Core.UI.Modules";
            yield return "Autodash.Core.UI.Models";
        }

        public bool AutoIncludeModelNamespace
        {
            get { return true; }
        }
    }
}
