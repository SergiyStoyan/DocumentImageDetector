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
        public static void FindMatch(string imageFile, string templateFile)
        {
            VectorOfKeyPoint modelKeyPoints; VectorOfKeyPoint observedKeyPoints; VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch(); Mat mask; Mat homography;

            Mat modelImage = new Mat(templateFile);
            Mat observedImage = new Mat(imageFile);

            homography = null;
            const int k = 2;
            const double uniquenessThreshold = 0.90;
            using (UMat uModelImage = modelImage.GetUMat(AccessType.Read))
            using (UMat uObservedImage = observedImage.GetUMat(AccessType.Read))
            {
                var featureDetector = new ORBDetector();// ORBDetector();
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
                            homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, observedKeyPoints, matches, mask, 2);
                    }
                }
            }
        }
    }
}
