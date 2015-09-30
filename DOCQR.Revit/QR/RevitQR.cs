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
        private string URL;

        private string data;

        private string groupname;

        public RevitQR(Document doc, SheetInfo info, string url)
        {
            // Erase all QRTAG Images from the Sheet
            FilteredElementCollector fec = new FilteredElementCollector(doc).OwnedByView(info.sheetId).OfCategory(BuiltInCategory.OST_RasterImages);
            foreach (Element element in fec.ToElements()) { if (element.Name.EndsWith("QRTAG")) doc.Delete(element.Id); }

            this.URL = url;

            foreach (ViewPortInfo vport in info.ViewPorts)
            {
                // Assemble URL
                data = String.Format("{0}/viewer/{1}", new object[] { URL, vport.docQRid });

                // Create an image Tag
                ImageTag(doc, info, vport);
            }
        }

        public void ImageTag(Document doc, SheetInfo info, ViewPortInfo vport)
        {
            // Create a new QRCoder Tag
            QRCoder.QRCodeGenerator qrGenerator = new QRCoder.QRCodeGenerator();

            // Generate a new QR Tag
            QRCoder.QRCodeGenerator.QRCode qrCode = qrGenerator.CreateQrCode(data, QRCoder.QRCodeGenerator.ECCLevel.M);

            // Assemble a temp file name
            string tempfile = System.IO.Path.GetTempFileName() + ".QRTAG";

            // Get the Tags graphics element
            System.Drawing.Bitmap bmp = qrCode.GetGraphic(5);

            // Save it temporarily
            bmp.Save(tempfile);

            // Get the related view sheet
            View sheet = (View)doc.GetElement(info.sheetId);

            // Create a image element to return from import
            Element imageElement = null;

            // Get the Viewports label outline
            Outline labelOutline = vport.vport.GetLabelOutline();

            // Get the lower left corner
            XYZ labelCorner = new XYZ(info.sheet.Origin.X + (labelOutline.MinimumPoint.X), info.sheet.Origin.Y + labelOutline.MinimumPoint.Y, 0);

            // Add some distance value
            XYZ location = labelCorner.Add(new XYZ(0, -0.5/12.0, 0));

            // Import the image
            doc.Import(tempfile, new ImageImportOptions() { RefPoint = location, Placement = BoxPlacement.TopLeft }, sheet, out imageElement);
            
            // Delete temp file
            if (System.IO.File.Exists(tempfile)) System.IO.File.Delete(tempfile);
        }


    }
}