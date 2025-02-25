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
    public partial class InputDialog : Form
    {
        private string inputPrompt;
        public InputDialog(string inputPrompt)
        {
            InitializeComponent();
            this.inputPrompt = inputPrompt;
            lblPrompt.Text = inputPrompt;
            txtInputBox.Focus();
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            if (txtInputBox.Text == "")
            {
                lblPrompt.Visible = true;
            }
            else
            {
                lblPrompt.Visible = false;
            }
        }

        private void txtInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                this.DialogResult = DialogResult.OK;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                this.DialogResult = DialogResult.Cancel;
            }
        }

        public string GetUserInput()
        {
            return txtInputBox.Text;
        }
    }
}
