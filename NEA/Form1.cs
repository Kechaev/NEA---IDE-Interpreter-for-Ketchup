using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Threading;
using System.Reflection;
using NEA.Classes;
using System.Collections;
using System.Diagnostics.PerformanceData;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace NEA
{
    public partial class IDE_MainWindow : Form
    {
        private Stack<string> undoStack = new Stack<string>();
        private Stack<int> undoStackCaretPosition = new Stack<int>();
        private Stack<string> redoStack = new Stack<string>();
        private Stack<int> redoStackCaretPosition = new Stack<int>();
        private int currentIndent = 0;
        private Machine machine;

        private string currentFilePath = null;
        private bool isSaved = true;
        private static int NoOfRuns = 1;

        // IntelliSense Hack 101
        // https://stackoverflow.com/questions/40016018/c-sharp-make-an-autocomplete-to-a-richtextbox
        public IDE_MainWindow()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            txtCodeField.Select();
            inString = false;
        }
        
        private void Run()
        {
            string name = $"Unsaved Program {NoOfRuns++}";
            if (Path.GetFileName(currentFilePath) != null)
            {
                name = Path.GetFileName(currentFilePath);
            }
            string time = DateTime.Now.ToLongTimeString();

            txtConsole.Text += $"=== {name} - {time} ===\r\n";

            machine = new Machine(txtCodeField.Text);

            // Error Checking
            try
            {
                machine.Interpret();

                string[] intermediate = machine.GetIntermediateCode();

                StartExecution(intermediate);
            }
            catch (Exception e)
            {
                ConsoleWrite(e.Message);
            }

            //No Error Checking
            //machine.Interpret();
            //string[] intermediate = machine.GetIntermediateCode();
            //StartExecution(intermediate);
        }

        public void StartExecution(string[] intermediateCode)
        {
            machine.SetRunningStatus(machine.GetValidity());
            while (machine.GetRunningStatus())
            {
                machine.FetchExecute(intermediateCode, ref txtConsole, false);

                txtConsole.SelectionStart = txtConsole.Text.Length;
                txtConsole.ScrollToCaret(); 
            }
        }

        private void Comment()
        {
            //int index = txtCodeField.SelectionStart;
            //int line = txtCodeField.GetLineFromCharIndex(index);
            //int firstCharOfLine = txtCodeField.GetFirstCharIndexOfCurrentLine();
            //int lineLength;
            //bool isLastLine = false;

            //if (line == txtCodeField.Lines.Length - 1)
            //{
            //    lineLength = txtCodeField.Text.Length - firstCharOfLine;
            //    isLastLine = true;
            //}
            //else
            //{
            //    int nextLineFirstChar = txtCodeField.GetFirstCharIndexFromLine(line + 1);
            //    lineLength = nextLineFirstChar - firstCharOfLine;
            //}

            //int selectionStart = txtCodeField.SelectionStart;
            //int selectionLength = txtCodeField.SelectionLength;

            //// No selection
            //// Caret is on a single line
            //if (selectionLength == 0)
            //{
            //    txtCodeField.Text = txtCodeField.Text.Insert(firstCharOfLine, "# ");
            //    int offset = 1;
            //    if (isLastLine)
            //    {
            //        offset = 2;
            //    }
            //    txtCodeField.Select(firstCharOfLine + lineLength + offset, 0);
            //}
            //// Selection
            //// Selection spans one line
            //else if (selectionLength < lineLength - (index - firstCharOfLine) + 1)
            //{
            //    txtCodeField.Text = txtCodeField.Text.Insert(firstCharOfLine, "# ");
            //    int offset = 1;
            //    if (isLastLine)
            //    {
            //        offset = 2;
            //    }
            //    txtCodeField.Select(firstCharOfLine + lineLength + offset, 0);
            //}
            //// Multiline selection
            //// Selections spans multiple lines
            //else
            //{
            //    int firstLine = firstCharOfLine;
            //    int lastLine = txtCodeField.GetLineFromCharIndex(index + selectionLength);

            //    string[] lines = txtCodeField.Lines;

            //    int totalAddedChars = 0;

            //    for (int i = firstCharOfLine; i <= lastLine; i++)
            //    {
            //        lines[i] = "# " + lines[i];
            //        totalAddedChars += 2;
            //    }

            //    txtCodeField.Lines = lines;
            //    txtCodeField.Select(selectionStart + selectionLength + totalAddedChars, 0);
            //}
        }

        private void Cut()
        {
            //string[] lines = txtCodeField.Lines;
            //int selectionStart = txtCodeField.SelectionStart;
            //int selectionLength = txtCodeField.SelectionLength;

            //if (selectionLength == 0)
            //{
            //    int line = txtCodeField.GetLineFromCharIndex(selectionStart);
            //    List<string> linesAsList = new List<string>(lines);
            //    linesAsList.RemoveAt(line);
            //    txtCodeField.Lines = linesAsList.ToArray();
            //    txtCodeField.SelectionStart = selectionStart;
            //}
            //else
            //{
            //    txtCodeField.Text.Remove(selectionStart, selectionLength);
            //}
        }

        private void UpdateCaretPosition()
        {
            int index = txtCodeField.SelectionStart;
            string text = txtCodeField.Text;
            int line = 0;

            for (int i = 0; i < index; i++)
            {
                if (text[i] == '\n')
                    line++;
            }

            int column = 0;

            for (int i = index - 1; i >= 0; i--)
            {
                if (text[i] == '\n')
                {
                    break;
                }

                column++;
            }

            statusLineInfo.Text = $"Line: {line + 1}";
            statusColumnInfo.Text = $"Column: {column + 1}";
        }

        public void ConsoleWrite(string text)
        {
            txtConsole.Text += text + "\r\n";
        }

        public void ClearConsole()
        {
            txtConsole.Text = "";
        }

        private bool inString;
        
        private void tsEditCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCodeField.SelectedText))
            {
                Clipboard.SetText(txtCodeField.SelectedText);
            }
            else if (!string.IsNullOrEmpty(txtConsole.SelectedText))
            {
                Clipboard.SetText(txtConsole.SelectedText);
            }
            else
            {
                MessageBox.Show("Please select some text to copy.");
            }
        }

        private void tsEditPaste_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                string toPaste = Clipboard.GetText();
                int selectionStart = txtCodeField.SelectionStart;
                int selectionLength = txtCodeField.SelectionLength;

                if (selectionLength > 0)
                {
                    txtCodeField.Text = txtCodeField.Text.Remove(selectionStart, selectionLength);
                }

                txtCodeField.Text = txtCodeField.Text.Insert(selectionStart, toPaste);

                //txtCodeField.Select(selectionStart + toPaste.Length, 0);
            }
            else
            {
                MessageBox.Show("There is no text to paste.");
            }
        }

        private void stripUndo_Click(object sender, EventArgs e)
        {
            txtCodeField.Undo();
        }

        private void stripRedo_Click(object sender, EventArgs e)
        {
            txtCodeField.Redo();
        }

        private void stripRun_Click(object sender, EventArgs e)
        {
            Run();
        }
        
        private void stripComment_Click(object sender, EventArgs e)
        {
            Comment();
            UpdateCaretPosition();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearConsole();
        }

        private void tsFileExit_Click(object sender, EventArgs e)
        {
            if (PromptToSaveChanges())
            {
                Application.Exit();
            }
        }

        private void txtCodeField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z && e.Control && e.Shift)
            {
                txtCodeField.Redo();
            }
            else if (e.KeyCode == Keys.Z && e.Control)
            {
                txtCodeField.Undo();
            }
            else if (e.KeyCode == Keys.F5)
            {
                Run();
            }
            else if (e.KeyCode == Keys.Back && txtCodeField.Focused)
            {
                //if (txtCodeField.SelectionStart == txtCodeField.Text.Length)
                //{
                //    txtCodeField.ScrollToCaret();
                //}
                //txtCodeField.SelectionColor = Color.Black;
            }
            else if (e.KeyCode == Keys.Up)
            {
                //int selected = txtCodeField.SelectionStart;
                //int line = txtCodeField.GetLineFromCharIndex(selected);
                //if (line == 0)
                //{
                //    txtCodeField.SelectionStart = 0;
                //}
            }
            else if (e.KeyCode == Keys.Down)
            {
                //int selected = txtCodeField.SelectionStart;
                //int line = txtCodeField.GetLineFromCharIndex(selected);
                //if (line == txtCodeField.Lines.Length - 1)
                //{
                //    txtCodeField.SelectionStart = txtCodeField.Text.Length;
                //}
            }
            else if (e.KeyCode == Keys.Enter)
            {
                //e.SuppressKeyPress = true;
                //int start = txtCodeField.SelectionStart;
                //int line = txtCodeField.GetLineFromCharIndex(start);
                //string[] lines = txtCodeField.Lines;
                //int nextLineStart = txtCodeField.GetFirstCharIndexFromLine(line + 1);
                //int lineEnd = txtCodeField.TextLength;
                //if (nextLineStart != -1)
                //{
                //    lineEnd = nextLineStart - 1;
                //}

                //if (line < lines.Length)
                //{
                //    string[] words = lines[line].TrimStart().Split(' ');
                //    string[] toIndent = { "FUNCTION", "IF", "ELSE", "COUNT", "REPEAT", "WHILE", "DO" };
                //    string[] unIndent = { "END" };

                //    string beforeSubstring = "";
                //    for (int i = 0; i <= line; i++)
                //    {
                //        beforeSubstring += $"{lines[i]}\n";
                //    }

                //    if (words.Length > 0 && toIndent.Contains(words[0].ToUpper()) && lineEnd == start)
                //    {
                //        currentIndent = FindIndent(beforeSubstring);
                //    }
                //    else if (words.Length > 0 && unIndent.Contains(words[0].ToUpper()))
                //    {
                //        currentIndent = FindIndent(beforeSubstring) - 1;
                //        string currentLine = lines[line].TrimStart('\t');
                //        for (int i = 0; i < currentIndent; i++)
                //        {
                //            currentLine = "\t" + currentLine;
                //        }

                //        int lineStart = txtCodeField.GetFirstCharIndexFromLine(line);
                //        int lineLength = lines[line].Length;

                //        txtCodeField.Select(lineStart, lineLength);
                //        txtCodeField.SelectedText = currentLine;
                //        txtCodeField.SelectionStart = lineStart + txtCodeField.Lines[line].Length;
                //    }

                //    string insert = "\n";

                //    for (int i = 0; i < currentIndent; i++)
                //    {
                //        insert += "\t";
                //    }

                //    txtCodeField.SelectedText = insert;
                //}
                //else
                //{
                //    txtCodeField.SelectedText = "\n";
                //}
            }
        }

        private int FindIndent(string beforeSubstring)
        {
            int indent = 0;
            string[] toIndent = { "FUNCTION", "IF", "ELSE", "COUNT", "REPEAT", "WHILE", "DO" };
            string[] unIndent = { "END" };
            string[] words = beforeSubstring.Split(' ');
            foreach (string word in words)
            {
                if (toIndent.Contains(word))
                {
                    indent++;
                }
                else if (unIndent.Contains(word))
                {
                    indent--;
                }
            }
            return indent;
        }

        // Fix for tab seleting elements of the applications
        // Overrides the ProcessCmdKey method
        // Injects new function of the tab and does not affect other Command Keys
        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    if (keyData == Keys.Tab)
        //    {
        //        AddTabSpace();

        //        return true;
        //    }

        //    return base.ProcessCmdKey(ref msg, keyData);
        //}

        private void tsEditCut_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void tsIntermediateView_Click(object sender, EventArgs e)
        {
            // List boxes
            if (txtCodeField.Text == "")
            {
                MessageBox.Show("No program has been entered.\nCannot open Intermediate View.");
            }
            else
            {
                try
                {
                    machine = new Machine(txtCodeField.Text);
                    machine.Interpret();

                    string[] intermediate = machine.GetIntermediateCode();

                    IntermediateView intermediateForm = new IntermediateView(machine.GetIntermediateCode(), machine.GetSubroutinesIntermediateCode(), machine.GetSubroutineDictionary());
                    intermediateForm.ShowDialog();
                }
                catch (Exception m)
                {
                    ConsoleWrite(m.Message);
                }
            }
        }

        private void tsTokenView_Click(object sender, EventArgs e)
        {
            // Lists boxes
            if (txtCodeField.Text == "")
            {
                MessageBox.Show("No program has been entered.\nCannot open Token View.");
            }
            else
            {
                machine = new Machine(txtCodeField.Text);
                Token[] tokens = machine.Tokenize();

                string[] tokensString = new string[tokens.Length];

                for (int i = 0; i < tokens.Length; i++)
                {
                    tokensString[i] = tokens[i].GetTokenType().ToString();
                }

                TokenView tokenForm = new TokenView(tokensString);
                tokenForm.ShowDialog();
            }
        }

        private void tsClear_Click(object sender, EventArgs e)
        {
            ClearConsole();
        }

        #region File Interactions
        private void tsFileOpen_Click(object sender, EventArgs e)
        {
            if (PromptToSaveChanges())
            {
                OpenFile();
                if (currentFilePath != null)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath).Remove(Path.GetFileName(currentFilePath).Length - 5, 5)}";
                }
            }
        }

        private void tsFileSaveAs_Click(object sender, EventArgs e)
        {
            if (!isSaved)
            {
                SaveFileAs();
                if (currentFilePath != null)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath).Remove(Path.GetFileName(currentFilePath).Length - 5, 5)}";
                }
            }
        }

        private void tsFileSave_Click(object sender, EventArgs e)
        {
            if (currentFilePath == null)
            {
                SaveFileAs();
            }
            else
            {
                File.WriteAllText(currentFilePath, txtCodeField.Text);
                isSaved = true;
                if (currentFilePath != null)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath).Remove(Path.GetFileName(currentFilePath).Length - 5, 5)}";
                }
            }
        }

        private void OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Ketchup Files (*.ktch)|*.ktch",
                Title = "File Open"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentFilePath = openFileDialog.FileName;
                txtCodeField.Text = File.ReadAllText(currentFilePath);
                isSaved = true;
                if (currentFilePath != null)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath).Remove(Path.GetFileName(currentFilePath).Length - 5, 5)}";
                }
            }
        }

        private void SaveFileAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "Custom Files (*.ktch)|*.ktch",
                Title = "Save File As"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentFilePath = saveFileDialog.FileName;
                File.WriteAllText(currentFilePath, txtCodeField.Text);
                isSaved = true;
                this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath).Remove(Path.GetFileName(currentFilePath).Length - 5, 5)}";
            }
        }

        private void SaveFile()
        {
            if (currentFilePath == null)
            {
                SaveFileAs();
                if (currentFilePath != null)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath).Remove(Path.GetFileName(currentFilePath).Length - 5, 5)}";
                    tabCodeControl.SelectedTab.Name = Path.GetFileName(currentFilePath).Remove(Path.GetFileName(currentFilePath).Length - 5, 5);
                }
            }
            else
            {
                File.WriteAllText(currentFilePath, txtCodeField.Text);
                isSaved = true;
                if (currentFilePath != null)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath).Remove(Path.GetFileName(currentFilePath).Length - 5, 5)}";
                    tabCodeControl.SelectedTab.Name = Path.GetFileName(currentFilePath).Remove(Path.GetFileName(currentFilePath).Length - 5, 5);
                }
            }
        }

        private bool PromptToSaveChanges()
        {
            //MessageBox.Show($"Run?  {!isSaved}");
            if (!isSaved)
            {
                // Fix this mess
                // Not saving on exit - problem in this method
                // Not prompting a save in other scenarios
                var result = MessageBox.Show($"Do you want to save changes?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    SaveFile();
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        private void tsFileNew_Click(object sender, EventArgs e)
        {
            // Temp fix
            PromptToSaveChanges();

            txtCodeField.Text = "";

            this.Text = "Ketchup™️ IDE";
        }
        #endregion

        private void IDE_MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isSaved && !PromptToSaveChanges())
            {
                e.Cancel = true;
            }
        }

        private void tsDebugTraceTable_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Trace Table");
        }

        private void tsDebugSyntaxCheck_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Check Syntax");
        }

        private void tsDebugBreakpoints_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Breakpoints");
        }

        private void stripFormat_Click(object sender, EventArgs e)
        {
            string usersCode = txtCodeField.Text;

            // Get Tokens and sort by need for capitalisation
            machine = new Machine(usersCode);

            Token[] tokens = machine.Tokenize();

            usersCode = "";

            Token prev = tokens[0];

            foreach (Token token in tokens)
            {
                if (!IsSameLine(token, prev))
                {
                    usersCode += "\n";
                }
                if (IsVariable(token) || IsLiteral(token))
                {
                    if (Is(token, TokenType.STR_LITERAL))
                    {
                        usersCode += "\"" + token.GetLiteral() + "\" ";
                    }
                    else
                    {
                        usersCode += token.GetLiteral() + " ";
                    }
                }
                else
                {
                    if (token.GetLiteral() == null) { }
                    else
                    {
                        usersCode += token.GetLiteral().ToUpper() + " ";
                    }
                }
                prev = token;
            }

            // Reset code to have no indentation
            string[] lines = usersCode.Split('\n');
            char[] toTrim = { '\t', ' ' };
            usersCode = "";
            foreach (string line in lines)
            {
                usersCode += line.Trim(toTrim) + "\n";
            }

            usersCode = usersCode.TrimEnd('\n');

            lines = usersCode.Split('\n');
            usersCode = "";
            string[] toIndent = { "FUNCTION", "COUNT", "IF", "ELSE", "WHILE", "REPEAT", "DO" };
            string[] unIndent = { "END" };
            int currentIndent = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] words = line.Split(' ');
                
                if (unIndent.Contains(words[0].ToUpper()))
                {
                    currentIndent--;
                }
                
                for (int j = 0; j < currentIndent; j++)
                {
                    usersCode += '\t';
                }
                usersCode += line + '\n';

                if (toIndent.Contains(words[0].ToUpper()))
                {
                    currentIndent++;
                }
            }

            usersCode = usersCode.TrimEnd('\n');

            txtCodeField.Text = usersCode;
        }

        private bool IsVariable(Token token)
        {
            return token.GetTokenType() == TokenType.VARIABLE;
        }

        private bool IsLiteral(Token token)
        {
            TokenType[] literals = { TokenType.STR_LITERAL, TokenType.CHAR_LITERAL,
                                     TokenType.INT_LITERAL, TokenType.DEC_LITERAL,
                                     TokenType.BOOL_LITERAL };
            return literals.Contains(token.GetTokenType());
        }

        private bool Is(Token token, TokenType type)
        {
            return token.GetTokenType() == type;
        }

        private bool IsSameLine(Token token1, Token token2)
        {
            return token1.GetLine() == token2.GetLine();
        }

        private void stripCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtCodeField.SelectedText))
            {
                Clipboard.SetText(txtCodeField.SelectedText);
            }
            else if (!string.IsNullOrEmpty(txtConsole.SelectedText))
            {
                Clipboard.SetText(txtConsole.SelectedText);
            }
            else
            {
                MessageBox.Show("Please select some text to copy.");
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtConsole.SelectedText))
            {
                Clipboard.SetText(txtConsole.SelectedText);
            }
            else
            {
                Clipboard.SetText(txtConsole.Text);
            }
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            txtConsole.Text = "";
        }

        private void btnCopyLastProgram_Click(object sender, EventArgs e)
        {
            string[] lines = txtConsole.Lines;
            int i = lines.Length - 1;
            bool searching = true;
            for (; i >= 0 && searching; i--)
            {
                // Regex not working
                Regex rg = new Regex(@"={3} (Unsaved Program \d+)|(.*\.ktch) - \d{2}:\d{2}:\d{2} ={3}");
                Match match = rg.Match(lines[i]);
                if (match.Success)
                {
                    searching = false;
                }
            }
            i++;
            if (!searching)
            {
                string output = "";
                for (int j = lines.Length - 2; j > i; j--)
                {
                    output = lines[j] + "\n" + output;
                }
                Clipboard.SetText(output);
            }
            else
            {
                MessageBox.Show($"No text was copied");
            }
        }

        Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Regular);
        Style PurpleStyle = new TextStyle(Brushes.Purple, null, FontStyle.Regular);
        Style PinkStyle = new TextStyle(Brushes.HotPink, null, FontStyle.Regular);
        Style OrangeStyle = new TextStyle(Brushes.DarkOrange, null, FontStyle.Regular);
        Style CyanStyle = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);
        Style RedStyle = new TextStyle(Brushes.DarkRed, null, FontStyle.Regular);
        Style BlueStyle = new TextStyle(Brushes.DarkBlue, null, FontStyle.Regular);
        Style GreyStyle = new TextStyle(Brushes.Gray, null, FontStyle.Italic);

        private void txtCodeField_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            // Regex explanation
            // \b - boundary character (beginning of a word)
            // (?i) - case insensitivity
            // | - OR operator
            // (?!\S) - Matches only whitespace characters ( ,\n,\t)

            e.ChangedRange.ClearStyle(GreenStyle);
            e.ChangedRange.ClearStyle(PurpleStyle);
            e.ChangedRange.ClearStyle(PinkStyle);
            e.ChangedRange.ClearStyle(OrangeStyle);
            e.ChangedRange.ClearStyle(CyanStyle);
            e.ChangedRange.ClearStyle(RedStyle);
            e.ChangedRange.ClearStyle(BlueStyle);
            e.ChangedRange.ClearStyle(GreyStyle);
            // String not working after "" (false positive)
            e.ChangedRange.SetStyle(GreenStyle, "(\".*?\")", RegexOptions.Singleline);
            e.ChangedRange.SetStyle(PurpleStyle, @"\b(?i)(print|input|message)(?!\S)");
            e.ChangedRange.SetStyle(PinkStyle, @"\b(?i)(set|create|add|take|away|multiply|divide|get|remainder|of)(?!\S)");
            e.ChangedRange.SetStyle(OrangeStyle, @"\b(?i)(count|while|do|repeat|if|else|function|procedure|then|as|times)(?!\S)");
            e.ChangedRange.SetStyle(BlueStyle, @"\b(?i)(integer|decimal|string|character|boolean|array|list)(?!\S)");
            e.ChangedRange.SetStyle(CyanStyle, @"\b(?i)(to|from|with|going|up|down|by)(?!\S)");
            e.ChangedRange.SetStyle(RedStyle, @"\b(?i)(end|return)(?!\S)");

            UpdateCaretPosition();
        }
    }
}