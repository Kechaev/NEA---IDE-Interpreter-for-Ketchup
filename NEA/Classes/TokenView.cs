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
        private Token[] tokens;

        public TokenView(Token[] tokens)
        {
            InitializeComponent();
            this.tokens = tokens;
        }

        private void TokenView_Load(object sender, EventArgs e)
        {
            string tokenString = "";
            int maxLength = 0;

            foreach (Token token in tokens)
            {
                tokenString += token.GetTokenType() + "\r\n";
                int length = token.GetTokenType().ToString().Length;
                if (length > maxLength)
                {
                    maxLength = length;
                }
            }

            if (maxLength > 20)
            {
                this.Width = maxLength * 10 + 35;
                txtTokens.Width = maxLength * 10;
                txtDescription.Width = maxLength * 10;
            }
        }
    }
}
