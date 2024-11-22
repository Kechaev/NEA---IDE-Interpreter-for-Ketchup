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
        private Dictionary<string, string> nameDescription = new Dictionary<string, string>()
        {
            {"JUMP_FALSE", "In the case, that the top of the stack is false, then jump the execution to the label mentioned." },
            {"JUMP", "Jumps the execution to the label mentioned" },
            {"ADJUST_TYPE", "Sets the data type of the variable to the mentioned type." },
            {"DECLARE_VAR", "Declares the variable mentioned." },
            {"STORE_VAR", "Stores the value at the top of the stack under the variable mentioned." },
            {"LOAD_VAR", "Loads the mentioned variable onto the stack." },
            {"LOAD_CONST", "Loads the mentioned constant onto the stack." },
            {"CALL", "Calls the function mentioned after the command. This can include built in functions such as PRINTING or INPUTING but can also call custom subroutines." },
            {"GREATER", "Compares the top two items and pushes a boolean value to the stack, decided by whether the first item is greater than the second." },
            {"LESS", "Compares the top two items and pushes a boolean value to the stack, decided by whether the first item is less than the second." },
            {"EQUAL", "Compares the top two items and pushes a boolean value to the stack, decided by whether the first item is equal to the second one." },
            {"GREATER_EQUAL", "Compares the top two items and pushes a boolean value to the stack, decided by whether the first item is greater than or equal to the second one." },
            {"LESS_EQUAL", "Compares the top two items and pushes a boolean value to the stack, decided by whether the first item is less than or equal to the second one." },
            {"NOT_EQUAL", "Compares the top two items and pushes a boolean value to the stack, decided by whether the first item is not equal to the second one." },
            {"ADD", "Adds the top two items off the stack and pushes the result back onto the stack.\r\nAlso responsible for adding characters and strings together, called concatenation." },
            {"SUB", "Performs subtraction on the top two items of the stack and pushes the result back onto the stack." },
            {"MUL", "Multiplies the top two items off the stack and pushes the result back onto the stack." },
            {"DIV", "Performs division on the top two items of the stack and pushes the result back onto the stack." },
            {"EXP", "Performs exponentiation on the top two items of the stack and pushes the result back onto the stack.\r\nExponentiation meaning raising the first number to the power of the second number." },
            {"MOD", "Performs the modulo operation on the top two items off the stack and pushes the result back onto the stack.\r\nThe modulo operator divides the first number by the second and returns the remainder." },
            {"HALT", "Indicates the end of the program and stops the execution of the program." },
        };

        // Add support for explaining what the individual words mean
        public IntermediateView(string[] intermediate)
        {
            InitializeComponent();
            this.intermediate = intermediate;
            txtIntermediateCode.Select();
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
            lblName.Text = "HALT";
            txtDescription.Text = nameDescription["HALT"];
            txtIntermediateCode.Select(txtIntermediateCode.GetFirstCharIndexFromLine(txtIntermediateCode.Lines.Length - 1),4);
            for (int i = 0; i < txtIntermediateCode.Lines.Length - 1; i++)
            {
                if (map[i, 0] < p.Y && map[i + 1, 0] > p.Y)
                {
                    string keyword = txtIntermediateCode.Lines[i].Split(' ')[0];
                    lblName.Text = RemoveUnderscore(keyword);
                    txtDescription.Text = nameDescription[keyword];
                    txtIntermediateCode.Select(txtIntermediateCode.GetFirstCharIndexFromLine(i), txtIntermediateCode.Lines[i].Length);
                }
            }
        }

        private string RemoveUnderscore(string underscored)
        {
            string output = "";
            foreach (char c in underscored)
            {
                if (c == '_')
                {
                    output += " ";
                }
                else
                {
                    output += c;
                }
            }
            return output;
        }
    }
}
