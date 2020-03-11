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
        //static private bool DetectSimilarImage()
        //{
        //    using (Image<Gray, byte> inputImage = new Image<Gray, byte>(@"c:\\temp\pp3.png"))
        //    {
        //        using (Image<Gray, byte> templateImage = new Image<Gray, byte>(@"c:\\temp\ppt.png"))
        //        {
        //            using (Image<Gray, float> match = inputImage.MatchTemplate(templateImage, TemplateMatchingType.Ccoeff))
        //            {
        //                //match.ROI = new Rectangle(templateImage.Width, templateImage.Height, inputImage.Width, inputImage.Height);

        //                Point[] MAX_Loc, Min_Loc;
        //                double[] min, max;
        //                match.MinMax(out min, out max, out Min_Loc, out MAX_Loc);

        //                using (Image<Gray, double> RG_Image = match.Convert<Gray, double>().Copy())
        //                {
        //                    if (max[0] > 0.85)
        //                    {
        //                        //Object_Location = MAX_Loc[0];
        //                        return true;
        //                    }
        //                }

        //            }
        //        }
        //    }
        //    return false;
        //}

        static public bool FindMatch(string pageFile, string templateFile)
        {
            VectorOfVectorOfPoint pageContours = getContours(pageFile, out Mat pageContoursHierachy, out Image<Gray, byte> image);
            MainForm.This.PageBox.Image = drawContours(image, pageContours);

            VectorOfVectorOfPoint templateContours = getContours(templateFile, out Mat templateContoursHierachy, out Image<Gray, byte> template);
            MainForm.This.TemplateBox.Image = drawContours(template, templateContours);

            return compareContours(pageContours, pageContoursHierachy, templateContours, templateContoursHierachy);
        }

        static private bool compareContours(VectorOfVectorOfPoint pageContours, Mat pageContoursHierachy, VectorOfVectorOfPoint templateContours, Mat templateContoursHierachy)
        {
            Array templateCH = templateContoursHierachy.GetData();
            int templateCHLength = templateCH.GetLength(1);
            Array pageCH = pageContoursHierachy.GetData();
            int pageCHLength = pageCH.GetLength(1);
            for (int i = 0; i < templateCHLength; i++)
            {
                if (-1 < (int)templateCH.GetValue(0, i, HierarchyKey.Parent))
                    continue;
                double m = 0;
                for (int j = 0; j < pageCHLength; j++)
                {
                    //double m = Emgu.CV.CvInvoke.MatchShapes(templateContours[i], pageContours[j], ContoursMatchType.I3);

                }
            }
            return false;
        }

        class HierarchyKey
        {
            public const int NextSibling = 0;
            public const int PreviousSibling = 1;
            public const int FirstChild = 2;
            public const int Parent = 3;
        }

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

        static private VectorOfVectorOfPoint getContours(string imageFile, out Mat hierachy, out Image<Gray, byte> image)
        {
            image = getPreprocessedImage(imageFile);
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
