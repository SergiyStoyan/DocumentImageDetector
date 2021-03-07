//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        http://www.cliversoft.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;
using Emgu.CV.Features2D;

namespace Cliver.testImageDetection
{
    class Deskewer
    {
        static public void DeskewAsSingleBlock(ref Bitmap bitmap)//good
        {
            using (Image<Rgb, byte> image = bitmap.ToImage<Rgb, byte>())
            {
                bitmap.Dispose();
                bitmap = deskew(image)?.ToBitmap();
            }
        }

        static Image<Rgb, byte> deskew(Image<Rgb, byte> image)
        {
            Image<Gray, byte> image2 = image.Convert<Gray, byte>();

            //List<LineSegment2D> lines = getLines(image2, 200);
            //image = image2.Convert<Rgb, byte>();
            //foreach (var l in lines)
            //    image.Draw(l, new Rgb(255, 0, 0), 1);
            //return image;

            double angle;
            //angle = getDeskewAngleByLines(image2);
            //if (angle < -360)
            angle = getDeskewAngleByLongestBlock(image2);

            if (angle == 0)
                return image;
            RotationMatrix2D rotationMat = new RotationMatrix2D();
            CvInvoke.GetRotationMatrix2D(new PointF((float)image.Width / 2, (float)image.Height / 2), angle, 1, rotationMat);
            Image<Rgb, byte> image3 = new Image<Rgb, byte>(image.Size);
            CvInvoke.WarpAffine(image, image3, rotationMat, image.Size);
            return image3;
        }

        //static List<Contour> getLines1(Image<Gray, byte> image, Size lineSize)
        //{
        //    List<Contour> lines = new List<Contour>();
        //    Mat se = CvInvoke.GetStructuringElement(ElementShape.Rectangle, lineSize, new Point(-1, -1));
        //    Image<Gray, byte> image2 = new Image<Gray, byte>(image.Size);
        //    CvInvoke.MorphologyEx(image, image2, MorphOp.Open, se, new Point(-1, -1), 2, BorderType.Default, new MCvScalar());
        //    VectorOfVectorOfPoint cs = new VectorOfVectorOfPoint();
        //    Mat h = new Mat();
        //    CvInvoke.FindContours(image2, cs, h, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
        //    if (cs.Size < 1)
        //        return lines;
        //    Array hierarchy = h.GetData();
        //    List<Contour> contours = new List<Contour>();
        //    for (int i = 0; i < cs.Size; i++)
        //        contours.Add(new Contour(hierarchy, i, cs[i]));
        //    if (contours.Where(a => a.ParentId < 0).Count() < 2)//the only parent is the whole page frame
        //        contours.RemoveAll(a => a.ParentId < 0);
        //    else
        //        contours.RemoveAll(a => a.ParentId >= 0);
        //    for (int i = 0; i < contours.Count; i++)
        //        lines.Add(contours[i]);
        //    return lines;
        //}

        static List<LineSegment2D> getLines(Image<Gray, byte> image, int minLineSize)//!!!needs tuning
        {
            CvInvoke.BitwiseNot(image, image);//to negative
            CvInvoke.GaussianBlur(image, image, new Size(9, 9), 0);//remove small spots
            CvInvoke.Threshold(image, image, 125, 255, ThresholdType.Otsu | ThresholdType.Binary);
            Mat se = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(30, 5), new Point(-1, -1));
            CvInvoke.Dilate(image, image, se, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Canny(image, image, 100, 30, 3);
            return CvInvoke.HoughLinesP(image, 1, 2 * Math.PI / 180, 10, minLineSize, 10).ToList();
        }

        static double getDeskewAngleByLines(Image<Gray, byte> image)
        {
            List<LineSegment2D> lines = new List<LineSegment2D>();
            //int maxAngle = 10;
            float angleDeviation = 2;
            // Detect horizontal lines
            lines = getLines(image, 100);
            //lines.AddRange(getLines(image, new Size(2, 50)));
            //return 0;
            if (lines.Count < 1)
                return -400;
            LineSegment2D horisontalLine = new LineSegment2D(new Point(0, 0), new Point(1000, 0));
            List<double> angles = new List<double>();
            foreach (var l in lines)
            {
                double a = l.GetExteriorAngleDegree(horisontalLine);
                //double a = l.Angle;
                angles.Add(a);
            }
            List<List<double>> bestAss = new List<List<double>>();
            for (int i = 0; i < angles.Count; i++)
            {
                List<double> as_ = new List<double>();
                bestAss.Add(as_);
                double ai = angles[i];
                for (int j = 0; j < angles.Count; j++)
                {
                    double aj = angles[j];
                    if (Math.Abs(ai - aj) < angleDeviation)
                        as_.Add(aj);
                }
            }
            List<double> bestAs = bestAss.Aggregate((a, b) => a.Count > b.Count ? a : b);
            return bestAs.Average();
        }

        static double getDeskewAngleByLongestBlock(Image<Gray, byte> image)
        {//https://becominghuman.ai/how-to-automatically-deskew-straighten-a-text-image-using-opencv-a0c30aed83df
            CvInvoke.BitwiseNot(image, image);//to negative
            CvInvoke.GaussianBlur(image, image, new Size(9, 9), 0);//remove small spots
            CvInvoke.Threshold(image, image, 125, 255, ThresholdType.Otsu | ThresholdType.Binary);
            Mat se = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(30, 1), new Point(-1, -1));
            CvInvoke.Dilate(image, image, se, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(image, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            if (contours.Size < 1)
                return -400;
            int maxW = 0;
            VectorOfPoint bestContour = null;
            double angle = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                RotatedRect rr = CvInvoke.MinAreaRect(contours[i]);
                Rectangle r = rr.MinAreaRect();
                int w = r.Width > r.Height ? r.Width : r.Height;
                if (maxW < w)
                {
                    maxW = w;
                    angle = rr.Angle;
                    bestContour = contours[i];
                }
            }

            //if (n++ == 0)
            //{
            //    Image<Rgb, byte> image1 = image.Convert<Rgb, byte>();
            //    image1.Draw(bestContour.ToArray(), new Rgb(255, 0, 0), 1);
            //    MainForm.This.PageBox.Image = image1.ToBitmap();
            //}

            if (angle > 45)
                angle -= 90;
            else if (angle < -45)
                angle += 90;
            return angle;
        }
       // static int n = 0;

        static double getDeskewAngleByLongestBlock2(Image<Gray, byte> image)
        {//https://becominghuman.ai/how-to-automatically-deskew-straighten-a-text-image-using-opencv-a0c30aed83df
            CvInvoke.BitwiseNot(image, image);//to negative
            CvInvoke.GaussianBlur(image, image, new Size(9, 9), 0);//remove small spots
            CvInvoke.Threshold(image, image, 125, 255, ThresholdType.Otsu | ThresholdType.Binary);
            Mat se = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(30, 5), new Point(-1, -1));
            CvInvoke.Dilate(image, image, se, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(image, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            if (contours.Size < 1)
                return -400;
            List<Angle> angles = new List<Angle>();// (a*wa+b*wb)/(n*(wa+wb))   
            for (int i = 0; i < contours.Size; i++)
            {
                RotatedRect rr = CvInvoke.MinAreaRect(contours[i]);
                double a = rr.Angle;
                Rectangle r = rr.MinAreaRect();
                int w = r.Width > r.Height ? r.Width : r.Height;
                if (a > 45)
                    a -= 90;
                else if (a < -45)
                    a += 90;
                angles.Add(new Angle { angle = a, width = w });
            }
            double s = angles.Sum(a => a.angle * a.width);
            double b = angles.Sum(a => a.width) * angles.Count;
            double angle = s / b;
            return angle;
        }

        static double getDeskewAngleByLongestBlock4(Image<Gray, byte> image)
        {//https://becominghuman.ai/how-to-automatically-deskew-straighten-a-text-image-using-opencv-a0c30aed83df
            CvInvoke.BitwiseNot(image, image);//to negative
            CvInvoke.GaussianBlur(image, image, new Size(9, 9), 0);//remove small spots
            CvInvoke.Threshold(image, image, 125, 255, ThresholdType.Otsu | ThresholdType.Binary);
            Mat se = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(30, 1), new Point(-1, -1));
            CvInvoke.Dilate(image, image, se, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(image, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            if (contours.Size < 1)
                return -400;
            List<Angle> angles = new List<Angle>();// (a*wa+b*wb)/(n*(wa+wb))   
            for (int i = 0; i < contours.Size; i++)
            {
                RotatedRect rr = CvInvoke.MinAreaRect(contours[i]);
                double a = rr.Angle;
                Rectangle r = rr.MinAreaRect();
                int w = r.Width > r.Height ? r.Width : r.Height;
                if (a > 45)
                    a -= 90;
                else if (a < -45)
                    a += 90;
                angles.Add(new Angle { angle = a, width = w });
            }
            //double s = angles.Sum(a => a.angle * a.width);
            //double b = angles.Sum(a => a.width) * angles.Count;
            //double angle = s / b;
            angles = angles.OrderByDescending(a => a.width).ToList();
            double angle = (angles[0].angle + angles[1].angle + angles[2].angle) / 3;
            return angle;
        }
        class Angle
        {
            public double angle;
            public int width;
        }

        static public void DeskewAsColumnOfBlocks(ref Bitmap bitmap, int blockMaxHeight, int minBlockSpan)
        {
            using (Image<Rgb, byte> image = bitmap.ToImage<Rgb, byte>())
            {
                bitmap.Dispose();

                //return image.ToBitmap();
                Image<Rgb, byte> deskewedimage = new Image<Rgb, byte>(image.Size);
                Image<Gray, byte> image2 = image.Convert<Gray, byte>();
                CvInvoke.BitwiseNot(image2, image2);
                //CvInvoke.Blur(image2, image2, new Size(3, 3), new Point(0, 0));
                CvInvoke.GaussianBlur(image2, image2, new Size(25, 25), 5);//remove small spots
                CvInvoke.Threshold(image2, image2, 125, 255, ThresholdType.Otsu | ThresholdType.Binary);
                Mat se = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(-1, -1));
                CvInvoke.Dilate(image2, image2, se, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                //CvInvoke.Erode(image2, image2, se, new Point(-1, -1), 5, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

                //CvInvoke.BitwiseNot(image2, image2);
                //return image2.ToBitmap();

                VectorOfVectorOfPoint cs = new VectorOfVectorOfPoint();
                Mat h = new Mat();
                CvInvoke.FindContours(image2, cs, h, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
                if (cs.Size < 1)
                    return;

                Array hierarchy = h.GetData();
                List<Contour> contours = new List<Contour>();
                for (int i = 0; i < cs.Size; i++)
                {
                    int p = (int)hierarchy.GetValue(0, i, Contour.HierarchyKey.Parent);
                    if (p < 1)
                        contours.Add(new Contour(hierarchy, i, cs[i]));
                }
                if (contours.Where(a => a.ParentId < 0).Count() < 2)//the only parent is the whole page frame
                    contours.RemoveAll(a => a.ParentId < 0);
                else
                    contours.RemoveAll(a => a.ParentId >= 0);

                contours = contours.OrderBy(a => a.BoundingRectangle.Bottom).ToList();
                for (int blockY = 0; blockY < image.Height;)
                {
                    int blockBottom = image.Height - 1;
                    Tuple<Contour, Contour> lastSpan = null;
                    for (; contours.Count > 0;)
                    {
                        Contour c = contours[0];
                        contours.RemoveAt(0);
                        if (contours.Count > 0)
                        {
                            Contour minTop = contours.Aggregate((a, b) => a.BoundingRectangle.Top < b.BoundingRectangle.Top ? a : b);
                            if (c.BoundingRectangle.Bottom + minBlockSpan <= minTop.BoundingRectangle.Top)
                                lastSpan = new Tuple<Contour, Contour>(c, minTop);
                        }

                        if (c.BoundingRectangle.Bottom > blockY + blockMaxHeight && lastSpan != null)
                        {
                            blockBottom = lastSpan.Item1.BoundingRectangle.Bottom + minBlockSpan / 2;
                            break;
                        }
                    }

                    Rectangle blockRectangle = new Rectangle(0, blockY, image2.Width, blockBottom + 1 - blockY);
                    Image<Rgb, byte> blockImage = image.Copy(blockRectangle);
                    blockImage = deskew(blockImage);
                    deskewedimage.ROI = blockRectangle;
                    blockImage.CopyTo(deskewedimage);
                    deskewedimage.ROI = Rectangle.Empty;
                    // break;
                    blockY = blockBottom + 1;
                }
                bitmap = deskewedimage?.ToBitmap();
            }
        }

        //public Bitmap DeskewByBlocks(Size blockMaxSize, int minBlockSpan)//!!!not completed
        //{
        //    Image<Gray, byte> image2 = image.Clone();
        //    CvInvoke.BitwiseNot(image2, image2);
        //    //CvInvoke.Blur(image2, image2, new Size(3, 3), new Point(0, 0));
        //    CvInvoke.GaussianBlur(image2, image2, new Size(25, 25), 5);//remove small spots
        //    CvInvoke.Threshold(image2, image2, 125, 255, ThresholdType.Otsu | ThresholdType.Binary);
        //    Mat se = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(-1, -1));
        //    CvInvoke.Dilate(image2, image2, se, new Point(-1, -1), 5, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
        //    //CvInvoke.Erode(image2, image2, se, new Point(-1, -1), 5, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

        //    CvInvoke.BitwiseNot(image2, image2);
        //    //return image2.ToBitmap();

        //    VectorOfVectorOfPoint cs = new VectorOfVectorOfPoint();
        //    Mat h = new Mat();
        //    CvInvoke.FindContours(image2, cs, h, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
        //    if (cs.Size < 1)
        //        return null;

        //    Array hierarchy = h.GetData();
        //    List<Contour> contours = new List<Contour>();
        //    for (int i = 0; i < cs.Size; i++)
        //    {
        //        int p = (int)hierarchy.GetValue(0, i, Contour.HierarchyKey.Parent);
        //        if (p < 1)
        //            contours.Add(new Contour(hierarchy, i, cs[i]));
        //    }
        //    if (contours.Where(a => a.ParentId < 0).Count() < 2)//the only parent is the whole page frame
        //        contours.RemoveAll(a => a.ParentId < 0);
        //    else
        //        contours.RemoveAll(a => a.ParentId >= 0);

        //    int x = 0;
        //    int y = 0;
        //    for (Rectangle br = new Rectangle(-2, -2, -1, -1); ;)
        //    {
        //        x = br.Right + 1;
        //        if (x >= image.Width)
        //        {
        //            x = 0;
        //            y = br.Bottom + 1;
        //            if (y >= image.Height)
        //                break;
        //        }
        //        int blockWidth = blockMaxSize.Width;
        //        if (x + blockWidth > image.Width)
        //            blockWidth = image.Width - x;
        //        int blockHight = blockMaxSize.Height;
        //        if (y + blockHight > image.Height)
        //            blockHight = image.Height - y;
        //        br = new Rectangle(new Point(x, y), new Size(blockWidth, blockHight));

        //        int lastId;
        //        for (int j = 0; j < contours.Count; j++)
        //        {
        //            if (br.Contains(contours[j].BoundingRectangle))
        //            {
        //                contours.RemoveAt(j);
        //                j--;
        //            }

        //        }
        //        //deskew()
        //    }
        //    return image2.ToBitmap();
        //}
    }

    public class Contour
    {
        public Contour(Array hierarchy, int i, VectorOfPoint points)
        {
            I = i;
            Points = points;
            NextSiblingId = (int)hierarchy.GetValue(0, i, HierarchyKey.NextSibling);
            PreviousSiblingId = (int)hierarchy.GetValue(0, i, HierarchyKey.PreviousSibling);
            FirstChildId = (int)hierarchy.GetValue(0, i, HierarchyKey.FirstChild);
            ParentId = (int)hierarchy.GetValue(0, i, HierarchyKey.Parent);
        }
        public class HierarchyKey
        {
            public const int NextSibling = 0;
            public const int PreviousSibling = 1;
            public const int FirstChild = 2;
            public const int Parent = 3;
        }

        public readonly int I;
        public readonly VectorOfPoint Points;

        public readonly int NextSiblingId = 0;
        public readonly int PreviousSiblingId = 1;
        public readonly int FirstChildId = 2;
        public readonly int ParentId = 3;

        public float Angle
        {
            get
            {
                if (_Angle < -360)
                {
                    if (RotatedRect.Size.Width > RotatedRect.Size.Height)
                        _Angle = 90 + RotatedRect.Angle;
                    else
                        _Angle = RotatedRect.Angle;
                }
                return _Angle;
            }
        }
        float _Angle = -400;

        //public PointF[] RotatedRectPoints
        //{
        //    get
        //    {
        //        if (_RotatedRectPoints == null)
        //            _RotatedRectPoints = RotatedRect.GetVertices();
        //        return _RotatedRectPoints;
        //    }
        //}
        //PointF[] _RotatedRectPoints = null;

        public RotatedRect RotatedRect
        {
            get
            {
                if (_RotatedRect.Size == RotatedRect.Empty.Size)
                {
                    if (Points.Size > 4)
                        _RotatedRect = CvInvoke.FitEllipse(Points);
                    //else
                    //{//if <5 then exception
                    //    Point[] ps = new Point[5];
                    //    Array.Copy(Points.ToArray(), ps, Points.Size);
                    //    for (int i = Points.Size; i < 5; i++)
                    //        ps[i] = Points[0];
                    //    _RotatedRect = CvInvoke.FitEllipse(new VectorOfPoint(ps));
                    //}
                }
                return _RotatedRect;
            }
        }
        RotatedRect _RotatedRect = RotatedRect.Empty;

        public float Length
        {
            get
            {
                if (_Length < 0)
                    _Length = Math.Max(RotatedRect.Size.Width, RotatedRect.Size.Height);
                return _Length;
            }
        }
        float _Length = -1;

        public double Area
        {
            get
            {
                if (_Area < 0)
                    _Area = CvInvoke.ContourArea(Points);
                return _Area;
            }
        }
        double _Area = -1;

        public RectangleF MinAreaRectangle
        {
            get
            {
                if (_MinAreaRect == RectangleF.Empty)
                    _MinAreaRect = RotatedRect.MinAreaRect();
                return _MinAreaRect;
            }
        }
        RectangleF _MinAreaRect = RectangleF.Empty;

        public Rectangle BoundingRectangle
        {
            get
            {
                if (_BoundingRectangle == Rectangle.Empty)
                    _BoundingRectangle = CvInvoke.BoundingRectangle(Points);
                return _BoundingRectangle;
            }
        }
        Rectangle _BoundingRectangle = Rectangle.Empty;

    }
}