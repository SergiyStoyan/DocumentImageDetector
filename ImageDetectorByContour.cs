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

        public bool FindOnPage(string pageFile)
        {
            ContouredImage page = new ContouredImage(pageFile);

            float minTemplateContourLength = Math.Max(template.GreyImage.Width, template.GreyImage.Height) / 10;

            List<Match> matches = new List<Match>();
            int consideredTeplateContoursCount = 0;
            VectorOfVectorOfPoint matchedContours = new VectorOfVectorOfPoint();
            foreach (Contour templateC in template.RobustContours)//.OrderByDescending(x => x.Points.Size))
            {
                //if (templateC.ParentId > -1)
                //    continue;
                //if (templateC.Length < minTemplateContourLength)
                //    continue;
                consideredTeplateContoursCount++;
                foreach (Contour pageC in page.RobustContours)
                {
                    Match m = new Match(templateC, pageC);
                    if (m.CvMatch > 0.2)
                        continue;
                    if (m.Angle > 10)
                        continue;
                    if (m.Scale < 0.9 || m.Scale > 1 / 0.9)
                        continue;
                    matches.Add(m);
                    matchedContours.Push(pageC.Points);
                }
            }
            //float currentAngle = 0;
            //float currentScale = 0;
            //foreach(Match m in matches)
            //{

            //}
            MainForm.This.PageBox.Image = drawContours(page.GreyImage, matchedContours);
            if (matches.Count / consideredTeplateContoursCount > 0.5)
                return true;
            return false;
        }

        class Match
        {
            public Match(Contour templateC, Contour pageC)
            {
                TemplateC = templateC;
                PageC = pageC;
                

            }
            public readonly Contour TemplateC;
            public readonly Contour PageC;

            public  double CvMatch
            {
                get
                {
                    if (_CvMatch == double.MinValue)
                        _CvMatch = Emgu.CV.CvInvoke.MatchShapes(TemplateC.Points, PageC.Points, ContoursMatchType.I2);
                    return _CvMatch;
                }
            }
            double _CvMatch = double.MinValue;

            public float Angle
            {
                get
                {
                    if (_Angle == float.MinValue)
                        _Angle = Math.Abs(TemplateC.RotatedRect.Angle - PageC.RotatedRect.Angle);
                    return _Angle;
                }
            }
            float _Angle = float.MinValue;

            public float Scale
            {
                get
                {
                    if (_Scale == float.MinValue)
                        _Scale = TemplateC.Length / PageC.Length;
                    return _Scale;
                }
            }
            float _Scale = float.MinValue;
        }

        class ContouredImage
        {
            public ContouredImage(string imageFile)
            {
                GreyImage = getPreprocessedImage(imageFile);
                Emgu.CV.CvInvoke.FindContours(GreyImage, CvContours, Hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

                Array hierarchy = Hierarchy.GetData();
                for (int i = 0; i < CvContours.Size; i++)
                    Contours.Add(new Contour(hierarchy, i, CvContours[i]));

                RobustContours = Contours.Where(x => x.Points.Size >= 10).ToList();//!!!RotatedRect cannot be calculated for Points.Size < 5
            }
            public readonly Image<Gray, byte> GreyImage;
            public readonly VectorOfVectorOfPoint CvContours = new VectorOfVectorOfPoint();
            public readonly Mat Hierarchy=new Mat();
            public readonly List<Contour> Contours = new List<Contour>();
            public readonly List<Contour> RobustContours;
        }

        class Contour
        {
            public Contour(Array hierarchy, int i,VectorOfPoint points)
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
                    if (_Area<0)
                        _Area = Emgu.CV.CvInvoke.ContourArea(Points);
                    return _Area;
                }
            }
            double _Area = -1;
        }

        //static private void get


        static private Bitmap drawContours(Image<Gray, byte> image, VectorOfVectorOfPoint contours)
        {
            Image<Rgb, byte> image2 = new Image<Rgb, byte>(image.Size);
            Emgu.CV.CvInvoke.CvtColor(image, image2, Emgu.CV.CvEnum.ColorConversion.Gray2Rgb);
            Emgu.CV.CvInvoke.DrawContours(image2, contours, -1, new MCvScalar(255, 0, 0), 1);
            return image2.ToBitmap();
        }

        static private VectorOfVectorOfPoint getContours2(string imageFile, out Mat hierachy, out Image<Gray, byte> image)//good!
        {
            image = new Image<Gray, byte>(imageFile);
            Emgu.CV.CvInvoke.Blur(image, image, new Size(10, 10), new Point(0, 0));
            Emgu.CV.CvInvoke.Threshold(image, image, 60, 255, ThresholdType.Otsu | ThresholdType.Binary);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //CvInvoke.Dilate(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            hierachy = new Mat();
            Emgu.CV.CvInvoke.FindContours(image, contours, hierachy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            return contours;
        }

        static private Image<Gray, byte> getPreprocessedImage(string imageFile)
        {
            Image<Gray, byte> image = new Image<Gray, byte>(imageFile);
            //Emgu.CV.CvInvoke.Blur(image, image, new Size(10, 10), new Point(0, 0));
            //Emgu.CV.CvInvoke.Threshold(image, image, 60, 255, ThresholdType.Otsu | ThresholdType.Binary);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Dilate(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Canny(image, image, 100, 30, 3);
            return image;
        }
    }
}
