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
        private int[,] map;

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
            
            map = new int[txtIntermediateCode.Lines.Length + 1, 2];

            for (int i = 0; i < txtIntermediateCode.Lines.Length + 1; i++)
            {
                map[i, 0] = i * 18;
                map[i, 1] = i * 18 + 18;
            }
        }

        private void txtIntermediateCode_Click(object sender, EventArgs e)
        {
            Point p = PointToClient(new Point(MousePosition.X, MousePosition.Y));
            for (int i = 0; i < txtIntermediateCode.Lines.Length - 1; i++)
            {
                if (map[i, 0] < p.Y && map[i + 1, 0] > p.Y)
                {
                    lblName.Text = txtIntermediateCode.Lines[i].ToString().Split(' ')[0];
                }
            }

        }
    }
}
