﻿using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NEA.Classes
{
    // Autocompletion box and tab to insert selected word
    // Base events are taken from this StackOverflow post
    // Implemented my own algorithm for detecting closest words, when words are not found
    // with the same beginning string
    // https://stackoverflow.com/questions/40016018/c-sharp-make-an-autocomplete-to-a-richtextbox

    // Had to adapt FastColoredTextBox into a hybrid between itself and a RichTextBox
    // Required overriding and addition of methods native to RichTextBox
    // Added Levenshtein Algorithm for cases where there are no words with the same initial characters
    class CustomFastColoredTextBox : FastColoredTextBox
    {
        private popUp popUp;
        private ListBox listBox;
        private TableLayoutPanel tableLayout;
        private bool isPopUpShowing;
        private bool isFromReplaceTab;
        private bool isFromMouseClick;
        private int suggestionCount = 5;

        public string[] suggestionDictionary;
        public int YOffset;
        public int XOffset;

        private List<string> identifierNames;

        public CustomFastColoredTextBox()
        {
            this.AcceptsTab = true;
            this.YOffset = 30;

            popUp = new popUp();
            popUp.TopMost = true;
            tableLayout = popUp.Controls[0] as TableLayoutPanel;
            listBox = new ListBox();
            listBox.Size = new Size(160, 54);
            tableLayout.Controls.Add(listBox);

            // Adding events to newly created text box
            this.TextChanged += TextBox_TextChanged;
            this.KeyDown += TextBox_KeyDown;
            this.KeyPress += TextBox_KeyPress;
            this.SelectionChanged += TextBox_SelectionChanged;

            // Events for interacting with the pop up
            listBox.Click += ListBox_Click;
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;

            // Dictionary of potential words to suggest
            suggestionDictionary = new string[] { "CREATE", "SET", "ADD", "TAKE", "AWAY", "MULTIPLY",
                                               "DIVIDE", "GET", "REMAINDER", "OF", "IF", "ELSE", "COUNT", "WITH", 
                                               "FROM", "GOING", "UP", "DOWN", "BY", "WHILE", "DO", "REPEAT", "FOR", 
                                               "EACH", "IN", "FUNCTION", "PROCEDURE", "INPUTS", "AS", "TO", "THEN", 
                                               "TRUE", "FALSE", "EQUAL", "GREATER", "LESS", "THAN", "INPUT", "MESSAGE", 
                                               "OR", "AND", "NOT", "END", "PRINT", "RETURN", "TIMES", "DIVIDED", "RAISE", 
                                               "POWER", "SWAP", "SORT", "LENGTH", "REMOVE" };
            popUp.Show();
            popUp.Hide();
        }

        // Get the pixel position of the character relative to the text box
        // Uses SizeF on strings to calculate the length from line 0, char 0
        public Point GetPositionFromCharIndex(int index)
        {
            Graphics g = this.CreateGraphics();
            
            SizeF textSize = g.MeasureString(this.Text.Substring(0, index), this.Font);

            SizeF localTextSize = g.MeasureString(this.Lines[GetLinesFromCharIndex(this.SelectionStart)], this.Font);

            Rectangle textRect = new Rectangle(0, 0, (int)textSize.Width, (int)textSize.Height - 20);

            // This allows us to ignore the width of all the other lines, other than the selected one
            Rectangle localTextRect = new Rectangle(0, 0, (int)localTextSize.Width, (int)localTextSize.Height - 20);

            // Taking away the vertical scroll of the text box to account for being on further lines (requiring scrolling from the top of the text box)
            return new Point(localTextRect.Width, textRect.Height - this.VerticalScroll.Value);
        }

        // Adds a word to the suggestion dictionary
        public void AddIntellisenseWord(string word)
        {
            string[] newWords = new string[suggestionDictionary.Length + 1];
            for (int i = 0; i < suggestionDictionary.Length; i++)
            {
                newWords[i] = suggestionDictionary[i];
            }
            newWords[newWords.Length - 1] = word;
            suggestionDictionary = new string[suggestionDictionary.Length + 1]; 
            suggestionDictionary = newWords;
        }

        // Removes a word from the suggestion dictionary
        public void RemoveIntellisenseWord(string word)
        {
            List<string> suggestionDictionaryList = suggestionDictionary.ToList();

            suggestionDictionaryList.ToList().Remove(word);

            suggestionDictionary = suggestionDictionaryList.ToArray();
        }

        // Removes the word currently being typed and checks for other words that are identifiers
        // When found the identifiers are added to a list of identifers, separate from the suggestion dictionary
        private void CheckForIdentifiers()
        {
            char[] separators = new char[] { ' ', ',', '(', ')', '\n', '\t' };
            
            // Remove the currently selected word
            
            int selectedIndex = this.SelectionStart;
            int wordStart = selectedIndex;
            string text = this.Text;
            while (wordStart > 0 && text[wordStart - 1] != ' ')
            {
                wordStart--;
            }

            // Find the end of the word
            int wordEnd = selectedIndex;
            while (wordEnd < text.Length && text[wordEnd] != ' ')
            {
                wordEnd++;
            }

            text = text.Remove(wordStart, wordEnd - wordStart);

            string[] words = text.Split(separators);

            List<string> identifiers = new List<string>();

            foreach (string word in words)
            {
                if (IsValidIdentifier(word))
                {
                    identifiers.Add(word);
                }
            }

            // Using a HashSet (Set) to create a set, which by definition has no duplicates
            // Converting the set back into a list, removing all duplicate identifier names
            List<string> noDuplicateList = new HashSet<string>(identifiers).ToList();

            identifierNames = noDuplicateList;
        }

        // Verifies that a word is not a valid keyword
        // Checks that it matches the pattern of a variable (starts lowercase)
        private bool IsValidIdentifier(string word)
        {
            string[] invalidWords = new string[] { ">=", "<=", "CREATE", "SET", "ADD", "TAKE", "AWAY", "MULTIPLY", "DIVIDE", "GET", "REMAINDER", "OF", "IF", "ELSE", "COUNT", "WITH", "FROM", "GOING", "UP", "DOWN", "BY", "WHILE", "DO", "REPEAT", "FOR", "EACH", "IN", "FUNCTION", "PROCEDURE", "INPUTS", "AS", "TO", "THEN", "TRUE", "FALSE", "EQUAL", "GREATER", "LESS", "THAN", "INPUT", "MESSAGE", "OR", "AND", "NOT", "END", "PRINT", "RETURN", "TIMES", "DIVIDED", "RAISE", "POWER", "[", "]", "(", ")", ",", "-", "+", "*", "/", "^", ">", "<", "=" };
            Regex validVariable = new Regex(@"[a-zA-Z_][a-zA-Z0-9_]*");
            if (invalidWords.Contains(word))
            {
                return false;
            }
            if (validVariable.IsMatch(word))
            {
                return true;
            }
            return false;
        }

        // Hides the pop up when the caret is moved off a certain word
        private void TextBox_SelectionChanged(object sender, EventArgs e)
        {
            popUp.Hide();
            isPopUpShowing = false;
        }

        // Exits out of the suggestion box when escape is clicked
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

        // Managing interactions with the suggestion box with arrow keys and tab (confirmation)
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

        // Shows the suggestion box and recalculates the identifiers
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            // Checking if a suggestion box should be shown
            string wordText;
            int lastIndexOfSpace;
            int lastIndexOfNewline;
            int lastIndexOfTab;
            int lastIndexOfQuote;
            int lastIndexOfLeftBracket;
            int lastIndexOfRightBracket;
            int lastIndexOfSquareLeftBracket;
            int lastIndexOfSquareRightBracket;
            int lastIndexOfComma;
            int lastIndexOf;
            if (this.SelectionStart > 0 && this.Text[this.SelectionStart - 1] != ' ' && this.Text[this.SelectionStart - 1] != '\t' && this.Text[this.SelectionStart - 1] != '\n' && this.Text[this.SelectionStart - 1] != '"' && this.Text[this.SelectionStart - 1] != '(' && this.Text[this.SelectionStart - 1] != ')' && this.Text[this.SelectionStart - 1] != '[' && this.Text[this.SelectionStart - 1] != ']' && this.Text[this.SelectionStart - 1] != ',')
            {
                wordText = this.Text.Substring(0, this.SelectionStart);
                lastIndexOfSpace = wordText.LastIndexOf(' ');
                lastIndexOfNewline = wordText.LastIndexOf('\n');
                lastIndexOfTab = wordText.LastIndexOf('\t');
                lastIndexOfQuote = wordText.LastIndexOf('"');
                lastIndexOfLeftBracket = wordText.LastIndexOf('(');
                lastIndexOfRightBracket = wordText.LastIndexOf(')');
                lastIndexOfSquareLeftBracket = wordText.LastIndexOf('[');
                lastIndexOfSquareRightBracket = wordText.LastIndexOf(']');
                lastIndexOfComma = wordText.LastIndexOf(',');
                lastIndexOf = Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(lastIndexOfComma, lastIndexOfSquareLeftBracket), lastIndexOfSquareRightBracket), lastIndexOfRightBracket), lastIndexOfLeftBracket), lastIndexOfQuote), lastIndexOfSpace), lastIndexOfNewline), lastIndexOfTab);
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
            
            // Reset suggestionDictionary
            suggestionDictionary = new string[] { "CREATE", "SET", "ADD", "TAKE", "AWAY", "MULTIPLY",
                                               "DIVIDE", "GET", "REMAINDER", "OF", "IF", "ELSE", "COUNT", "WITH",
                                               "FROM", "GOING", "UP", "DOWN", "BY", "WHILE", "DO", "REPEAT", "FOR",
                                               "EACH", "IN", "FUNCTION", "PROCEDURE", "INPUTS", "AS", "TO", "THEN",
                                               "TRUE", "FALSE", "EQUAL", "GREATER", "LESS", "THAN", "INPUT", "MESSAGE",
                                               "OR", "AND", "NOT", "END", "PRINT", "RETURN", "TIMES", "DIVIDED", "RAISE",
                                               "POWER" };

            CheckForIdentifiers();

            foreach (string identifier in identifierNames)
            {
                AddIntellisenseWord(identifier);
            }
        }

        // Uses differnt algorithms to add keywords based on their relevence
        private bool PopulateListBox(string wordTyping)
        {
            listBox.Items.Clear();

            int wordTypingLength = wordTyping.Length;

            List<KeyValuePair<int, string>> tempWords = new List<KeyValuePair<int, string>>();
            
            foreach (string word in suggestionDictionary)
            {
                if (word.ToUpper().StartsWith(wordTyping.ToUpper()))
                {
                    tempWords.Add(new KeyValuePair<int, string>(1, word));
                }
            }

            // No keywords have the same beginning
            // Resort to minimal Levenshtein distance
            if (tempWords.Count == 0 && wordTypingLength > 1)
            {
                int distance = -1;
                // For small lengths of wordTyping use recursive algorithm
                if (wordTypingLength < 3)
                {
                    foreach (string word in suggestionDictionary)
                    {
                        distance = -1;
                        if (word.Length < 5)
                        {
                            distance = LevenshteinRecursive(word, wordTyping.ToUpper());
                        }
                        else if (word.Length < 10)
                        {
                            distance = LevenshteinDynamic(word, wordTyping.ToUpper());
                        }
                        tempWords.Add(new KeyValuePair<int, string>(distance, word));
                    }
                }
                else if (wordTypingLength < 10)
                {
                    foreach (string word in suggestionDictionary)
                    {
                        distance = -1;
                        if (word.Length < 10)
                        {
                            distance = LevenshteinDynamic(word, wordTyping.ToUpper());
                        }
                        tempWords.Add(new KeyValuePair<int, string>(distance, word));
                    }
                }

                int minimumDistance = int.MaxValue;
                foreach (var v in tempWords)
                {
                    int currentDistance = v.Key;
                    if (currentDistance < minimumDistance && currentDistance >= 1)
                    {
                        minimumDistance = v.Key;
                    }
                }

                int differenceAllowed = 3;
                if (wordTypingLength < 3)
                {
                    differenceAllowed = 1;
                }

                for (int i = 0; i < tempWords.Count; i++)
                {
                    var v = tempWords[i];
                    if (v.Key > minimumDistance + differenceAllowed || v.Key > 2) 
                    {
                        tempWords.Remove(v);
                    }
                }
            }

            string[] outArray;

            // Sort the following using the key first and then by levenshtein distance
            outArray = tempWords.OrderBy(o => o.Key).ThenBy( n => Math.Abs(n.Value.Length - wordTypingLength)).Take(suggestionCount).Select(s => s.Value).Distinct().ToArray();

            listBox.Items.AddRange(outArray);

            return outArray.Length > 0;
        }

        // When clicking to select a word in the suggestion box, that word is inputted into the code field
        // If not clicked then do nothing in this method
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

        // Change isFromMouseClick to true to help identify mouse clicks in other methods
        private void ListBox_Click(object sender, EventArgs e)
        {
            isFromMouseClick = true;
        }
       
        // Places the suggestion box in the correct position
        // Preps the suggestion box
        // Shows the suggestion box
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

        // Replaces the word being typed with the keyword provided
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
        private int LevenshteinDynamic(string word1, string word2)
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

        // Gets length (for Levenshtein Algorithm)
        static int Length(string word)
        {
            return word.Length;
        }

        // Gets the first character from a word (string, not char)
        static string Head(string word)
        {
            return word[0].ToString();
        }

        // Gets all the characters excluding the head
        static string Tail(string word)
        {
            string tail = "";
            for (int i = 1; i < word.Length; i++)
            {
                tail += word[i];
            }
            return tail;
        }

        // Returns teh smallest value
        // Math.Min equivalent for 3 parameters
        static int Min(int a, int b, int c)
        {
            return Math.Min(a, Math.Min(b, c));
        }

        // Personal implementation of the built in methods for RichTextBox
        // Which are not available for FastColoredTextBox

        // Returns the line on which the char is located
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

        // Returns all lines as an array of strings
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

        // Returns the index of the first character in a line
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
