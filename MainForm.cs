using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testImageDetection
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
                //ImageDetectorByContour idbc = new ImageDetectorByContour(@"c:\\temp\t1.png");
                //idbc.FindOnPage(@"c:\\temp\test1.png");
                //ImageDetectorByKeyPoints.FindMatch(@"c:\\temp\pp3.png", @"c:\\temp\ppt.png");
                //ImageDetectorByTemplate.FindMatch(@"c:\\temp\test1.png", @"c:\\temp\t1.png");
                //ImageDetectorByTemplate.FindMatches(@"c:\\temp\test1.png", @"c:\\temp\t1.png", new Size(20,20));
                ImageDeskew.Deskew(@"c:\\temp\2.png");
            };
        }

        public static readonly MainForm This;
    }
}
