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
    class ImageDetectorByKeyPoints
    {
        static private Image<Rgb, byte> getPreprocessedImage(string imageFile)
        {
            Image<Rgb, byte> image = new Image<Rgb, byte>(imageFile);
            //Emgu.CV.CvInvoke.Blur(image, image, new Size(10, 10), new Point(0, 0));
            //Emgu.CV.CvInvoke.Threshold(image, image, 60, 255, ThresholdType.Otsu | ThresholdType.Binary);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Dilate(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //CvInvoke.Canny(image, image, 100, 30, 3);
            return image;
        }

        public static void FindMatch(string pageFile, string templateFile)
        {
            Image<Rgb, byte> page = getPreprocessedImage(pageFile);
            Image<Rgb, byte> template = getPreprocessedImage(templateFile);

            var detector = new ORBDetector();
            VectorOfKeyPoint templateKeyPoints = new VectorOfKeyPoint();
            Mat templateDescriptors = new Mat();
            detector.DetectAndCompute(template, null, templateKeyPoints, templateDescriptors, false);

            VectorOfKeyPoint pageKeyPoints = new VectorOfKeyPoint();
            Mat pageDescriptors = new Mat();
            detector.DetectAndCompute(page, null, pageKeyPoints, pageDescriptors, false);
            using (var matcher = new BFMatcher(DistanceType.L1))
            {
                matcher.Add(templateDescriptors);
                VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();

                //VectorOfDMatch matches2 = new VectorOfDMatch();
                //matcher.Match(pageDescriptors, matches2);


                matcher.KnnMatch(pageDescriptors, matches, 2, null);

                Mat            mask = new Mat(matches.Size, 1, DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(255));
                Features2DToolbox.VoteForUniqueness(matches, 0.8, mask);
                Mat homography = new Mat();
                int nonZeroCount = CvInvoke.CountNonZero(mask);
                if (nonZeroCount >= 4)
                {
                    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(templateKeyPoints, pageKeyPoints, matches, mask, 1.5, 20);
                    if (nonZeroCount >= 4)
                        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(templateKeyPoints, pageKeyPoints, matches, mask, 2);
                }

                Mat result = new Mat();
                Features2DToolbox.DrawMatches(template, templateKeyPoints, page, pageKeyPoints, matches, result, new MCvScalar(0, 255, 0), new MCvScalar(255, 0, 0), mask, Features2DToolbox.KeypointDrawType.NotDrawSinglePoints);

                //Features2DToolbox.DrawMatches(template, templateKeyPoints, page, pageKeyPoints, matches2, result, new MCvScalar(0, 255, 0), new MCvScalar(255, 0, 0), null, Features2DToolbox.KeypointDrawType.NotDrawSinglePoints);

                MainForm.This.PageBox.Image = result.ToBitmap();
            }
        }
    }
}
