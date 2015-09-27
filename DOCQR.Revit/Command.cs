using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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

        private UIApplication _uiApp;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uiApp = commandData.Application;
            Document doc = _uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = _uiApp.ActiveUIDocument;

            // take care of AppDomain load issues
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            DOCQRclient client = new DOCQRclient(" web url ");
            LogInFrm loginForm = new LogInFrm(client);

            if (loginForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ProjectSelectFrm ProjectSelectFrm = new ProjectSelectFrm(client);
                if (ProjectSelectFrm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                }
            }

            //try

            // {
            Transaction trans = new Transaction(doc, "QR");
            trans.Start();

            GetSheetViewInfo(doc);

            trans.Commit();
            trans.Dispose();
            //}
            //catch (System.Exception ex)
            //{
            //    return Result.Failed;
            //}

            //doc.Regenerate();

            return Result.Succeeded;

        }


        System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // if the request is coming from us
            if ((args.RequestingAssembly != null) && (args.RequestingAssembly == this.GetType().Assembly))
            {
                if ((args.Name != null) && (args.Name.Contains(",")))  // ignore resources and such
                {
                    string asmName = args.Name.Split(',')[0];

                    string targetFilename = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, asmName + ".dll");

                    _uiApp.Application.WriteJournalComment("Assembly Resolve issue. Looking for: " + args.Name, false);
                    _uiApp.Application.WriteJournalComment("Looking for " + targetFilename, false);

                    if (File.Exists(targetFilename))
                    {
                        _uiApp.Application.WriteJournalComment("Found, and loading...", false);
                        return System.Reflection.Assembly.LoadFrom(targetFilename);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the project sheets and the views on the sheets
        /// </summary>
        private void GetSheetViewInfo(Document doc)
        {
            FilteredElementCollector col = new FilteredElementCollector(doc);
            List<Element> Elements = new List<Element>();
            Elements.AddRange(col.OfClass(typeof(ViewSheet)).ToElements());


            List<SheetInfo> Sheets = new List<SheetInfo>();                   // the list of sheet id's



            foreach (Element ele in Elements)
            {
                ViewSheet TempSheet = (ViewSheet)ele;           // convert element to view sheet
                SheetInfo info = new SheetInfo(doc, TempSheet);
                RevitQR QR = new RevitQR(doc, info);
                Sheets.Add(info);
            }
        }

    }
}
