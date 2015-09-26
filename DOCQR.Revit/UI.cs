using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;
using System.Xml;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;

using System.Windows.Media.Imaging;


namespace DOCQR.Revit
{
    /// <summary>
    /// Create Grevit UI
    /// </summary>    
    class UI : IExternalApplication
    {
        /// <summary>
        /// Assembly path
        /// </summary>
        static string path = typeof(UI).Assembly.Location;

        /// <summary>
        /// Create UI on StartUp
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnStartup(UIControlledApplication application)
        {

            RibbonPanel grevitPanel = application.CreateRibbonPanel("DOCQR");

            PushButton commandButton = grevitPanel.AddItem(new PushButtonData("DOCQR", "DOCQR", path, "DOCQR.Revit.Upload")) as PushButton;
            commandButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.processor.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(32, 32));

            commandButton.SetContextualHelp(new ContextualHelp(ContextualHelpType.Url, "http://github.com/docqr"));

            

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

    }
}