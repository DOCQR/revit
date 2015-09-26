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
    public class RevitQR
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

                // XYZ Location
                double locationX = placement.X;
                double locationY = placement.Y;

                // Stepsize for QR Squares
                double step = 1;


                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        var module = qrCode.ModuleMatrix[y][x];
                        if (module)
                        {
                            CurveLoop curveLoop = new CurveLoop();
                            List<CurveLoop> list = new List<CurveLoop>();

                            XYZ A = new XYZ(locationX + (x * step), locationY + (y * step), 0);
                            XYZ B = new XYZ(locationX + ((x + 1) * step), locationY + (y * step), 0);
                            XYZ C = new XYZ(locationX + ((x + 1) * step), locationY + ((y + 1) * step), 0);
                            XYZ D = new XYZ(locationX + (x * step), locationY + ((y + 1) * step), 0);

                            curveLoop.Append(Line.CreateBound(A, B));
                            curveLoop.Append(Line.CreateBound(B, C));
                            curveLoop.Append(Line.CreateBound(C, D));
                            curveLoop.Append(Line.CreateBound(D, A));

                            list.Add(curveLoop);

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