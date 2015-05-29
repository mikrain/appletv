using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AppleTvLiar.AppleChannels.HtmlManager;
using AppleTvService;
using Common;

namespace AppleTvLiarForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Logger.Write("Start Form1");
            try
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                var _appletvLiar = new AppletvLiar();
                _appletvLiar.Start();

              
            }
            catch (Exception exception)
            {
                Logger.Write(exception.Message);
            }

        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

    }
}
