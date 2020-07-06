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

        static public void FindMatchs(string pageFile, string templateFile, Size padding)
        {
            Image<Rgb, byte> pageRgbImage = new Image<Rgb, byte>(pageFile);
            Image<Rgb, byte> templateRgbImage = new Image<Rgb, byte>(templateFile);
            using (Image<Gray, byte> pageImage = getPreprocessedImage(pageFile))
            {
                using (Image<Gray, byte> templateImage = getPreprocessedImage(templateFile))
                {
                    using (Image<Gray, float> match = pageImage.MatchTemplate(templateImage, TemplateMatchingType.CcoeffNormed))
                    {
                        float[,,] matches = match.Data;
                        List<Rectangle> matchRs = new List<Rectangle>();
                        for (int x = matches.GetLength(1) - 1; x >= 0; x--)
                        {
                            for (int y = matches.GetLength(0) - 1; y >= 0; y--)
                            {
                                double matchScore = matches[y, x, 0];
                                if (matchScore > 0.91)
                                {
                                    Rectangle r = new Rectangle(new Point(x, y), templateImage.Size);
                                    var d = matchRs.FirstOrDefault(a => a.Contains(r));
                                    if (matchRs.FirstOrDefault(a => a.Contains(r)) == Rectangle.Empty)
                                    {
                                        pageRgbImage.Draw(r, new Rgb(255, 0, 0), 1);
                                        r.Inflate(padding);
                                        matchRs.Add(r);
                                    }
                                }

                            }
                        }
                    }
                }
            }
            MainForm.This.PageBox.Image = pageRgbImage.ToBitmap();
            MainForm.This.TemplateBox.Image = templateRgbImage.ToBitmap();
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
