using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace DOCQR.Revit
{
    public class RevitQR : IExternalCommand
    {

        private static FilledRegionType solidFill;

        private static string URL;

        public RevitQR(Document doc, XYZ placement, Guid ProjectModelId, ElementId ViewportId, ElementId viewSheetId)
        {
            // Get Filled Region
            FilteredElementCollector col = new FilteredElementCollector(doc);
            col.OfClass(typeof(Autodesk.Revit.DB.FilledRegionType));

            if (solidFill == null)
            {
                FillPatternElement elem = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "Solid fill");
                solidFill = col.FirstElement() as FilledRegionType;
                solidFill.Background = FilledRegionBackground.Opaque;
                solidFill.FillPatternId = elem.Id;
            }

            // Assemble URL
            string data = String.Format("{0}/{1}/{2}", new object[] { URL, ProjectModelId.ToString(), ViewportId.IntegerValue.ToString() });

            // Assemble Group 
            string groupname = String.Format("DOCQR-{0}", ViewportId.IntegerValue.ToString());

            // Create a new QRCoder Tag
            QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
            QRCoder.QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(data, QRCoder.QRCodeGenerator.ECCLevel.M);

            // Stepsize for QR Squares
            double step = 1;

            
            if (qrCode.ModuleMatrix != null)
            {
                // Delete existing Group
                FilteredElementCollector grpsrc = new FilteredElementCollector(doc);
                grpsrc.OfClass(typeof(Autodesk.Revit.DB.Group));
                foreach (Autodesk.Revit.DB.Group grp in grpsrc.ToElements())
                {
                    if (grp.GroupType.Name == groupname) doc.Delete(grp.Id);
                }


                List<ElementId> elementsForGrouping = new List<ElementId>();


                var size = qrCode.ModuleMatrix.Count;

                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        var module = qrCode.ModuleMatrix[y][x];
                        if (module)
                        {
                            CurveLoop cl = new CurveLoop();
                            List<CurveLoop> list = new List<CurveLoop>();

                            cl.Append(Line.CreateBound(new XYZ(x * step, y * step, 0.0), new XYZ((x + 1) * step, y * step, 0.0)));
                            cl.Append(Line.CreateBound(new XYZ((x + 1) * step, y * step, 0.0), new XYZ((x + 1) * step, (y + 1) * step, 0.0)));
                            cl.Append(Line.CreateBound(new XYZ((x + 1) * step, (y + 1) * step, 0.0), new XYZ(x * step, (y + 1) * step, 0.0)));
                            cl.Append(Line.CreateBound(new XYZ(x * step, (y + 1) * step, 0.0), new XYZ(x * step, y * step, 0.0)));

                            list.Add(cl);

                            FilledRegion fr = FilledRegion.Create(doc, solidFill.Id, viewSheetId, list);
                            elementsForGrouping.Add(fr.Id);

                        }
                    }
                }


                Group grpn = doc.Create.NewGroup(elementsForGrouping);
                grpn.GroupType.Name = groupname;
            }



            doc.Regenerate();


        }



    }

}