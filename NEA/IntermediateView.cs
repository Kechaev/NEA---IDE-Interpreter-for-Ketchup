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
    public partial class IntermediateView : Form
    {
        private string[] intermediate;

        // Add support for explaining what the individual words mean
        public IntermediateView(string[] intermediate)
        {
            InitializeComponent();
            this.intermediate = intermediate;
        }

        private void IntermediateView_Load(object sender, EventArgs e)
        {
            string intermediateString = "";
            int maxLength = 0;

            foreach (string line in intermediate)
            {
                int length = line.Length;
                if (length > maxLength)
                {
                    maxLength = length;
                }
                intermediateString += $"{line}\r\n";
            }

            MessageBox.Show($"max = {maxLength}");

            if (maxLength > 20)
            {
                this.Width = maxLength * 12 + 30;
                txtIntermediateCode.Width = maxLength * 12;
            }
            txtIntermediateCode.Text = intermediateString;
            txtIntermediateCode.Select(intermediateString.Length,0);
        }
    }
}
