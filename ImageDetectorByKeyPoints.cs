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
        static private Image<Gray, byte> getPreprocessedImage(string imageFile)
        {
            Image<Gray, byte> image = new Image<Gray, byte>(imageFile);
            //Emgu.CV.CvInvoke.Blur(image, image, new Size(10, 10), new Point(0, 0));
            //Emgu.CV.CvInvoke.Threshold(image, image, 60, 255, ThresholdType.Otsu | ThresholdType.Binary);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Dilate(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //CvInvoke.Canny(image, image, 100, 30, 3);
            return image;
        }

        public static void FindMatch(string pageFile, string templateFile)
        {
            VectorOfKeyPoint modelKeyPoints; VectorOfKeyPoint observedKeyPoints; VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch(); Mat mask; Mat homography;

            Image<Gray, byte> page = getPreprocessedImage(pageFile);
            Image<Gray, byte> template = getPreprocessedImage(templateFile);

            //Mat modelImage = new Mat(templateFile);
            //Mat observedImage = new Mat(pageFile);

            homography = null;
            const int k = 2;
            const double uniquenessThreshold = 0.90;
            {
                var featureDetector = new ORBDetector();// ORBDetector();
                Mat modelDescriptors = new Mat();
                modelKeyPoints = new VectorOfKeyPoint();
                featureDetector.DetectAndCompute(template, null, modelKeyPoints, modelDescriptors, false);
                Mat observedDescriptors = new Mat();
                observedKeyPoints = new VectorOfKeyPoint();
                featureDetector.DetectAndCompute(page, null, observedKeyPoints, observedDescriptors, false);
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
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, matches, mask, 2);
                    }
                }
            }
        }

        public static void FindMatch2(string pageFile, string templateFile)
        {

            Image<Gray, byte> page = getPreprocessedImage(pageFile);
            Image<Gray, byte> template = getPreprocessedImage(templateFile);

            FastFeatureDetector fastFeatureDetector = new FastFeatureDetector(15);
            VectorOfKeyPoint templateKeyPoints = new VectorOfKeyPoint();
            Mat templateDescriptors = new Mat();
            fastFeatureDetector.DetectAndCompute(template, null, templateKeyPoints, templateDescriptors, false);

            VectorOfKeyPoint pageKeyPoints = new VectorOfKeyPoint();
            Mat pageDescriptors = new Mat();
            fastFeatureDetector.DetectAndCompute(page, null, pageKeyPoints, pageDescriptors, false);

            using (BFMatcher matcher = new BFMatcher(DistanceType.L2))
            {
                matcher.Add(templateDescriptors);
                VectorOfDMatch matches = new VectorOfDMatch();
                matcher.Match(pageDescriptors, matches);

                //const double uniquenessThreshold = 0.90;
                //Mat mask;
                //Features2DToolbox.VoteForUniqueness(matches, uniquenessThreshold, mask);                    
                //int nonZeroCount = CvInvoke.CountNonZero(mask);
                //if (nonZeroCount >= 4)
                //{
                //    nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(templateKeyPoints, pageKeyPoints, matches, mask, 1.5, 20);
                //Mat homography = null;
                //    if (nonZeroCount >= 4)
                //        homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(templateKeyPoints, pageKeyPoints, matches, mask, 2);
                //}
            }
        }
    }
}
