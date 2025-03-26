using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NEA.Classes
{
    public partial class TokenView : Form
    {
        private string[] tokens;
        private ListBox lstBox;

        public TokenView(string[] tokens)
        {
            InitializeComponent();
            this.tokens = tokens;
        }

        private void TokenView_Load(object sender, EventArgs e)
        {
            lstBox = new ListBox();

            lstBox.SelectedIndexChanged += new EventHandler(lstBox_SelectedIndexChanged);

            lstBox.Size = new System.Drawing.Size(200, 100);
            lstBox.Font = new Font("Courier New", 16);

            tableMain.Controls.Add(lstBox,0,0);

            lstBox.Dock = DockStyle.Fill;

            foreach (string line in tokens)
            {
                lstBox.Items.Add(line);
            }

            lstBox.EndUpdate();
            lstBox.SetSelected(0, true);
        }

        private string KeyByValue(Dictionary<string, int> dictionary, int value)
        {
            foreach (KeyValuePair<string, int> pair in dictionary)
            {
                if (pair.Value == value)
                {
                    return pair.Key;
                }
            }
            return null;
        }

        private void lstBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lstBox = sender as ListBox;
            if (lstBox.SelectedIndex != -1)
            {
                string instruction = lstBox.Items[lstBox.SelectedIndex].ToString();
                string[] parts = instruction.Split(' ');

                string opcode = parts[0];
                string operand = "";
                if (parts.Length > 1)
                {
                    operand = parts[1];
                }

                lblName.Text = opcode;
            }
        }
    }
}
