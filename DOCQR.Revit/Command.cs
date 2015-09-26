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


        /// <summary>
        /// Get the project sheets and the views on the sheets
        /// </summary>
        private void GetViews(Document doc)
        {
            FilteredElementCollector col = new FilteredElementCollector(doc);
            List<Element> Elements = new List<Element>();
            Elements.AddRange(col.OfClass(typeof(ViewSheet)).ToElements());


            List<ElementId> SheetIDs = new List<ElementId>();                   // the list of sheet id's
            List<List<ElementId>> ViewPortIDs = new List<List<ElementId>>();    // each sheet might have more the one view on it
            List<List<Guid>> ViewPortGuids = new List<List<Guid>>();
            List<List<XYZ>> ViewPortLocation = new List<List<XYZ>>();


            foreach (Element ele in Elements)
            {
                ViewSheet TempSheet = (ViewSheet)ele;           // convert element to view sheet
                SheetIDs.Add(TempSheet.Id);                     // extract sheet ID

                List<Guid> TempViewPortGuids = new List<Guid>();  // temporary list for view guids
                List<ElementId> TempViewPortIDs = new List<ElementId>();
                List<XYZ> TempViewPortLocation = new List<XYZ>();

                // for each sheet extract each view port
                foreach (View v in TempSheet.Views)
                {
                    Guid guid = new Guid(v.UniqueId);
                    TempViewPortGuids.Add(guid);                    // get the view port guid

                    TempViewPortIDs.Add(v.Id);                      // get the view port id

                    LocationPoint lp = (LocationPoint)v.Location;   // get the view port start point
                    TempViewPortLocation.Add(lp.Point);
                }

                ViewPortGuids.Add(TempViewPortGuids);               // save all the guids
                ViewPortIDs.Add(TempViewPortIDs);                   // save all the ids
                ViewPortLocation.Add(TempViewPortLocation);         // save all the location points
            }
        }       // close function


        // view port id - element ID, guid
        // view port location - xyz
        // sheet id


    }       // close class
}           // close namespace