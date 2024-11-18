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

        public TokenView(string[] tokens)
        {
            InitializeComponent();
            this.tokens = tokens;
        }

        private void TokenView_Load(object sender, EventArgs e)
        {
            txtTokenView.Lines = tokens;
        }
    }
}
