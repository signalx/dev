using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StepRunner
{
    public partial class Form1 : Form
    {
        public string url { set; get; }

        public Form1(string u)
        {
            url = u;
            InitializeComponent();
        }

       // private WebBrowser webBrowser1;

        private void Form1_Load(object sender, EventArgs e)
        {
            //  this.webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.DocumentText = "0";
            webBrowser1.Document.OpenNew(true);
            webBrowser1.Document.Write(url);
            webBrowser1.Refresh();
            //webBrowser1 = new System.Windows.Forms.WebBrowser();
            //this.webBrowser1.Dock = DockStyle.Fill;

            //this.Controls.Add(this.webBrowser1);
            //this.webBrowser1.ScriptErrorsSuppressed = true;

            //webBrowser1.Navigate(url);
            //webBrowser1.Refresh(WebBrowserRefreshOption.Completely);
            //Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(x => { this.Close(); });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew(() => { })
                .ContinueWith(
                    r =>
                    {
                    }, scheduler);
        }
    }
}