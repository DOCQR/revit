using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace DOCQR.Revit
{
    public class SheetInfo
    {
        public ElementId sheetId;
        public List<ViewPortInfo> ViewPorts;
        public ViewSheet sheet;
        
        public SheetInfo(Document doc, ViewSheet TempSheet)
        {
            this.ViewPorts = new List<ViewPortInfo>();


            this.sheetId = TempSheet.Id;                     // extract sheet ID
            this.sheet = TempSheet;

            // for each sheet extract each view port
            foreach (ElementId vid in TempSheet.GetAllViewports())
            {
                Viewport vport = (Viewport)doc.GetElement(vid);
                View v = (View)doc.GetElement(vport.ViewId);

                if (v.ViewType == ViewType.AreaPlan || v.ViewType == ViewType.EngineeringPlan || v.ViewType == ViewType.Elevation || v.ViewType == ViewType.FloorPlan || v.ViewType == ViewType.Section || v.ViewType == ViewType.ThreeD)
                {
 

                    ViewPorts.Add(new ViewPortInfo(v.Id, vport.GetBoxOutline().MinimumPoint, v, vport));
                }
            }

        }
    }

    public class ViewPortInfo
    {
        public ElementId id;
 
        public XYZ location;
        public View view;
        public Viewport vport;
        public string docQRid;              // this will hold the returned value from the web server

        public ViewPortInfo(ElementId id, XYZ location, View v, Viewport vp)
        {
            this.id = id;
     
            this.location = location;
            this.view = v;
            this.vport = vp;
        }
    }
}
