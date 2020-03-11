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
              //ImageDetectorByContour.FindMatch(@"c:\\temp\mf.png", @"c:\\temp\ppt.png");
                ImageDetectorByKeyPoints.FindMatch(@"c:\\temp\pp3.png", @"c:\\temp\ppt.png");
            };
        }

        public static readonly MainForm This;
    }
}
