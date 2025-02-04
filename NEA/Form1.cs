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

namespace NEA
{
    public partial class IDE_MainWindow : Form
    {
        private Stack<string> undoStack = new Stack<string>();
        private Stack<int> undoStackCaretPosition = new Stack<int>();
        private Stack<string> redoStack = new Stack<string>();
        private Stack<int> redoStackCaretPosition = new Stack<int>();
        private Machine machine;

        private string currentFilePath = null;
        private bool isSaved = true;

        // IntelliSense Hack 101
        // https://stackoverflow.com/questions/40016018/c-sharp-make-an-autocomplete-to-a-richtextbox
        public IDE_MainWindow()
        {
            InitializeComponent();
            InitializeStacks();
            WindowState = FormWindowState.Maximized;
            txtCodeField.Select();
            timer.Start();
        }

        private void InitializeStacks()
        {
            undoStack.Push("");
            undoStackCaretPosition.Push(0);
        }

        private int GetMaxLineLength()
        {
            string[] lines = txtConsole.Lines;
            int max = 0;

            foreach (string line in lines)
            {
                int length = line.Length;
                if (length > max)
                {
                    max = length;
                }
            }

            return Math.Max(max,10);
        }
        
        private void Run()
        {
            if (txtConsole.Text.Length > 0)
            {
                string newExecuteSeparator = "";
                for (int i = 0; i < GetMaxLineLength(); i++)
                {
                    newExecuteSeparator += "=";
                }
                txtConsole.Text += $"{newExecuteSeparator}\r\n";
            }

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

        private void UpdateLineNumbers()
        {
            // Update Line Numbers

            int lineCount = txtCodeField.Lines.Length;
            string lineNumbers = "1";
            for (int i = 2; i <= lineCount; i++)
            {
                lineNumbers += "\n" + i.ToString();
            }

            txtLineNumber.Text = lineNumbers;

            // Update Scroll
            // https://stackoverflow.com/questions/1827323/synchronize-scroll-position-of-two-richtextboxes

            int nPos = NativeScroller.GetScrollPos(txtCodeField.Handle, 1);
            nPos <<= 16;
            uint wParam = (uint)NativeScroller.ScrollBarCommands.SB_THUMBPOSITION | (uint)nPos;
            NativeScroller.SendMessage(txtLineNumber.Handle, (int)NativeScroller.Message.WM_VSCROLL, new IntPtr(wParam), new IntPtr(0));
        }

        private void Comment()
        {
            int index = txtCodeField.SelectionStart;
            int line = txtCodeField.GetLineFromCharIndex(index);
            int firstCharOfLine = txtCodeField.GetFirstCharIndexOfCurrentLine();
            int lineLength;
            bool isLastLine = false;

            if (line == txtCodeField.Lines.Length - 1)
            {
                lineLength = txtCodeField.Text.Length - firstCharOfLine;
                isLastLine = true;
            }
            else
            {
                int nextLineFirstChar = txtCodeField.GetFirstCharIndexFromLine(line + 1);
                lineLength = nextLineFirstChar - firstCharOfLine;
            }

            int selectionStart = txtCodeField.SelectionStart;
            int selectionLength = txtCodeField.SelectionLength;

            // No selection
            // Caret is on a single line
            if (selectionLength == 0)
            {
                txtCodeField.Text = txtCodeField.Text.Insert(firstCharOfLine, "# ");
                int offset = 1;
                if (isLastLine)
                {
                    offset = 2;
                }
                txtCodeField.Select(firstCharOfLine + lineLength + offset, 0);
            }
            // Selection
            // Selection spans one line
            else if (selectionLength < lineLength - (index - firstCharOfLine) + 1)
            {
                txtCodeField.Text = txtCodeField.Text.Insert(firstCharOfLine, "# ");
                int offset = 1;
                if (isLastLine)
                {
                    offset = 2;
                }
                txtCodeField.Select(firstCharOfLine + lineLength + offset, 0);
            }
            // Multiline selection
            // Selections spans multiple lines
            else
            {
                int firstLine = firstCharOfLine;
                int lastLine = txtCodeField.GetLineFromCharIndex(index + selectionLength);

                string[] lines = txtCodeField.Lines;

                int totalAddedChars = 0;

                for (int i = firstCharOfLine; i <= lastLine; i++)
                {
                    lines[i] = "# " + lines[i];
                    totalAddedChars += 2;
                }

                txtCodeField.Lines = lines;
                txtCodeField.Select(selectionStart + selectionLength + totalAddedChars, 0);
            }
        }

        private void Cut()
        {
            string[] lines = txtCodeField.Lines;
            int selectionStart = txtCodeField.SelectionStart;
            int selectionLength = txtCodeField.SelectionLength;

            if (selectionLength == 0)
            {
                int line = txtCodeField.GetLineFromCharIndex(selectionStart);
                List<string> linesAsList = new List<string>(lines);
                linesAsList.RemoveAt(line);
                txtCodeField.Lines = linesAsList.ToArray();
                txtCodeField.SelectionStart = selectionStart;
            }
            else
            {
                txtCodeField.Text.Remove(selectionStart, selectionLength);
            }
        }

        private void Undo()
        {
            if (undoStack.Count > 1)
            {
                // Pops the current state
                string currentState = undoStack.Pop();
                redoStack.Push(currentState);
                int currentPos = undoStackCaretPosition.Pop();
                redoStackCaretPosition.Push(currentPos);

                // Sets the previous state
                txtCodeField.Text = undoStack.Peek();
                txtCodeField.SelectionStart = undoStackCaretPosition.Peek();
            }
        }

        private void Redo()
        {
            if (redoStack.Count > 1)
            {
                txtCodeField.Text = redoStack.Pop();
                txtCodeField.SelectionStart = redoStackCaretPosition.Pop();
            }
        }

        private void UpdateCaretPosition()
        {
            int index = txtCodeField.SelectionStart;
            int line = txtCodeField.GetLineFromCharIndex(index);
            int column = index - txtCodeField.GetFirstCharIndexOfCurrentLine();

            statusLineInfo.Text = $"Line: {line + 1}";
            statusColumnInfo.Text = $"Column: {column + 1}";
        }

        private void AddTabSpace()
        {
            int selectionLength = txtCodeField.SelectionLength;
            int selectionStart = txtCodeField.SelectionStart;

            txtCodeField.Text = txtCodeField.Text.Remove(selectionStart, selectionLength).Insert(selectionStart, "\t");

            txtCodeField.Select(selectionStart + 1, 0);
        }

        public void ConsoleWrite(string text)
        {
            txtConsole.Text += text + "\r\n";
        }

        public void ClearConsole()
        {
            txtConsole.Text = "";
        }

        private void txtCodeField_TextChanged(object sender, EventArgs e)
        {
            isSaved = false;
            if (undoStack.Count == 0 || undoStack.Peek() != txtCodeField.Text)
            {
                undoStack.Push(txtCodeField.Text);
                undoStackCaretPosition.Push(txtCodeField.SelectionStart);
            }
            UpdateCaretPosition();
            UpdateLineNumbers();
        }

        private void txtCodeField_VScroll(object sender, EventArgs e)
        {
            UpdateLineNumbers();
        }

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

                txtCodeField.Select(selectionStart + toPaste.Length, 0);
            }
            else
            {
                MessageBox.Show("There is no text to paste.");
            }
        }

        private void stripUndo_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void stripRedo_Click(object sender, EventArgs e)
        {
            Redo();
        }

        private void stripRun_Click(object sender, EventArgs e)
        {
            Run();
        }
        
        private void stripComment_Click(object sender, EventArgs e)
        {
            Comment();
            if (undoStack.Count == 0 || undoStack.Peek() != txtCodeField.Text)
            {
                undoStack.Push(txtCodeField.Text);
                undoStackCaretPosition.Push(txtCodeField.SelectionStart);
            }
            UpdateCaretPosition();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearConsole();
        }

        private void btnMinusFont_Click(object sender, EventArgs e)
        {
            txtConsole.Font = new Font("Courier New", txtConsole.Font.Size - 1);
        }

        private void btnPlusFont_Click(object sender, EventArgs e)
        {
            txtConsole.Font = new Font("Courier New", txtConsole.Font.Size + 1);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ClearConsole();
            txtConsole.Font = new Font("Courier New", 12);
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
                Redo();
            }
            else if (e.KeyCode == Keys.Z && e.Control)
            {
                Undo();
            }
            else if (e.KeyCode == Keys.F5)
            {
                Run();
            }
            else if (e.KeyCode == Keys.Back && txtCodeField.Focused)
            {
                if (txtCodeField.SelectionStart == txtCodeField.Text.Length)
                {
                    txtCodeField.ScrollToCaret();
                }
            }
            else if (e.KeyCode == Keys.Up)
            {
                int selected = txtCodeField.SelectionStart;
                int line = txtCodeField.GetLineFromCharIndex(selected);
                if (line == 0)
                {
                    txtCodeField.SelectionStart = 0;
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                int selected = txtCodeField.SelectionStart;
                int line = txtCodeField.GetLineFromCharIndex(selected);
                if (line == txtCodeField.Lines.Length - 1)
                {
                    txtCodeField.SelectionStart = txtCodeField.Text.Length;
                }
            }
        }
        
        // Fix for tab seleting elements of the applications
        // Overrides the ProcessCmdKey method
        // Injects new function of the tab and does not affect other Command Keys
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Tab)
            {
                AddTabSpace();

                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void txtCodeField_SelectionChanged(object sender, EventArgs e)
        {
            UpdateCaretPosition();
        }

        private void tsEditCut_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void tsIntermediateView_Click(object sender, EventArgs e)
        {
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

        private void tsFileOpen_Click(object sender, EventArgs e)
        {
            if (PromptToSaveChanges())
            {
                OpenFile();
            }
        }

        private void tsFileSaveAs_Click(object sender, EventArgs e)
        {
            if (!isSaved)
            {
                SaveFileAs();
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
            }
        }

        private void SaveFile()
        {
            if (currentFilePath == null)
            {
                SaveFileAs();
            }
            else
            {
                File.WriteAllText(currentFilePath, txtCodeField.Text);
                isSaved = true;
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

        private void txtCodeField_SelectionChanged_1(object sender, EventArgs e)
        {
            UpdateCaretPosition();
            txtCodeField.AppendText("");
            txtCodeField.ScrollToCaret();
        }

        private void txtCodeField_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            // Dumbest piece of code I've ever written
            if (txtCodeField.ZoomFactor != 1.0f)
            {
                txtCodeField.ZoomFactor = 1.0f;
            }
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
    }
}