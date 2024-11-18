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
        private Panel[] panels;

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
            }

            if (maxLength > 20)
            {
                this.Width = maxLength * 10 + 35;
                txtIntermediateCode.Width = maxLength * 10;
                txtDescription.Width = maxLength * 10;
            }
            txtIntermediateCode.TabIndex = 1;
            txtIntermediateCode.Lines = intermediate;
            txtIntermediateCode.Select(intermediateString.Length,0);
            GenerateButtons();
        }

        private void GenerateButtons()
        {
            int lines = txtIntermediateCode.Lines.Length;
            int counter = 0;
            int yCoord = 10;

            panels = new Panel[lines];
            foreach (string line in intermediate)
            {
                int width = line.Length * 10;
                int height = 18;
                Panel panel = new Panel()
                {
                    Text = $"{line}",
                    Width = width,
                    Height = height,
                    Location = new Point(10, yCoord),
                    TabIndex = 0,
                    BorderStyle = 0,
                    BackColor = Color.Transparent,
                    ForeColor = Color.Transparent,
                    //Visible = false
                };
                // ???
                panel.Click += Panel_Click();
                yCoord += height;

                panels[counter] = panel;
                Controls.Add(panel);

                panel.BringToFront();

                counter++;
            }
        }

        private void Panel_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Panel clicked");
            throw new NotImplementedException();
        }
    }
}
