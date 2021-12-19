using netDxf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using netDxf.Entities;
using Point = System.Windows.Point;
using GeoExport;


namespace GeometryToDxfExport
{
    public class GeometryDxfExporter : GeoExport.GeometryExporter
    {
        #region internv variables
        int EllipsCt, FigureCt, GeoCt, LineCt;
        DxfDocument doc = new DxfDocument();
        List<netDxf.Entities.EntityObject> dxfEntities = new List<EntityObject>();

        #endregion

        #region Properties
        //public int PathFigureIterationSteps = 1000;
        #endregion

        #region Konstruktor
        public GeometryDxfExporter() : this(1000) { }

        public GeometryDxfExporter(int PathFigureIterationSteps)
        {
            this.PathFigureIterationSteps = PathFigureIterationSteps;
        }
        #endregion
        public void ExportDxf(string file, params Geometry[] geos)
        {
            dxfEntities = new List<EntityObject>();
            foreach (Geometry g in geos)
            {
                addGeoEntitie(g);
            }
            DxfDocument doc = new DxfDocument();
            doc.AddEntity(dxfEntities);
            string? Extension = System.IO.Path.GetExtension(file);
            if (Extension != ".dxf") file = $"{file}.dxf";
            doc.Save(file);
        }

        void addGeoEntitie(Geometry geo)
        {

            if (geo is GeometryGroup)
            {
                GeometryGroup group = geo as GeometryGroup;
                foreach (Geometry g in group.Children)
                {
                    addGeoEntitie(g);
                }
                return;
            }
            if (geo is EllipseGeometry)
            {
                EllipseGeometry ell = geo as EllipseGeometry;
                netDxf.Entities.Ellipse ellipse = ToDxfEllipse(ell);
                ellipse.Layer = new netDxf.Tables.Layer($"ellips{EllipsCt}");
                EllipsCt++;
                dxfEntities.Add(ellipse);
                return;
            }
            if (geo is LineGeometry)
            {
                LineGeometry geoLine = geo as LineGeometry;
                System.Windows.Point P1 = geoLine.StartPoint;
                System.Windows.Point P2 = geoLine.EndPoint;

                netDxf.Entities.Line line = new netDxf.Entities.Line(DxfExtension.ToVector2(P1), DxfExtension.ToVector2(P2));
                line.Layer = new netDxf.Tables.Layer($"line{LineCt}");
                LineCt++;
                dxfEntities.Add(line);
            }
            if (geo is PathGeometry)
            {

                PathGeometry path = geo as PathGeometry;

                foreach (PathFigure figure in path.Figures)
                {

                    //addSegmentEntities(figure);
                    List<Point> pst = new List<Point>();
                    pst = getFigurePoints(figure);
                    //if (minArea > 0)
                    //{
                    //    VisvalingamWyatt wyatt = new VisvalingamWyatt();
                    //    pst = wyatt.Simplify(pst, true, minArea);

                    //}
                    netDxf.Entities.Polyline poly = PointsToPolyLine(pst);
                    poly.Layer = new netDxf.Tables.Layer($"figure{FigureCt}");
                    poly.IsClosed = true;
                    FigureCt++;
                    dxfEntities.Add(poly);
                }
                return;
            }
            else
            {
                PathGeometry path = PathGeometry.CreateFromGeometry(geo);
                addGeoEntitie(path);
            }

        }

        //trys to iterate over PathSegment, if it only consist of lines => it collects all EdgePoints and convert it to DXF-PolyLine
        //public List<Point> getFigurePoints(PathFigure fig, out bool succesful)
        //{

        //    List<Point> points = new List<Point>();
        //    succesful = false;
        //    points.Add(fig.StartPoint);
        //    foreach (PathSegment segm in fig.Segments)
        //    {
        //        if (segm is LineSegment)
        //        {
        //            LineSegment line = segm as LineSegment;
        //            points.Add(line.Point);
        //            continue;
        //        }
        //        if (segm is PolyLineSegment)
        //        {
        //            PolyLineSegment polyLine = segm as PolyLineSegment;
        //            PointCollection ptc = polyLine.Points;
        //            for (int i = 0; i < ptc.Count; i++)
        //            {
        //                points.Add(ptc[i]);
        //            }
        //            continue;
        //        }
        //        succesful = false;
        //        return new List<Point>();
        //        //other types are ArcSegment,PolyBezierSegment and QuadraticBezierSegment
        //    }
        //    return points;
        //}
        //public List<Point> getFigurePoints(PathFigure fig, int Steps = 1000)
        //{

        //    if (fig == null) return new List<Point>();
        //    List<Point> list = new List<Point>();
        //    PathGeometry geo = new PathGeometry();
        //    geo.Figures.Add(fig);
        //    geo = geo.GetFlattenedPathGeometry();
        //    geo.GetPointAtFractionLength(0, out Point P_Start, out Point tangent);
        //    Point P1;


        //    geo.GetPointAtFractionLength(0, out Point P0, out Point T0);
        //    for (int i = 1; i < Steps; i++)
        //    {
        //        double progress = (double)(1d / Steps) * i;
        //        geo.GetPointAtFractionLength(progress, out P1, out Point T1);
        //        list.Add(P1);

        //        if (Double.IsNaN(P0.X) || Double.IsNaN(P0.Y) || Double.IsInfinity(P0.X) || Double.IsInfinity(P0.Y)) throw new Exception("invalid Coordinates");
        //        P0 = P1;
        //    }
        //    if (fig.IsClosed)
        //    {
        //        list.Add(P_Start);
        //    }
        //    return list;
        //}
        //public List<Point> getFigurePoints(PathFigure fig)
        //{

        //    if (fig == null) return new List<Point>();
        //    List<Point> list = new List<Point>();
        //    bool succes;
        //    list = getFigurePoints(fig, out succes);
        //    if (!succes)
        //    {
        //        list = getFigurePoints(fig, PathFigureIterationSteps);
        //    }

        //    return list;
        //}

        #region static Conversions
        static netDxf.Entities.Polyline PointsToPolyLine(List<Point> pts, int layer = 0)
        {
            Polyline poly = new Polyline(ToPolyLineVertexes(pts));
            poly.Color = AciColor.Blue;
            poly.Lineweight = Lineweight.W13;
            poly.Layer = new netDxf.Tables.Layer($"PathFigure{layer}");
            poly.IsClosed = true;
            return poly;
        }
        static netDxf.Entities.Ellipse ToDxfEllipse(EllipseGeometry ell)
        {
            Vector2 P = new Vector2(ell.Center.X, ell.Center.Y);
            Ellipse dxfEllips = new Ellipse(P, ell.RadiusX, ell.RadiusY);
            return dxfEllips;
        }




        static IEnumerable<PolylineVertex> ToPolyLineVertexes(List<Point> pts)
        {
            foreach (System.Windows.Point p in pts)
            {
                PolylineVertex vertex = new PolylineVertex(p.X, p.Y, 0);
                yield return vertex;
            }
        }
        #endregion

    }
}
