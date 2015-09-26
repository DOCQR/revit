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

        private string URL;

        private string data;

        private string groupname;

        public RevitQR(Document doc, SheetInfo info, bool asImage, string url)
        {
            if (asImage)
            {
                FilteredElementCollector fec = new FilteredElementCollector(doc).OwnedByView(info.sheetId).OfCategory(BuiltInCategory.OST_RasterImages);
                foreach (Element element in fec.ToElements()) { if (element.Name.EndsWith("QRTAG")) doc.Delete(element.Id);}
            }


            this.URL = url;

            foreach (ViewPortInfo vport in info.ViewPorts)
            {
                // Assemble URL
                data = String.Format("{0}/{1}/{2}", new object[] { URL, vport.guid.ToString(), vport.id.IntegerValue.ToString() });

                // Assemble Group 
                groupname = String.Format("DOCQR-{0}", vport.id.IntegerValue.ToString());


                if (!asImage)
                    HatchTag(doc, info, vport);
                else
                    ImageTag(doc, info, vport);
            }
        }

        public void ImageTag(Document doc, SheetInfo info, ViewPortInfo vport)
        {

            // Create a new QRCoder Tag
            QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
            QRCoder.QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(data, QRCoder.QRCodeGenerator.ECCLevel.M);
            string tempfile = System.IO.Path.GetTempFileName() + ".QRTAG";
            qrCode.GetGraphic(5).Save(tempfile);
            View sheet = (View)doc.GetElement(info.sheetId);
            Element imageElement = null;
            doc.Import(tempfile, new ImageImportOptions() { RefPoint = vport.location }, sheet, out imageElement);
            string n = imageElement.GetType().ToString();

        }


        public void HatchTag(Document doc, SheetInfo info, ViewPortInfo vport)
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

                // Create a new QRCoder Tag
                QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
                QRCoder.QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(data, QRCoder.QRCodeGenerator.ECCLevel.M);

                qrCode.GetGraphic(1).Save(System.IO.Path.GetTempFileName());


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
                    double locationX = vport.location.X;
                    double locationY = vport.location.Y;

                    // Stepsize for QR Squares
                    double step = 0.01;


                    for (int x = 0; x < size; x++)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            var module = qrCode.ModuleMatrix[y][x];
                            if (module)
                            {
                                CurveLoop curveLoop = new CurveLoop();
                                List<CurveLoop> list = new List<CurveLoop>();

                                XYZ A = new XYZ(locationX + (x * step), locationY - (y * step), 0);
                                XYZ B = new XYZ(locationX + ((x + 1) * step), locationY - (y * step), 0);
                                XYZ C = new XYZ(locationX + ((x + 1) * step), locationY - ((y - 1) * step), 0);
                                XYZ D = new XYZ(locationX + (x * step), locationY - ((y - 1) * step), 0);
                                
                                curveLoop.Append(Line.CreateBound(A, B));
                                curveLoop.Append(Line.CreateBound(B, C));
                                curveLoop.Append(Line.CreateBound(C, D));
                                curveLoop.Append(Line.CreateBound(D, A));

                                list.Add(curveLoop);

                                FilledRegion fr = FilledRegion.Create(doc, solidFill.Id, info.sheetId, list);
                                elementsForGrouping.Add(fr.Id);

                            }
                        }
                    }


                    Group grpn = doc.Create.NewGroup(elementsForGrouping);
                    grpn.GroupType.Name = groupname;
                }



                


            }
        }



    

}