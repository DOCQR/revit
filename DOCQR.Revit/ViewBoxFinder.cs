﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace DOCQR.Revit
{
    public static class ViewBoxFinder
    {
        #region Declarations
        #endregion

        #region Properties

        #endregion

        #region Constructor
        
        #endregion

        #region PublicMethods
        public static IList<ViewBox> GetViewBoxes(ViewSheet vs)
        {
            ISet<ElementId> views = vs.GetAllPlacedViews();
            List<ViewBox> boxes = new List<ViewBox>();

            foreach (ElementId id in views)
            {
                View v = vs.Document.GetElement(id) as View;
                switch (v.ViewType)
                {
                    case ViewType.AreaPlan:
                    case ViewType.CeilingPlan:
                    case ViewType.Elevation:
                    case ViewType.EngineeringPlan:
                    case ViewType.FloorPlan:
                    case ViewType.Section:
                    case ViewType.ThreeD:
                        ViewBox box = GetViewBox(v);
                        if (box != null) boxes.Add(box);
                        break;
                    default:
                        // skip
                        break;
                }
                
            }
            return boxes;
        }
        public static ViewBox GetViewBox(View v)
        {
            if (v is ViewPlan)
            {
                return getPlanViewBox(v as ViewPlan);
            }
            if (v is ViewSection)
            {
                return getSectionViewBox(v as ViewSection);
            }
            return null;
        }
        #endregion

        #region PRivateMethods
        private static ViewBox getPlanViewBox(ViewPlan vp)
        {
            System.Diagnostics.Debug.WriteLine("ViewPlan: " + vp.Id + " Outline: " + vp.Outline.Min.U + "," + vp.Outline.Min.V + " to " + vp.Outline.Max.U + "," + vp.Outline.Max.V);
            System.Diagnostics.Debug.WriteLine("ViewPlan Scale: " + vp.Scale);

            
            XYZ tmp1 = vp.Origin.Add(new XYZ(vp.Outline.Min.U * (double)vp.Scale, vp.Outline.Min.V * (double)vp.Scale, 0));
            XYZ tmp2 = vp.Origin.Add(new XYZ(vp.Outline.Max.U * (double)vp.Scale, vp.Outline.Max.V * (double)vp.Scale, 0));

            // double check the cropbox, if it is smaller...
            if (vp.CropBoxActive)
            {
                tmp1 = vp.CropBox.Min;
                tmp2 = vp.CropBox.Max;
            }

            // in a plan view, we will work with the cropbox (if it is set?)
            ViewBox box = new ViewBox() { P1 = tmp1, P2 = tmp2 };

            // now reset by the level and the viewdepth
            if (vp.GenLevel != null)
            {
                PlanViewRange pvr = vp.GetViewRange();
                ElementId topId = pvr.GetLevelId(PlanViewPlane.TopClipPlane);
                ElementId bottomId = pvr.GetLevelId(PlanViewPlane.ViewDepthPlane);
                if (topId != ElementId.InvalidElementId)
                {
                    Level top = vp.Document.GetElement(topId) as Level;
                    box.P2 = new XYZ(box.P2.X, box.P2.Y, top.Elevation + pvr.GetOffset( PlanViewPlane.TopClipPlane ) );

                }
                if (bottomId != ElementId.InvalidElementId)
                {
                    Level bottom = vp.Document.GetElement(bottomId) as Level;
                    box.P1 = new XYZ(box.P1.X, box.P1.Y, bottom.Elevation + pvr.GetOffset(PlanViewPlane.ViewDepthPlane));
                }
            }

            // set the transform
            box.TransformationMatrix = Transform.Identity;

            return box;

        }

        private static ViewBox getSectionViewBox(ViewSection vs)
        {
            ViewBox box = new ViewBox();

            XYZ tmp1 = vs.Origin;
            XYZ right = XYZ.BasisZ.CrossProduct(vs.ViewDirection);

            box.P1 = vs.CropBox.Transform.OfPoint(vs.CropBox.Min);
            box.P2 = vs.CropBox.Transform.OfPoint(vs.CropBox.Max);

            box.TransformationMatrix = vs.CropBox.Transform;

            return box;
        }
        #endregion
    }
}