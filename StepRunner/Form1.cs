using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepRunner
{
    public partial class Form1 : Form
    {
        public string url { set; get; }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Task.Delay(TimeSpan.FromSeconds(3)).ContinueWith(x =>
            {
                webBrowser1.Navigate(url);
                webBrowser1.Refresh(WebBrowserRefreshOption.Completely);
            });
            timer1.Enabled = false;
        }
    }
}