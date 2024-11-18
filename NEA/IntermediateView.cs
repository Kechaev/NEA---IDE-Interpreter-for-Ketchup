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
        private Button[] buttons;

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
            txtIntermediateCode.Lines = intermediate;
            txtIntermediateCode.Select(intermediateString.Length,0);
            GenerateButtons();
        }

        private void GenerateButtons()
        {
            int lines = txtIntermediateCode.Lines.Length;
            int counter = 0;
            int yCoord = 10;

            buttons = new Button[lines];
            foreach (string line in intermediate)
            {
                int width = line.Length * 10;
                int height = 18;

                Button button = new Button()
                {
                    Text = $"B{counter + 1}",
                    Width = width,
                    Height = height,
                    Location = new Point(0, yCoord)
                };
                yCoord += height;

                buttons[counter] = button;
                Controls.Add(button);

                counter++;
            }
        }
    }
}
