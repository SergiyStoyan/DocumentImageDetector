using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cliver.testImageDetection
{
    public partial class MainForm : Form
    {
        static MainForm()
        {
            This = new MainForm();
        }

        public MainForm()
        {
            InitializeComponent();

            Load += delegate {
                //Bitmap b = new Bitmap(@"c:\\temp\5\r.bmp");
                Bitmap b = new Bitmap(@"c:\\temp\5\test.jpg");
                //ImageDetectorByContour idbc = new ImageDetectorByContour(@"c:\\temp\t1.png");
                //idbc.FindOnPage(@"c:\\temp\test1.png");
                //ImageDetectorByKeyPoints.FindMatch(@"c:\\temp\pp3.png", @"c:\\temp\ppt.png");
                //ImageDetectorByTemplate.FindMatch(@"c:\\temp\test1.png", @"c:\\temp\t1.png");
                //ImageDetectorByTemplate.FindMatches(@"c:\\temp\test1.png", @"c:\\temp\t1.png", new Size(20,20));
                //Deskewer.DeskewAsColumnOfBlocks(ref b, 1000, 30);
                ImageDetectorByTemplate.Clear(ref b);
                MainForm.This.PageBox.Image = b;

               // b.Save(@"c:\temp\5\_r.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            };
        }

        public static readonly MainForm This;
    }
}
