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

        // Changes the speed of loading the progress bar
        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Enabled = true;
            progressBar.Increment(5);
            if (progressBar.Value >= 200 && progressBar.Value <= 499)
            {
                progressBar.Increment(10);
            }
            if (progressBar.Value == 500 && progressBar.Value <= 899)
            {
                progressBar.Increment(1);
            }
            if (progressBar.Value == 900 && progressBar.Value <= 999)
            {
                progressBar.Increment(10);
            }
            if (progressBar.Value >= 1000)
            {
                timer.Enabled = false;
                IDE_MainWindow form = new IDE_MainWindow();
                form.Show();
                this.Hide();
                //this.Close();
            }
        }
    }
}
