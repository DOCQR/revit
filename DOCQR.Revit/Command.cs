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
    public class Upload : IExternalCommand
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

                //if (doc.ActiveView.ViewType == ViewType.DrawingSheet) createQRCode(doc, (ViewSheet)doc.ActiveView);

                trans.Commit();
                trans.Dispose();
            }
            catch (System.Exception ex)
            {
                return Result.Failed;
            }

            return Result.Succeeded;

        }

    }

}