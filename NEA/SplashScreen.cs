using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NEA
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
            progressBar.ForeColor = Color.Red;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Enabled = true;
            progressBar.Increment(2);
            if (progressBar.Value >= 100)
            {
                timer.Enabled = false;
                IDE_MainWindow form = new IDE_MainWindow();
                form.Show();
                this.Hide();
            }
        }
    }
}
