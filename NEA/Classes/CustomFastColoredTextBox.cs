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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NEA.Classes
{
    // https://stackoverflow.com/questions/40016018/c-sharp-make-an-autocomplete-to-a-richtextbox
    // Had to adapt FastColoredTextBox into a hybrid between itself and a RichTextBox
    // Required overriding and addition of methods native to RichTextBox
    class CustomFastColoredTextBox : FastColoredTextBox
    {
        private popUp popUp;
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

            popUp = new popUp();
            popUp.Size = new Size(100, 40);
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

            // Add support for variables and function names

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
                        if (PopulateListBox(wordText))
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
                            if (PopulateListBox(wordText))
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

        private bool PopulateListBox(string wordTyping)
        {
            listBox.Items.Clear();

            int wordTypingLength = wordTyping.Length;

            List<KeyValuePair<int, string>> tempWords = new List<KeyValuePair<int, string>>();
            
            foreach (string word in intellisenseWords)
            {
                if (word.StartsWith(wordTyping.ToUpper()))
                {
                    tempWords.Add(new KeyValuePair<int, string>(1, word));
                }
            }

            // No keywords have the same beginning
            // Resort to minimal Levenshtein distance
            if (tempWords.Count > 0)
            {

                // For small lengths of wordTyping use recursive algorithm
                if (wordTypingLength < 3)
                {
                    foreach (string word in intellisenseWords)
                    {
                        if (word.Length < 3)
                        {

                        }
                    }
                }
            }

            string[] outArray;

            // Sort the following using the key first and then by levenshtein distance
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
        
        // Wagner Fischer Approach to the Levenshtein Algorithm - Dynamic implementation
        private int LevenshteinRecursiveenshteinDynamic(string word1, string word2)
        {
            int word1Length = word1.Length;
            int word2Length = word2.Length;
            if (word1Length > 0 && word2Length > 0)
            {
                int[,] matrix = new int[word2Length + 1, word1Length + 1];

                int longestLenth = Math.Max(word1Length, word2Length);

                for (int i = 0; i <= longestLenth; i++)
                {
                    if (i <= word1Length)
                    {
                        matrix[0, i] = i;
                    }
                    if (i <= word2Length)
                    {
                        matrix[i, 0] = i;
                    }
                }

                for (int j = 1; j <= word2Length; j++)
                {
                    for (int i = 1; i <= word1Length; i++)
                    {
                        // Deletion
                        int aboveCell = matrix[j - 1, i];
                        // Insertion
                        int leftCell = matrix[j, i - 1];
                        // Substitution
                        int diagonalCell = matrix[j - 1, i - 1];

                        if (word1[i - 1] != word2[j - 1])
                        {
                            matrix[j, i] = 1 + Min(aboveCell, diagonalCell, leftCell);
                        }
                        else
                        {
                            matrix[j, i] = diagonalCell;
                        }
                    }
                }

                return matrix[matrix.GetLength(0) - 1, matrix.GetLength(1) - 1];
            }
            return -1;
        }

        // Levenshtein Algoithm (Original) - Recursive implementation

        static int LevenshteinRecursive(string a, string b)
        {
            if (Length(b) == 0)
            {
                return Length(a);
            }
            else if (Length(a) == 0)
            {
                return Length(b);
            }
            else if (Head(a) == Head(b))
            {
                return LevenshteinRecursive(Tail(a), Tail(b));
            }
            int delete = LevenshteinRecursive(Tail(a), b);
            int insert = LevenshteinRecursive(a, Tail(b));
            int substitute = LevenshteinRecursive(Tail(a), Tail(b));

            int minimum = Min(delete, insert, substitute);
            return 1 + minimum;
        }

        static int Length(string word)
        {
            return word.Length;
        }

        static string Head(string word)
        {
            return word[0].ToString();
        }

        static string Tail(string word)
        {
            string tail = "";
            for (int i = 1; i < word.Length; i++)
            {
                tail += word[i];
            }
            return tail;
        }

        static int Min(int a, int b, int c)
        {
            return Math.Min(a, Math.Min(b, c));
        }

        static int Difference(int a, int b)
        {
            return Math.Abs(a - b);
        }

        // Personal implementation of the built in methods for RichTextBox
        // Which are not available for FastColoredTextBox
        public int GetLinesFromCharIndex(int selectionStart)
        {
            string text = this.Text;
            int line = 0;
            for (int i = 0; i < selectionStart && i < this.Text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                }
            }
            return line;
        }

        public string[] GetLinesFromTextBox()
        {
            List<string> linesList = new List<string>();

            string lastLine = "";

            foreach (char c in this.Text)
            {
                if (c == '\n')
                {
                    linesList.Add(lastLine);
                    lastLine = "";
                }
                else
                {
                    lastLine += c;
                }
            }

            linesList.Add(lastLine);

            return linesList.ToArray();
        }

        public int GetFirstCharIndexOfLine(int selectedLine)
        {
            int line = 0;
            string[] lines = GetLinesFromTextBox();
            for (int i = 0; i < this.Text.Length; i++)
            {
                char c = this.Text[i];
                if (c == '\n')
                {
                    line++;

                }
                if (line == selectedLine)
                {
                    if (i == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        return i + 1;
                    }
                }
            }
            return -1;
        }
    }
}
