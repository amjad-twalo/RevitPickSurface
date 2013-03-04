using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit;
using System.Windows.Forms;
using Autodesk.Revit.Geometry;
using Autodesk.Revit.Elements;

namespace testPickOne
{
    public class testPickOne : IExternalCommand 
    {

        #region IExternalCommand Members

        public IExternalCommand.Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.Application app = commandData.Application;
            Document document = app.ActiveDocument;
            List<Line> loop = getLoop(document);
            

            return IExternalCommand.Result.Succeeded;
        }

        #endregion

        private List<Line> getLoop(Document document)
        {
            Autodesk.Revit.Application app = document.Application;
            List<XYZ> points = new List<XYZ>();
            List<Line> result = new List<Line>();
            bool test = true;
            bool first = true;
            bool second = false;

            Autodesk.Revit.Element prevElem = new Autodesk.Revit.Element();
            while (test)
            {
                document.Selection.PickOne();

                ElementSet elems = document.Selection.Elements;
                List<ModelLine> selection = new List<ModelLine>();
                foreach (Autodesk.Revit.Element elem in elems)
                {
                    ModelLine t = elem as ModelLine;
                    if (elem.Equals(prevElem))
                    {
                        MessageBox.Show("Don't Choose same Line Twice");
                        continue;
                    }
                    else
                        prevElem = elem;
                    selection.Add(t);
                }
                document.Selection.Elements.Clear();
                if (selection.Count == 1)
                {
                    Curve curve = selection[0].GeometryCurve;

                    Line line = curve as Line;
                   
                    XYZ start = line.get_EndPoint(0);
                    XYZ end = line.get_EndPoint(1);
                    if (second)
                    {
                        if(start.AlmostEqual(points[1]))
                            points.Add(end);
                        else if(end.AlmostEqual(points[1]))
                            points.Add(start);
                        else if(start.AlmostEqual(points[0]))
                        {
                            XYZ temp = points[0];
                            points[0] = points[1];
                            points[1] = temp;

                            points.Add(end);
                        }
                        else if (end.AlmostEqual(points[0]))
                        {
                            XYZ temp = points[0];
                            points[0] = points[1];
                            points[1] = temp;

                            points.Add(start);
                        }
                        else
                        {
                            throw new Exception("Pick Consecutive Lines");
                        }
                        second = false;
                    }
                    else if (first)
                    {
                        points.Add(line.get_EndPoint(0));
                        points.Add(line.get_EndPoint(1));
                        first = false;
                        second = true;
                    }
                    else
                    {
                        if (start.AlmostEqual(points[points.Count - 1]))
                        {
                            points.Add(end);
                            if (end.AlmostEqual(points[0]))
                            {
                                break;
                            }
                        }
                        else if (end.AlmostEqual(points[points.Count - 1]))
                        {
                            points.Add(end);
                            if (start.AlmostEqual(points[0]))
                            {
                                break;
                            } 
                        }
                        else
                        {
                            throw new Exception("Please Choose Consecutive Lines");
                        }
                    }
                }
                else
                {
                    throw new Exception("");
                }

            }

            XYZ firstPoint = points[0];
            XYZ secondPoint = new XYZ();
            for (int i = 1; i < points.Count; i++)
            {
                secondPoint = points[i];
                Line l = Line.get_Bound(firstPoint, secondPoint);
                result.Add(l);
                firstPoint = secondPoint;
            }
            return result;
        }


    }
}
