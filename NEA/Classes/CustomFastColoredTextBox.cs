using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NEA.Classes
{
    // https://stackoverflow.com/questions/40016018/c-sharp-make-an-autocomplete-to-a-richtextbox
    // Had to adapt FastColoredTextBox into a hybrid between itself and a RichTextBox
    // Required overriding and addition of methods native to RichTextBox
    class CustomFastColoredTextBox : FastColoredTextBox
    {
        private IntellisensePopUp popUp;
        private ListBox listBox;
        private bool isPopUpShowing;
        private bool isFromReplaceTab;
        private bool isFromMouseClick;
        private int suggestionCount = 5;

        public string[] intellisenseWords;
        public int YOffset;
        public int XOffset;

        public int GetSuggestionCount()
        {
            return suggestionCount;
        }

        public void SetSuggestionCount(int count)
        {
            suggestionCount = count;
        }

        public CustomFastColoredTextBox()
        {
            this.AcceptsTab = true;
            this.YOffset = 30;

            popUp = new IntellisensePopUp();
            popUp.Size = new Size(200, 200);
            popUp.TopMost = true;
            listBox = new ListBox();
            listBox.Dock = DockStyle.Fill;
            popUp.Controls.Add(listBox);

            this.TextChanged += TextBox_TextChanged;
            this.KeyDown += TextBox_KeyDown;
            this.KeyPress += TextBox_KeyPress;
            this.SelectionChanged += TextBox_SelectionChanged;

            listBox.Click += ListBox_Click;
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;

            intellisenseWords = new string[] { "CREATE", "SET", "ADD", "TAKE", "AWAY", "MULTIPLY", "DIVIDE", "GET", "THE", "REMAINDER", "OF",
                                               "MODULO", "IF", "ELSE", "COUNT", "WITH", "FROM", "GOING", "UP", "DOWN", "BY", "WHILE", "DO", "REPEAT", "FOR", "EACH", "IN", "FUNCTION",
                                               "PROCEDURE", "INPUTS", "AS", "TO", "STR_LITERAL", "CHAR_LITERAL", "INT_LITERAL", "DEC_LITERAL", "BOOL_LITERAL", "TRUE", "FALSE",
                                               "LEFT_BRACKET", "RIGHT_BRACKET", "ADD", "SUB", "MUL", "DIV", "MOD", "EXP", "IS", "A", "FACTOR", "MULTIPLE", "THEN", "NEWLINE", "TABSPACE", "TIMES", "DIVIDED", "RAISE", "POWER",
                                               "INPUT", "MESSAGE", "PRINT", "AND", "OR", "NOT", "END", "RETURN", "EOF" };

            popUp.Show();
            popUp.Hide();
        }

        private const int EM_POSFROMCHAR = 0xD6; 

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, int lParam);

        // https://github.com/dotnet/winforms/blob/62ebdb4b0d5cc7e163b8dc9331dc196e576bf162/src/System.Windows.Forms/src/System/Windows/Forms/Controls/RichTextBox/RichTextBox.cs#L2263C9-L2275C25
        // GitHub source code for the GetPositionFromCharIndex method, from the WinForms official repo
        // This is used in context with a RichTextBox, however since this is an overriden FastColoredTextBox
        // We use DLL imports to deal with any unkown keywords
        public unsafe Point GetPositionFromCharIndex(int index)
        {
            if (index < 0 || index > Text.Length)
            {
                return Point.Empty;
            }

            // SendMessage returns an integer which encodes two 16-bit values
            int result = SendMessage(Handle, EM_POSFROMCHAR, IntPtr.Zero, index);
            // Mask out the lower 16-bits as the x coordinate
            int x = result & 0xFFFF;
            // LSL 16 and mask to get the upper 16-bits as the y coordinate
            int y = (result >> 16) & 0xFFFF;
            return new Point(x, y);
        }

        public string[] GetintellisenseWords()
        {
            return intellisenseWords;
        }

        public void SetintellisenseWords(string[] words)
        {
            intellisenseWords = words;
        }

        public void AddIntellisenseWord(string word)
        {
            string[] newWords = new string[intellisenseWords.Length + 1];
            for (int i = 0; i < intellisenseWords.Length; i++)
            {
                newWords[i] = intellisenseWords[i];
            }
            newWords[newWords.Length + 1] = word;
            intellisenseWords = newWords;
        }

        public int GetXOffset()
        {
            return XOffset;
        }
        
        public int GetYOffset()
        {
            return YOffset;
        }

        public void SetXOffset(int XOffset)
        {
            this.XOffset = XOffset;
        }

        public void SetYOffset(int YOffset)
        {
            this.YOffset = YOffset;
        }

        private void TextBox_SelectionChanged(object sender, EventArgs e)
        {
            popUp.Hide();
            isPopUpShowing = false;
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (isFromReplaceTab)
            {
                e.Handled = true;
                isFromReplaceTab = false;
            }
            else
            {
                if (e.KeyChar == (char)Keys.Escape)
                {
                    popUp.Hide();
                    isPopUpShowing = false;
                    e.Handled = true;
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (isPopUpShowing)
            {
                if (e.KeyCode == Keys.Up)
                {
                    if (listBox.Items.Count > 0)
                    {
                        if (listBox.SelectedIndex > 0)
                        {
                            listBox.SelectedIndex--;
                        }
                        e.Handled = true;
                    }
                }
                else if (e.KeyCode == Keys.Down)
                {
                    if (listBox.Items.Count > 0)
                    {
                        if (listBox.SelectedIndex < listBox.Items.Count - 1)
                        {
                            listBox.SelectedIndex++;
                        }
                        e.Handled = true;
                    }
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    if (listBox.SelectedIndex >= 0)
                    {
                        ReplaceCurrentWordWith(listBox.Items[listBox.SelectedIndex].ToString());
                    }
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Tab)
                {
                    if (listBox.SelectedIndex >= 0)
                    {
                        ReplaceCurrentWordWith(listBox.Items[listBox.SelectedIndex].ToString());
                    }
                    e.Handled = true;
                }
            }
        }

        // Unchecked
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (intellisenseWords != null && intellisenseWords.Length > 0)
            {
                string wordText;
                int lastIndexOfSpace;
                int lastIndexOfNewline;
                int lastIndexOfTab;
                int lastIndexOf;
                if (this.SelectionStart == this.Text.Length)
                {
                    if (this.SelectionStart > 0 && this.Text[this.SelectionStart - 1] != ' ' && this.Text[this.SelectionStart - 1] != '\t' && this.Text[this.SelectionStart - 1] != '\n')
                    {
                        wordText = this.Text.Substring(0, this.SelectionStart);
                        lastIndexOfSpace = wordText.LastIndexOf(' ');
                        lastIndexOfNewline = wordText.LastIndexOf('\n');
                        lastIndexOfTab = wordText.LastIndexOf('\t');
                        lastIndexOf = Math.Max(Math.Max(lastIndexOfSpace, lastIndexOfNewline), lastIndexOfTab);
                        if (lastIndexOf >= 0)
                        {
                            wordText = wordText.Substring(lastIndexOf + 1);
                        }
                        if (PopulateIntelliListBox(wordText))
                        {
                            ShowAutoCompleteForm();
                        }
                        else
                        {
                            popUp.Hide();
                            isPopUpShowing = false;
                        }
                    }
                    else
                    {
                        popUp.Hide();
                        isPopUpShowing = false;
                    }
                }
                else
                {
                    char currentChar = this.Text[this.SelectionStart];
                    if (this.SelectionStart > 0)
                    {
                        if (this.SelectionStart > 0 && this.Text[this.SelectionStart - 1] != ' ' && this.Text[this.SelectionStart - 1] != '\t' && this.Text[this.SelectionStart - 1] != '\n'
                            && (this.Text[this.SelectionStart] == ' ' || this.Text[this.SelectionStart] == '\t' || this.Text[this.SelectionStart] == '\n'))
                        {
                            wordText = this.Text.Substring(0, this.SelectionStart);
                            lastIndexOfSpace = wordText.LastIndexOf(' ');
                            lastIndexOfNewline = wordText.LastIndexOf('\n');
                            lastIndexOfTab = wordText.LastIndexOf('\t');
                            lastIndexOf = Math.Max(Math.Max(lastIndexOfSpace, lastIndexOfNewline), lastIndexOfTab);
                            if (lastIndexOf >= 0)
                            {
                                wordText = wordText.Substring(lastIndexOf + 1);
                            }
                            if (PopulateIntelliListBox(wordText))
                            {
                                ShowAutoCompleteForm();
                            }
                            else
                            {
                                popUp.Hide();
                                isPopUpShowing = false;
                            }
                        }
                        else
                        {
                            popUp.Hide();
                            isPopUpShowing = false;
                        }
                    }
                    else
                    {
                        popUp.Hide();
                        isPopUpShowing = false;
                    }
                }
            }
            else
            {
                popUp.Hide();
                isPopUpShowing = false;
            }
        }

        private bool PopulateIntelliListBox(string wordTyping)
        {
            listBox.Items.Clear();

            List<KeyValuePair<int, string>> tempWords = new List<KeyValuePair<int, string>>();
            string[] outArray;

            for (int i = 0; i < intellisenseWords.Length; i++)
            {
                if (intellisenseWords[i].StartsWith(wordTyping, StringComparison.CurrentCultureIgnoreCase))
                {
                    tempWords.Add(new KeyValuePair<int, string>(1, intellisenseWords[i]));
                }
            }

            if (tempWords.Count < suggestionCount && wordTyping.Length > 1)
            {
                for (int i = 0; i < intellisenseWords.Length; i++)
                {
                    if (intellisenseWords[i].Contains(wordTyping))
                    {
                        if (tempWords.Count(c => c.Value == intellisenseWords[i]) == 0)
                        {
                            tempWords.Add(new KeyValuePair<int, string>(2, intellisenseWords[i]));
                        }
                    }
                }
            }

            outArray = tempWords.OrderBy(o => o.Key).Take(suggestionCount).Select(s => s.Value).ToArray();

            listBox.Items.AddRange(outArray);

            return outArray.Length > 0;
        }
        // End Unchecked

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isFromMouseClick)
            {
                if (listBox.SelectedIndex >= 0)
                {
                    ReplaceCurrentWordWith(listBox.SelectedItem.ToString());
                    isFromReplaceTab = false;
                }
            }
            isFromMouseClick = false;
        }

        private void ListBox_Click(object sender, EventArgs e)
        {
            isFromMouseClick = true;
        }

        private void ShowAutoCompleteForm()
        {
            Point tmpPoint = this.PointToClient(this.Location);
            Point tmpSelPoint = this.GetPositionFromCharIndex(this.SelectionStart);
            popUp.Location = new Point((-1 * tmpPoint.X) + tmpSelPoint.X + this.XOffset, (-1 * tmpPoint.Y) + tmpSelPoint.Y + this.Margin.Top + this.YOffset);
            popUp.Show();
            listBox.SelectedIndex = 0;
            isPopUpShowing = true;
            this.Focus();
        }

        private void ReplaceCurrentWordWith(string word)
        {
            int selectionStart = this.SelectionStart;

            string startString = "";
            string endString = this.Text.Substring(selectionStart);
            string wordText = this.Text.Substring(0, selectionStart);
            int lastIndexOfSpace = wordText.LastIndexOf(' ');
            int lastIndexOfNewline = wordText.LastIndexOf('\n');
            int lastIndexOfTab = wordText.LastIndexOf('\t');
            int latestIndexOfNonChar = Math.Max(Math.Max(lastIndexOfSpace, lastIndexOfNewline), lastIndexOfTab);
            if (latestIndexOfNonChar >= 0)
            {
                startString = wordText.Substring(0, latestIndexOfNonChar + 1);
                wordText = wordText.Substring(latestIndexOfNonChar + 1);
            }
            
            this.Text = string.Format("{0}{1} {2}", startString, word, endString);

            if (latestIndexOfNonChar >= 0)
            {
                this.SelectionStart = startString.Length + word.Length + 1;
            }
            else
            {
                this.SelectionStart = word.Length + 1;
            }

            isFromReplaceTab = true;
        }
    }
}
