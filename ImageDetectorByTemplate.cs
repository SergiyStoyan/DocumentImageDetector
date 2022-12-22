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
using System.Text.RegularExpressions;

namespace Cliver.testImageDetection
{
    class ImageDetectorByTemplate
    {
        static public void Clear(ref Bitmap bitmap)
        {
            using (Image<Gray, byte> image = bitmap.ToImage<Gray, byte>())
            //using (Image<Rgb, byte> image = bitmap.ToImage<Rgb, byte>())
            {
                bitmap.Dispose();
                var i = image;

                //i._EqualizeHist();
                //i._GammaCorrect(0.2);

                CvInvoke.BitwiseNot(i, i);
                //i = i.ThresholdBinary(new Gray(255), new Gray(255));
                i = i.ThresholdToZero(new Gray(100));
                CvInvoke.BitwiseNot(i, i);
                //i = i.ThresholdAdaptive(new Rgb(255, 255, 255), AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 155, new Rgb(30, 30, 30));
                //i = i.ThresholdAdaptive(new Gray(200), AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 5, new Gray(3));
                //CvInvoke.AdaptiveThreshold(i, i, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 55, 30);
                //CvInvoke.Dilate(i, i, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                bitmap = i.ToBitmap();
                //pageRgbImage.Save(Regex.Replace(pageFile, @"","", RegexOptions.IgnoreCase);
            }
        }

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

        static public void FindMatches(string pageFile, string templateFile, Size padding)
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
                        Dictionary<Rectangle, Match> paddedMatchRs2bestMatchP = new Dictionary<Rectangle, Match>();

                        for (int x = matches.GetLength(1) - 1; x >= 0; x--)
                        {
                            for (int y = matches.GetLength(0) - 1; y >= 0; y--)
                            {
                                double score = matches[y, x, 0];
                                //if (score < 0.003)//SqdiffNormed
                                if (score > 0.70)//CcoeffNormed
                                {
                                    Rectangle r = new Rectangle(new Point(x, y), templateImage.Size);
                                    var kv = paddedMatchRs2bestMatchP.FirstOrDefault(a => a.Key.Contains(r));
                                    if (kv.Key == Rectangle.Empty)
                                    {
                                        Rectangle ar = new Rectangle(r.Location, r.Size);
                                        ar.Inflate(padding);
                                        paddedMatchRs2bestMatchP[ar] = new Match { Rectangle = r, Score = score };
                                    }
                                    else
                                    {
                                        if (kv.Value.Score < score)
                                            paddedMatchRs2bestMatchP[kv.Key] = new Match { Rectangle = r, Score = score };
                                    }
                                }
                            }
                        }
                        foreach (Match m in paddedMatchRs2bestMatchP.Values)
                            pageRgbImage.Draw(m.Rectangle, new Rgb(255, 0, 0), 1);
                    }
                }
            }
            MainForm.This.PageBox.Image = pageRgbImage.ToBitmap();
            MainForm.This.TemplateBox.Image = templateRgbImage.ToBitmap();
        }

        class Match
        {
            public Rectangle Rectangle;
            public double Score;
        }

        static private Image<Gray, byte> getPreprocessedImage(string imageFile)
        {
            Image<Gray, byte> image = new Image<Gray, byte>(imageFile);
            //Emgu.CV.CvInvoke.Blur(image, image, new Size(10, 10), new Point(0, 0));
            //Emgu.CV.CvInvoke.Threshold(image, image, 60, 255, ThresholdType.Otsu | ThresholdType.Binary);
            //Emgu.CV.CvInvoke.Erode(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //CvInvoke.Dilate(image, image, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            //CvInvoke.Canny(image, image, 100, 30, 3);
            return image;
        }
    }
}
