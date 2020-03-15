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

namespace testImageDetection
{
    class ImageDetectorByContour
    {
        public ImageDetectorByContour(string templateFile)
        {
            template = new ContouredImage(templateFile);
            MainForm.This.TemplateBox.Image = drawContours(template.GreyImage, template.CvContours);
        }
        ContouredImage template;

        public float MaxExpectedRotationDeviation = 20;
        public float MaxExpectedScaleDeviation = 0.9f;
        public float MaxCvMatchResult = 0.4f;
        public float Threshold = 0.5f;

        public List<Result> FindOnPage(string pageFile, bool allMatches = true)
        {
            List<Result> results = new List<Result>();
            ContouredImage page = new ContouredImage(pageFile);
            //float minTemplateContourLength = Math.Max(template.GreyImage.Width, template.GreyImage.Height) / 10;

            if (allMatches)
            {
                List<Match> matches = new List<Match>();
                foreach (Contour templateContour in template.RobustContours)
                {
                    foreach (Contour pageContour in page.RobustContours)
                    {
                        Match m = new Match(templateContour, pageContour);
                        if (m.CvMatch > MaxCvMatchResult)
                            continue;
                        if (Math.Abs(m.Page2Template180InvariantRotation) > MaxExpectedRotationDeviation)
                            continue;
                        if (Math.Abs(m.Page2TemplateScale - 1) > MaxExpectedScaleDeviation)
                            continue;
                        matches.Add(m);
                    }
                }
                foreach (Match m in matches)
                {
                    List<Match> matchPossibleCollection = new List<Match>();
                    matchPossibleCollection.Add(m);

                    foreach (Contour tc in template.RobustContours)
                    {
                        if (tc == m.TemplateContour)
                            continue;
                        const float padding = 0.3f;
                        RotatedRect expectedRotatedRect = new RotatedRect(
                            new PointF(tc.RotatedRect.Center.X * m.Page2TemplateScale, tc.RotatedRect.Center.Y * m.Page2TemplateScale),
                            new SizeF(tc.RectangleF.Width * (m.Page2TemplateScale + padding), tc.RectangleF.Height * (m.Page2TemplateScale + padding)),
                            tc.Angle
                            );
                        VectorOfPointF ps = new VectorOfPointF(expectedRotatedRect.GetVertices());
                        Match m2 = matches.FirstOrDefault(x =>
                            x != m
                            && x.TemplateContour == tc
                            && Math.Abs(x.Page2TemplateScale - m.Page2TemplateScale) < 0.2f
                            //&& CvInvoke.PointPolygonTest(ps, x.PageContour.RotatedRectPoints[0], false) > 0
                            //&& CvInvoke.PointPolygonTest(ps, x.PageContour.RotatedRectPoints[1], false) > 0
                            //&& CvInvoke.PointPolygonTest(ps, x.PageContour.RotatedRectPoints[2], false) > 0
                            //&& CvInvoke.PointPolygonTest(ps, x.PageContour.RotatedRectPoints[3], false) > 0
                        );
                        if (m2 != null)
                            matchPossibleCollection.Add(m2);
                    }
                    results.Add(CreateResult(template, matchPossibleCollection));
                }
                results = results.OrderByDescending(x => x.MatchCollection.Count).ToList();
                //if ( < Threshold)
                //    break;
                if (results.Count > 0)
                {
                    results = results.Where(x => x.MatchCollection.Count > 15).ToList();
                    VectorOfVectorOfPoint bestMatchCollectionCvContours = new VectorOfVectorOfPoint();
                    //results.ForEach(x => bestMatchCollectionCvContours.Push(new VectorOfPoint(x.RotatedRect.GetVertices().Select(y => new Point((int)y.X, (int)y.Y)).ToArray())));
                    results.ForEach(x => x.MatchCollection.ForEach(a=> bestMatchCollectionCvContours.Push(a.PageContour.Points)));
                    MainForm.This.PageBox.Image = drawContours(page.GreyImage, bestMatchCollectionCvContours);
                }
            }
            else
            {
                throw new Exception("TBD");
            }
            if (results.Count > 0)
            {
                return results;
            }
            return null;
        }

        static ImageDetectorByContour()
        {
            Result.Initialize();
        }
        private static Func<ContouredImage, List<Match>, Result> CreateResult;//to keep Result constructor hidden

        public class Result
        {
            internal static void Initialize()
            {
                CreateResult = Create;
            }
            static Result Create(ContouredImage template, List<Match> goodMatchCollection)
            {
                return new Result( template, goodMatchCollection);
            }

            Result(ContouredImage template, List<Match> goodMatchCollection)
            {
                this.template = template;
                MatchCollection = goodMatchCollection;
                Rotation = goodMatchCollection[0].Page2Template180InvariantRotation;
                Scale = goodMatchCollection[0].Page2TemplateScale;
            }
            readonly ContouredImage template;

            public readonly List<Match> MatchCollection;
            public readonly float Rotation;
            public readonly float Scale;

            public float Goodness
            {
                get
                {
                    if (_Goodness < 0)
                        _Goodness = (float)MatchCollection.Count / template.RobustContours.Count;
                    return _Goodness;
                }
            }
            float _Goodness = -1;

            public RotatedRect RotatedRect
            {
                get
                {
                    if (_RotatedRect.Size == RotatedRect.Empty.Size)
                    {
                        VectorOfPoint ps = new VectorOfPoint();
                        foreach (Match m in MatchCollection)
                            ps.Push(m.PageContour.Points);
                        _RotatedRect = CvInvoke.MinAreaRect(ps);
                    }
                    return _RotatedRect;
                }
            }
            RotatedRect _RotatedRect = RotatedRect.Empty;
        }

        //class Matches
        //{
        //    public Matches(ContouredImage template, ContouredImage page)
        //    {
        //        matches = new Match[template.Contours.Count, page.Contours.Count];
        //        this.template = template;
        //        this.page = page;
        //    }
        //    readonly Match[,] matches;
        //    readonly ContouredImage template;
        //    readonly ContouredImage page;

        //    public Match this[int templateContourId, int pageContourId]
        //    {
        //        get
        //        {
        //            Match m = matches[templateContourId, pageContourId];
        //            if (m == null)
        //                m = new Match(template.Contours[templateContourId], page.Contours[pageContourId]);
        //            return m;
        //        }
        //    }
        //}

        public class Match
        {
            public Match(Contour templateContour, Contour pageContour)
            {
                TemplateContour = templateContour;
                PageContour = pageContour;
            }
            public readonly Contour TemplateContour;
            public readonly Contour PageContour;

            //public bool IsConsiderable
            //{
            //    get
            //    {
            //        return CvMatch < 0.2 && Angle < 10 && Scale > 0.9 && Scale < 1 / 0.9;
            //    }
            //}

            public double CvMatch
            {
                get
                {
                    if (_CvMatch == double.MinValue)
                        _CvMatch = Emgu.CV.CvInvoke.MatchShapes(TemplateContour.Points, PageContour.Points, ContoursMatchType.I2);
                    return _CvMatch;
                }
            }
            double _CvMatch = double.MinValue;

            public float Page2Template180InvariantRotation//some contours are detected as if overturned
            {
                get
                {
                    if (_Rotation == float.MinValue)
                    {
                        _Rotation = PageContour.RotatedRect.Angle - TemplateContour.RotatedRect.Angle;
                        //if (_Rotation > 90)
                        //    _Rotation -= 180;
                        //else if (_Rotation < -90)
                        //    _Rotation += 180;
                    }
                    return _Rotation;
                }
            }
            float _Rotation = float.MinValue;

            public float Page2TemplateScale
            {
                get
                {
                    if (_Scale == float.MinValue)
                        _Scale = PageContour.Length / TemplateContour.Length;
                    return _Scale;
                }
            }
            float _Scale = float.MinValue;
        }

        class ContouredImage
        {
            const int MinContourPointCount = 10;
            public ContouredImage(string imageFile)
            {
                GreyImage = getPreprocessedImage(imageFile);
                //!!!try to make more primitive
                //Mat tmp = grey.clone();
                //morphologyEx(tmp, tmp, MORPH_GRADIENT, getStructuringElement(MORPH_ELLIPSE, Size(3, 3)));
                //bitwise_not(tmp, tmp);
                //!!!try to smooth
                //epsilon = 0.1 * cv.arcLength(cnt, True)
                //approx = cv.approxPolyDP(cnt, epsilon, True)
                Emgu.CV.CvInvoke.FindContours(GreyImage, CvContours, Hierarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                Array hierarchy = Hierarchy.GetData();
                for (int i = 0; i < CvContours.Size; i++)
                    Contours.Add(new Contour(hierarchy, i, CvContours[i]));

                RobustContours = Contours.Where(x => x.Points.Size >= MinContourPointCount).ToList();//!!!RotatedRect cannot be calculated for Points.Size < 5
            }
            public readonly Image<Gray, byte> GreyImage;
            public readonly VectorOfVectorOfPoint CvContours = new VectorOfVectorOfPoint();
            public readonly Mat Hierarchy = new Mat();
            readonly List<Contour> Contours = new List<Contour>();
            public readonly List<Contour> RobustContours;

            static private Image<Gray, byte> getPreprocessedImage2(string imageFile)
            {
                Image<Gray, byte> image = new Image<Gray, byte>(imageFile);
                Emgu.CV.CvInvoke.Blur(image, image, new Size(10, 10), new Point(0, 0));
                //Emgu.CV.CvInvoke.Threshold(image, image, 60, 255, ThresholdType.Otsu | ThresholdType.Binary);
                //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                //CvInvoke.Dilate(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                CvInvoke.Canny(image, image, 100, 30, 3);
                return image;
            }

            static private Image<Gray, byte> getPreprocessedImage(string imageFile)
            {
                Image<Gray, byte> image = new Image<Gray, byte>(imageFile);
                Emgu.CV.CvInvoke.Blur(image, image, new Size(3, 3), new Point(0, 0));
                Emgu.CV.CvInvoke.Threshold(image, image, 125, 255, ThresholdType.Otsu | ThresholdType.Binary);
                Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                CvInvoke.Dilate(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                return image;
            }
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
            class HierarchyKey
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
                    if (_Angle < -400)
                    {
                        if (RotatedRect.Size.Width > RotatedRect.Size.Height)
                            _Angle = 90 + RotatedRect.Angle;
                        else
                            _Angle = RotatedRect.Angle;
                    }
                    return _Angle;
                }
            }
            float _Angle = -401;

            public PointF[] RotatedRectPoints
            {
                get
                {
                    if (_RotatedRectPoints==null)
                        _RotatedRectPoints = RotatedRect.GetVertices();
                    return _RotatedRectPoints;
                }
            }
            PointF[] _RotatedRectPoints = null;

            public RotatedRect RotatedRect
            {
                get
                {
                    if (_RotatedRect.Size == RotatedRect.Empty.Size)
                        _RotatedRect = Emgu.CV.CvInvoke.FitEllipse(Points);
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
                        _Area = Emgu.CV.CvInvoke.ContourArea(Points);
                    return _Area;
                }
            }
            double _Area = -1;

            public RectangleF RectangleF
            {
                get
                {
                    if (_RectangleF == Rectangle.Empty)
                        _RectangleF = RotatedRect.MinAreaRect();
                    return _RectangleF;
                }
            }
            RectangleF _RectangleF = RectangleF.Empty;
        }

        static private Bitmap drawContours(Image<Gray, byte> image, VectorOfVectorOfPoint contours)
        {
            Image<Rgb, byte> image2 = new Image<Rgb, byte>(image.Size);
            Emgu.CV.CvInvoke.CvtColor(image, image2, Emgu.CV.CvEnum.ColorConversion.Gray2Rgb);
            Emgu.CV.CvInvoke.DrawContours(image2, contours, -1, new MCvScalar(255, 0, 0), 1);
            return image2.ToBitmap();
        }
    }
}