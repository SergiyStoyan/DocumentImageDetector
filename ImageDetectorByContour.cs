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
        public static void FindMatch(string imageFile, string templateFile)
        {
            VectorOfKeyPoint modelKeyPoints; VectorOfKeyPoint observedKeyPoints; VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch(); Mat mask; Mat homography;

            Mat modelImage = new Mat(templateFile);
            Mat observedImage = new Mat(imageFile);

            homography = null;
          const  int k = 2;
            const double uniquenessThreshold = 0.90;
            using (UMat uModelImage = modelImage.GetUMat(AccessType.Read))
            using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
            {
                var featureDetector = new ORBDetector();
                Mat modelDescriptors = new Mat();
                modelKeyPoints = new VectorOfKeyPoint();
                featureDetector.DetectAndCompute(uModelImage, null, modelKeyPoints, modelDescriptors, false);
                Mat observedDescriptors = new Mat();
                observedKeyPoints = new VectorOfKeyPoint();
                featureDetector.DetectAndCompute(uObservedImage, null, observedKeyPoints, observedDescriptors, false);
                using (BFMatcher matcher = new BFMatcher(DistanceType.Hamming, false))
                {                   
                    matcher.Add(modelDescriptors);

                    matcher.KnnMatch(observedDescriptors, matches, k);
                    mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                    mask.SetTo(new MCvScalar(255));
                    Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);

                   // matches[0][0].Distance

                    int nonZeroCount = CvInvoke.CountNonZero(mask);
                    if (nonZeroCount >= 4)
                    {
                        nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, observedKeyPoints, matches, mask, 1.5, 20);
                        if (nonZeroCount >= 4)
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints,
                                observedKeyPoints, matches, mask, 2);
                    }
                }
            }
        }

        static private bool DetectSimilarImage()
        {
            using (Image<Gray, byte> inputImage = new Image<Gray, byte>(@"c:\\temp\pp3.png"))
            {
                using (Image<Gray, byte> templateImage = new Image<Gray, byte>(@"c:\\temp\ppt.png"))
                {
                    using (Image<Gray, float> match = inputImage.MatchTemplate(templateImage, TemplateMatchingType.Ccoeff))
                    {
                        //match.ROI = new Rectangle(templateImage.Width, templateImage.Height, inputImage.Width, inputImage.Height);

                        Point[] MAX_Loc, Min_Loc;
                        double[] min, max;
                        match.MinMax(out min, out max, out Min_Loc, out MAX_Loc);

                        using (Image<Gray, double> RG_Image = match.Convert<Gray, double>().Copy())
                        {
                            if (max[0] > 0.85)
                            {
                                //Object_Location = MAX_Loc[0];
                                return true;
                            }
                        }

                    }
                }
            }
            return false;
        }

        static public bool DetectSimilarImage2(string imageFile, string templateFile)
        {
            VectorOfVectorOfPoint imageContours = getContours(imageFile, out Mat imageContoursHierachy, out Image<Gray, byte> image);
           // Image<Gray, byte> justCountor = new Image<Gray, byte>(384, 284, new Gray(255)); 
            MainForm.This.PageBox.Image = drawContours(image, imageContours);

             VectorOfVectorOfPoint templateContours = getContours(templateFile, out Mat templateContoursHierachy, out Image<Gray, byte> template);
            Emgu.CV.CvInvoke.DrawContours(template, templateContours, -1, new MCvScalar(255, 0, 0), 1);
            MainForm.This.TemplateBox.Image = drawContours(template, templateContours);

            return false;
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
            image = new Image<Gray, byte>(imageFile);
            //Emgu.CV.CvInvoke.Blur(image, image, new Size(10, 10), new Point(0, 0));
            //Emgu.CV.CvInvoke.Threshold(image, image, 60, 255, ThresholdType.Otsu | ThresholdType.Binary);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Dilate(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Canny(image, image, 100, 30, 3);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            hierachy = new Mat();
            Emgu.CV.CvInvoke.FindContours(image, contours, hierachy, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            return contours;
        }
    }
}
