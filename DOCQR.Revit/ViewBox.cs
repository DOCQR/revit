using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace DOCQR.Revit
{
    public class ViewBox
    {
        #region Properties
        public int Id { get; set; }
        public string Name { get; set; }
        public XYZ P1 { get; set; }
        public XYZ P2 { get; set; }
        public Transform TransformationMatrix { get; set; }
        public XYZ EyePoint { get; set; }
        public XYZ EyeVector { get; set; }
        #endregion
    }
}
