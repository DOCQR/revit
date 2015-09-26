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
    [Transaction(TransactionMode.Manual)]
    public class QRcode : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            try
            {
                Transaction trans = new Transaction(doc, "QR");
                trans.Start();

                if (doc.ActiveView.ViewType == ViewType.DrawingSheet) createQRCode(doc, (ViewSheet)doc.ActiveView);

                trans.Commit();
                trans.Dispose();
            }
            catch (System.Exception ex)
            {
                return Result.Failed;
            }

            return Result.Succeeded;

        }




        public void createQRCode(Document doc, ViewSheet v)
        {

            FilteredElementCollector col = new FilteredElementCollector(doc);
            col.OfClass(typeof(Autodesk.Revit.DB.FilledRegionType));

            FillPatternElement elem = FillPatternElement.GetFillPatternElementByName(doc, FillPatternTarget.Drafting, "Solid fill");
            FilledRegionType frt = col.FirstElement() as FilledRegionType;

            frt.Background = FilledRegionBackground.Opaque;
            frt.FillPatternId = elem.Id;

            string data = "http://coredev.thorntontomasetti.com/" + v.SheetNumber;

            QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();
            QRCoder.QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(data, QRCoder.QRCodeGenerator.ECCLevel.M);

            double step = 1;


            if (qrCode.ModuleMatrix != null)
            {
                FilteredElementCollector grpsrc = new FilteredElementCollector(doc);
                grpsrc.OfClass(typeof(Autodesk.Revit.DB.Group));
                foreach (Autodesk.Revit.DB.Group grp in grpsrc.ToElements())
                {
                    if (grp.GroupType.Name == "QRTag-" + v.SheetNumber) doc.Delete(grp.Id);
                }


                List<ElementId> ems = new List<ElementId>();




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

                            FilledRegion fr = FilledRegion.Create(doc, frt.Id, v.Id, list);
                            ems.Add(fr.Id);

                        }

                    }

                }


                Group grpn = doc.Create.NewGroup(ems);
                grpn.GroupType.Name = "QRTag-" + v.SheetNumber;
            }



            doc.Regenerate();


        }



    }

}