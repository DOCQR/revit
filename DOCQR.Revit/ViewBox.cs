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
        public BoundingBoxXYZ bbox { get; set; }
        public XYZ DirectionUp { get; set; }
        public XYZ EyePosition { get; set; }
        public XYZ DirectionView { get; set; }
        #endregion
    }
}
