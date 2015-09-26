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
                foreach (Element element in fec.ToElements()) { if (element.Name.EndsWith("QRTAG")) doc.Delete(element.Id); }
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

        }






    }
}