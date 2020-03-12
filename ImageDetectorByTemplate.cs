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
    class ImageDetectorByTemplate
    {
        static public bool FindMatch(string pageFile, string templateFile)
        {
            Image<Rgb, byte> pageRgbImage = new Image<Rgb, byte>(pageFile);
            Image<Rgb, byte> templateRgbImage = new Image<Rgb, byte>(templateFile);
            bool result = false;
            using (Image<Gray, byte> pageImage = getPreprocessedImage(pageFile))
            {
                using (Image<Gray, byte> templateImage = getPreprocessedImage(templateFile))
                {
                    using (Image<Gray, float> match = pageImage.MatchTemplate(templateImage, TemplateMatchingType.CcoeffNormed))
                    {
                        match.MinMax(out double[] min, out double[] max, out Point[] minPoint, out Point[] maxPoint);

                        if (max[0] > 0.70)
                        {
                            pageRgbImage.Draw(new Rectangle(maxPoint[0], templateImage.Size), new Rgb(255, 0, 0), 1);
                            result = true;
                        }
                    }
                }
            }
            MainForm.This.PageBox.Image = pageRgbImage.ToBitmap();
            MainForm.This.TemplateBox.Image = templateRgbImage.ToBitmap();
            return result;
        }

        static private Image<Gray, byte> getPreprocessedImage(string imageFile)
        {
            Image<Gray, byte> image = new Image<Gray, byte>(imageFile);
            Emgu.CV.CvInvoke.Blur(image, image, new Size(10, 10), new Point(0, 0));
            //Emgu.CV.CvInvoke.Threshold(image, image, 60, 255, ThresholdType.Otsu | ThresholdType.Binary);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //CvInvoke.Dilate(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //CvInvoke.Canny(image, image, 100, 30, 3);
            return image;
        }
    }
}
