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

        private int _viewCount = 0;
        private UIApplication _uiApp;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _uiApp = commandData.Application;
            Document doc = _uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = _uiApp.ActiveUIDocument;

            // take care of AppDomain load issues
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Transaction trans = null;
            try
            {

                if (uidoc.ActiveGraphicalView.ViewType != ViewType.ThreeD) throw new ApplicationException("Please run this command from a 3D View!");

                string WebURL = "http://128.8.215.91";
                DOCQRclient client = new DOCQRclient(WebURL);
                client.IsDummy = false;
                LogInFrm loginForm = new LogInFrm(client);



                if (loginForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ProjectSelectFrm ProjectSelectFrm = new ProjectSelectFrm(client);
                    if (ProjectSelectFrm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        List<SheetInfo> sheets = GetSheetViewInfo(doc);
                        List<ElementId> ViewsToDelete = new List<ElementId>();
                        ViewNames names = new ViewNames();
                        client.GetModelID(ProjectSelectFrm.SelectedProject.id);

                        ProgressForm progress = new ProgressForm(_viewCount * 3);
                        progress.Show();

                        trans = new Transaction(doc, "QR");
                        trans.Start();

                        // go through all the sheets and then views
                        // make a 3d view 
                        // save each 3d view json file
                        foreach (SheetInfo sheet in sheets)
                        {
                            foreach (ViewPortInfo vpInfo in sheet.ViewPorts)
                            {
                                Spectacles.RevitExporter.Command cmd = new Spectacles.RevitExporter.Command();
                                string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), doc.Title + vpInfo.view.Id + ".json");

                                if (progress.IsCancelled) throw new ApplicationException("DOCQR Cancelled...");
                                progress.SetStatus("Exporting View: " + vpInfo.view.Name + " to Spectacles...");
                                System.Diagnostics.Debug.WriteLine("Exporting View " + vpInfo.view.Name + " to Spectacles...");
                                View3D temp3dView = vpInfo.view.GetMatching3DView(doc);
                                cmd.ExportEntireModel(temp3dView, tempFile);
                                ViewsToDelete.Add(temp3dView.Id);

                                if (progress.IsCancelled) throw new ApplicationException("DOCQR Cancelled...");
                                progress.Step();
                                progress.SetStatus("Sending View: " + vpInfo.view.Name + " to DOCQR...");
                                vpInfo.docQRid = client.SendModelInfo(ProjectSelectFrm.SelectedProject, tempFile);            // send the model and view info to the web server
                                names.Views.Add(new ViewName() { Name = vpInfo.view.Name, ID = vpInfo.docQRid });
                                progress.Step();
                            }
                        }

                        //client.SendViewInfo(names);

                        progress.SetStatus("Adding QR Codes...");
                        foreach (SheetInfo sheet in sheets)
                        {
                            if (progress.IsCancelled) throw new ApplicationException("DOCQR Cancelled...");
                            progress.Step();
                            RevitQR QR = new RevitQR(doc, sheet, true, WebURL);                                                   // create QR codes
                        }

                        progress.Close();

                        doc.Delete(ViewsToDelete);

                        trans.Commit();
                        trans.Dispose();

                        TaskDialog.Show("DOCQR", "QR Codes added to " + _viewCount + " viewports!");
                    }
                }
            }
            catch (ApplicationException aex)
            {
                TaskDialog.Show("DOCQR", aex.Message);
                if ((trans != null) && (trans.HasStarted())) trans.RollBack();
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Unexpected Error", ex.GetType().Name + ": " + ex.Message);
                if ((trans != null) && (trans.HasStarted())) trans.RollBack();
                return Result.Failed;
            }

            //try

            // {
           
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
        /// <param name="doc"></param>
        /// <returns>return the list of sheet information</returns>
        private List<SheetInfo> GetSheetViewInfo(Document doc)
        {
            FilteredElementCollector col = new FilteredElementCollector(doc);
            List<Element> Elements = new List<Element>();
            Elements.AddRange(col.OfClass(typeof(ViewSheet)).ToElements());

            List<SheetInfo> Sheets = new List<SheetInfo>();                   // the list of sheet id's

            foreach (Element ele in Elements)
            {
                ViewSheet TempSheet = (ViewSheet)ele;           // convert element to view sheet
                SheetInfo info = new SheetInfo(doc, TempSheet);
                if (info.ViewPorts.Count > 0)
                {
                    Sheets.Add(info);
                    _viewCount += info.ViewPorts.Count;
                }
            }

            return Sheets;
        }

    }
}
