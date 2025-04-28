using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NEA
{
    public partial class IntermediateView : Form
    {
        // Think of a new implementation
        // Doesn't work for scrollable text
        private string[] intermediate;
        private List<string[]> subroutineIntermediate;
        private Dictionary<string, int> subroutineDict;
        private Dictionary<string, string> nameDescription = new Dictionary<string, string>()
        {
            {"LABEL", "A point to which the program can jump to." },
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
            {"RETURN", "Takes the top value of the stack and goes back to the main branch." },
            {"STORE_LIST_ITEM", "Adds the item at the top of the stack to the list variable referenced." },
            {"REMOVE_LIST_ITEM", "Removes the item at the top of the stack from the list variable referenced." },
            {"CREATE_LIST", "Converts the variable referenced into a list, making it an empty list." },
            {"SORT", "Sorts the list variable referenced and updates the order of the list to be sorted." },
            {"LENGTH", "Pushes the length of the variable referenced." },
            {"SWAP", "Takes the top 3 values of the stack, the top number references the list variable and the other two values represent the indexes to be sorted." },
            {"HALT", "Indicates the end of the program and stops the execution of the program." },
        };

        private ListBox lstBox;

        public IntermediateView(string[] intermediate, List<string[]> subroutineIntermediate, Dictionary<string, int> subroutineDict)
        {
            InitializeComponent();
            this.intermediate = intermediate;
            this.subroutineIntermediate = subroutineIntermediate;
            this.subroutineDict = subroutineDict;
        }

        // Changes size and adds events to the list box
        private void IntermediateView_Load(object sender, EventArgs e)
        {
            // Finds the max line length
            int maxLength = 0;
            
            foreach (string line in intermediate)
            {
                int length = line.Length;
                if (length > maxLength)
                {
                    maxLength = length;
                }
            }

            if (maxLength > 30)
            {
                this.Width = maxLength * 10 + 35;
                txtDescription.Width = maxLength * 10;
            }

            lstBox = new ListBox();

            lstBox.SelectedIndexChanged += new EventHandler(lstBox_SelectedIndexChanged);

            lstBox.Size = new System.Drawing.Size(200, 100);
            lstBox.Font = new Font("Courier New", 16);

            tabMain.Controls.Add(lstBox);

            lstBox.Dock = DockStyle.Fill;

            foreach (string line in intermediate)
            {
                lstBox.Items.Add(line);
            }

            lstBox.EndUpdate();
            lstBox.SetSelected(0,false);

            // Adds tabs for other subroutines
            for (int i = 0; i < subroutineIntermediate.Count; i++)
            {
                TabPage tp = new TabPage($"Subroutine {KeyByValue(subroutineDict, i)}");
                tabControlIntermediate.Controls.Add(tp);

                ListBox lstBox = new ListBox();

                lstBox.SelectedIndexChanged += new EventHandler(lstBox_SelectedIndexChanged);

                lstBox.Dock = DockStyle.Fill;
                lstBox.Font = new Font("Courier New", 16);
                foreach (string line in subroutineIntermediate[i])
                {
                    lstBox.Items.Add(line);
                }

                tp.Controls.Add(lstBox);
            }
        }

        // Removes underscore from the string provided
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

        // Returns the key from the value in the dictionary
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

        // Changes the information displayed in the lblName and lblDescription to give info about the selected instruction
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
                try
                {
                    txtDescription.Text = nameDescription[opcode];
                }
                catch
                {
                    MessageBox.Show("An error occurred when trying to read instruction.");
                }
            }
            else
            {
                txtDescription.Text = "";
            }
        }
    }
}
