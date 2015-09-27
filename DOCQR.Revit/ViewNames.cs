using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOCQR.Revit
{
    public class ViewName
    {
        public string Name;
        public string ID;
    }

    public class ViewNames {
        public List<ViewName> Views; public ViewNames() { this.Views = new List<ViewName>(); } }
}
