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
using System.Security.Permissions;
using static System.Net.Mime.MediaTypeNames;

namespace NEA
{
    public partial class IDE_MainWindow : Form
    {
        private Thread executionLoop;
        private Machine machine;

        private bool isSaved;
        private static int NoOfRuns = 1;

        private List<CustomFastColoredTextBox> arrayCodeFields;
        private CustomFastColoredTextBox currentCodeField;
        private List<string> currentFilePath;
        private bool isRunning;
        private bool isThreadAborted;
        private bool unchangedCode;

        public IDE_MainWindow()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            txtCodeField.Select();
            currentCodeField = txtCodeField;
            arrayCodeFields = new List<CustomFastColoredTextBox>();
            arrayCodeFields.Add(currentCodeField);
            currentFilePath = new List<string>();
            currentFilePath.Add(null);
            txtCodeField.AutoIndent = true;
            isSaved = true;
            isRunning = false;
            isThreadAborted = true;
            unchangedCode = false;
        }

        private void Run()
        {
            string name = $"Unsaved Program {NoOfRuns++}";
            if (Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]) != null)
            {
                name = Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]);
            }
            string time = DateTime.Now.ToLongTimeString();

            txtConsole.Text += $"=== {name} - {time} ===\r\n";

            TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
            CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
            machine = new Machine(txtCodeField.Text, txtConsole.Text);

            // Error Checking
            try
            {
                machine.Interpret();
                string[] intermediate = machine.GetIntermediateCode();
                try
                {
                    executionLoop = new Thread(ExecutionLoop);
                    executionLoop.Start();
                }
                catch (Exception e)
                {
                    // Failed to translate or execute
                    // Show error

                    this.Invoke(new MethodInvoker(delegate
                    {
                        ConsoleWrite(e.Message);
                    }));
                }
            }
            catch (Exception e)
            {
                // Failed to interpret
                // Set the stripRun back to RunSymbol
                // Show error
                this.Invoke(new MethodInvoker(delegate
                {
                    ConsoleWrite(e.Message);
                }));
                stripRun.Image = Properties.Resources.RunSymbolSmall;
            }



            //No Error Checking
            //machine.Interpret();

            //string[] intermediate = machine.GetIntermediateCode();

            //executionLoop = new Thread(ExecutionLoop);
            //executionLoop.Start();
        }

        private void ExecutionLoop()
        {
            string[] intermediate = machine.GetIntermediateCode();
            StartExecution(intermediate);
        }

        public void StartExecution(string[] intermediateCode)
        {
            machine.SetRunningStatus(machine.GetValidity());
            while (machine.GetRunningStatus())
            {
                try
                {
                    machine.FetchExecute(intermediateCode, ref txtConsole, false);
                }
                catch (Exception e)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        ConsoleWrite(e.Message);
                    }));
                }

                this.Invoke(new MethodInvoker(delegate
                {
                    txtConsole.SelectionStart = txtConsole.Text.Length;
                    txtConsole.ScrollToCaret();
                }));

                string consoleText = machine.GetConsoleText();
                if (consoleText != txtConsole.Text)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        txtConsole.Text = consoleText;
                    }));
                }
            }
            stripRun.Image = Properties.Resources.RunSymbolSmall;
            isRunning = false;
            isThreadAborted = true;
        }

        private void ControlledRun()
        {
            if (!isRunning || executionLoop == null)
            {
                isRunning = true;
                stripRun.Image = Properties.Resources.EndSymbolSmall;
                if (!isThreadAborted && unchangedCode)
                {
                    executionLoop.Resume();
                }
                else
                {
                    Run();
                }
            }
            else
            {
                isRunning = false;
                isThreadAborted = false;
                stripRun.Image = Properties.Resources.RunSymbolSmall;
                executionLoop.Suspend();
            }
        }

        private void Comment()
        {
            int index = txtCodeField.SelectionStart;
            int line = currentCodeField.GetLinesFromCharIndex(index);
            int firstCharOfLine = txtCodeField.GetFirstCharIndexOfLine(line);
            int lineLength;
            bool isLastLine = false;

            if (line == txtCodeField.GetLinesFromTextBox().Length - 1)
            {
                lineLength = txtCodeField.Text.Length - firstCharOfLine;
                isLastLine = true;
            }
            else
            {
                int nextLineFirstChar = txtCodeField.GetFirstCharIndexOfLine(line + 1);
                lineLength = nextLineFirstChar - firstCharOfLine;
            }

            int selectionStart = txtCodeField.SelectionStart;
            int selectionLength = txtCodeField.SelectionLength;

            // No selection
            // Caret is on a single line
            if (selectionLength == 0)
            {
                if (txtCodeField.Text.Substring(firstCharOfLine, 2) == "  ")
                {
                    txtCodeField.Text = txtCodeField.Text.Remove(firstCharOfLine, 2).Insert(firstCharOfLine, "# ");
                }
                else
                {
                    txtCodeField.Text = txtCodeField.Text.Insert(firstCharOfLine, "# ");
                }
                txtCodeField.SelectionStart = firstCharOfLine + lineLength;
            }
            // Selection
            // Selection spans one line
            else if (selectionLength < lineLength - (index - firstCharOfLine) + 1)
            {
                if (txtCodeField.Text.Substring(firstCharOfLine, 2) == "  ")
                {
                    txtCodeField.Text = txtCodeField.Text.Remove(firstCharOfLine, 2).Insert(firstCharOfLine, "# ");
                }
                else
                {
                    txtCodeField.Text = txtCodeField.Text.Insert(firstCharOfLine, "# ");
                }
                txtCodeField.SelectionStart = firstCharOfLine + lineLength;
            }
            // Multiline selection
            // Selections spans multiple lines
            else
            {
                int firstLine = txtCodeField.GetLinesFromCharIndex(firstCharOfLine);
                int lastLine = txtCodeField.GetLinesFromCharIndex(index + selectionLength);

                string[] lines = txtCodeField.GetLinesFromTextBox();

                int totalAddedChars = 0;

                for (int i = firstLine; i <= lastLine; i++)
                {
                    firstCharOfLine = txtCodeField.GetFirstCharIndexOfLine(txtCodeField.GetLinesFromCharIndex(lines[i][0]));
                    if (lines[i].Substring(0, 2) == "  ")
                    {
                        lines[i] = lines[i].Remove(0, 2).Insert(0, "# ");
                    }
                    else
                    {
                        lines[i] = lines[i].Insert(0, "# ");
                    }
                    totalAddedChars += 2;
                }

                string newText = "";
                foreach (string l in lines)
                {
                    newText += l + "\n";
                }
                
                txtCodeField.Text = newText.Remove(newText.Length - 1);;
                txtCodeField.SelectionStart = selectionStart + selectionLength + totalAddedChars;
            }
        }

        private void Cut()
        {
            string[] lines = txtCodeField.Lines.ToArray();
            int selectionStart = txtCodeField.SelectionStart;
            int selectionLength = txtCodeField.SelectionLength;

            if (selectionLength == 0)
            {
                int currentLine = txtCodeField.GetLinesFromCharIndex(selectionStart);
                List<string> linesAsList = new List<string>(lines);
                linesAsList.RemoveAt(currentLine);
                string newText = "";
                foreach (string line in linesAsList)
                {
                    newText += line + "\n";
                }
                txtCodeField.Text = newText;
                txtCodeField.SelectionStart = selectionStart;
            }
            else
            {
                txtCodeField.Text = txtCodeField.Text.Remove(selectionStart, selectionLength);
            }
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
        
        private void tsEditCopy_Click(object sender, EventArgs e)
        {
            TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
            CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
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
            TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
            CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
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

                txtCodeField.SelectionStart = selectionStart + toPaste.Length;

                //txtCodeField.Select(selectionStart + toPaste.Length, 0);
            }
            else
            {
                MessageBox.Show("There is no text to paste.");
            }
        }

        private void stripUndo_Click(object sender, EventArgs e)
        {
            TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
            CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
            txtCodeField.Undo();
        }

        private void stripRedo_Click(object sender, EventArgs e)
        {
            TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
            CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
            txtCodeField.Redo();
        }

        private void stripRun_Click(object sender, EventArgs e)
        {
            ControlledRun();
            unchangedCode = true;
        }
        
        private void stripComment_Click(object sender, EventArgs e)
        {
            Comment();
            UpdateCaretPosition();
        }

        private void tsFileExit_Click(object sender, EventArgs e)
        {
            if (PromptToSaveChanges())
            {
                CloseAllForms();
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
                ControlledRun();
                unchangedCode = true;
            }
            else
            {
                UpdateCaretPosition();
            }
        }

        private void tsEditCut_Click(object sender, EventArgs e)
        {
            Cut();
        }

        #region Code Views
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
                    machine = new Machine(txtCodeField.Text, txtConsole.Text);
                    machine.Interpret();

                    string[] intermediate = machine.GetIntermediateCode();

                    IntermediateView intermediateForm = new IntermediateView(machine.GetIntermediateCode(), machine.GetSubroutinesIntermediateCode(), machine.GetSubroutineDictionary());
                    intermediateForm.ShowDialog();
                }
                catch (Exception m)
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        ConsoleWrite(m.Message);
                    }));
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
                try
                {
                    machine = new Machine(txtCodeField.Text, txtConsole.Text);
                    Token[] tokens = machine.Tokenize();

                    string[] tokensString = new string[tokens.Length];

                    for (int i = 0; i < tokens.Length; i++)
                    {
                        tokensString[i] = tokens[i].GetTokenType().ToString();
                    }

                    TokenView tokenForm = new TokenView(tokensString);
                    tokenForm.ShowDialog();
                }
                catch
                {
                    MessageBox.Show("Invalid program entererd.\nCannot open Token View.");
                }
            }
        }

        private void tsClear_Click(object sender, EventArgs e)
        {
            ClearConsole();
        }
        #endregion

        #region File Interactions
        private void tsFileOpen_Click(object sender, EventArgs e)
        {
            if (PromptToSaveChanges())
            {
                DialogResult dialogResult = OpenFile();
                if (currentFilePath != null && dialogResult == DialogResult.OK)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5)}";
                    tabCodeControl.TabPages[tabCodeControl.SelectedIndex].Text = Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5).ToString() + " ×";
                }
            }
        }

        private void tsFileSaveAs_Click(object sender, EventArgs e)
        {
            if (currentFilePath[tabCodeControl.SelectedIndex] == null)
            {
                if (!isSaved)
                {
                    SaveFileAs();
                    if (currentFilePath != null)
                    {
                        this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5)}";
                        tabCodeControl.TabPages[tabCodeControl.SelectedIndex].Text = Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5).ToString() + " ×";
                    }
                }
            }
            else
            {
                DialogResult dialogResult = SaveFileAs();
                if (dialogResult == DialogResult.OK)
                {
                    tabCodeControl.TabPages[tabCodeControl.SelectedIndex].Text = Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5).ToString() + " ×";
                }
            }
        }

        private void tsFileSave_Click(object sender, EventArgs e)
        {
            if (currentFilePath[tabCodeControl.SelectedIndex] == null)
            {
                DialogResult dialogResult = SaveFileAs();
                if (dialogResult == DialogResult.OK)
                {
                    tabCodeControl.TabPages[tabCodeControl.SelectedIndex].Text = Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5).ToString() + " ×";
                }
            }
            else
            {
                TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
                CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
                File.WriteAllText(currentFilePath[tabCodeControl.SelectedIndex], txtCodeField.Text);
                isSaved = true;
                if (currentFilePath != null)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5)}";
                }
            }
        }

        private DialogResult OpenFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Ketchup Files (*.ktch)|*.ktch",
                Title = "File Open"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
                CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
                currentFilePath[tabCodeControl.SelectedIndex] = openFileDialog.FileName;
                txtCodeField.Text = File.ReadAllText(currentFilePath[tabCodeControl.SelectedIndex]);
                isSaved = true;
                if (currentFilePath != null)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5)}";
                }
                return DialogResult.OK;
            }
            return DialogResult.Cancel;
        }

        private DialogResult SaveFileAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "Custom Files (*.ktch)|*.ktch",
                Title = "Save File As"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
                CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
                currentFilePath[tabCodeControl.SelectedIndex] = saveFileDialog.FileName;
                File.WriteAllText(currentFilePath[tabCodeControl.SelectedIndex], txtCodeField.Text);
                isSaved = true;
                this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5)}";
                return DialogResult.OK;
            }
            else
            {
                return DialogResult.Cancel;
            }
        }

        private void SaveFile()
        {
            if (tabCodeControl.TabIndex > -1)
            {
                if (currentFilePath[tabCodeControl.SelectedIndex] == null)
                {
                    SaveFileAs();
                    isSaved = true;
                }
                else
                {
                    TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
                    CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
                    File.WriteAllText(currentFilePath[tabCodeControl.SelectedIndex], txtCodeField.Text);
                    isSaved = true;
                }
                if (currentFilePath[tabCodeControl.SelectedIndex] != null && tabCodeControl.SelectedTab != null)
                {
                    this.Text = $"Ketchup™️ IDE - {Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Remove(Path.GetFileName(currentFilePath[tabCodeControl.SelectedIndex]).Length - 5, 5)}";
                }
            }
            
        }

        private bool PromptToSaveChanges()
        {
            if (!isSaved)
            {
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

        #region Tabs
        private void tsFileNew_Click(object sender, EventArgs e)
        {
            TabPage tabPage = new TabPage();

            CustomFastColoredTextBox newTxtCodeField = new CustomFastColoredTextBox();

            newTxtCodeField.TextChanged += txtCodeField_TextChanged;
            newTxtCodeField.KeyDown += txtCodeField_KeyDown;
            newTxtCodeField.AutoIndentNeeded += txtCodeField_AutoIndentNeeded;
            newTxtCodeField.Dock = DockStyle.Fill;
            newTxtCodeField.LineNumberColor = Color.MidnightBlue;
            newTxtCodeField.Font = new Font("Courier New", 12);

            arrayCodeFields.Add(newTxtCodeField);
            currentFilePath.Add(null);

            tabPage.Controls.Add(newTxtCodeField);

            tabPage.Text = "<untitled> ×";

            tabCodeControl.Controls.Add(tabPage);

            tabCodeControl.SelectedTab = tabPage;

            this.Text = "Ketchup™️ IDE";
        }

        private void tabCodeControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabCodeControl.SelectedIndex != -1)
            {
                TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
                CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
                machine = new Machine(txtCodeField.Text, txtConsole.Text);
            }
        }
        #endregion
        #endregion

        private void CloseAllForms()
        {
            if (executionLoop != null && executionLoop.ThreadState == ThreadState.Suspended)
            {
                executionLoop.Resume();
                executionLoop.Abort();
            }
            else if (executionLoop != null)
            {
                executionLoop.Abort();
            }
            // Else to nothing - the thread is null and doesnt exist
            FormCollection openForms = System.Windows.Forms.Application.OpenForms;
            for (int i = 0; i < openForms.Count; i++)
            {
                openForms[i].Close();
            }
        }

        private void IDE_MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!PromptToSaveChanges())
            {
                e.Cancel = true;
            }
            CloseAllForms();
        }

        private void stripCopy_Click(object sender, EventArgs e)
        {
            TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
            CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;
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

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearConsole();
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

        #region Syntax Highlighting
        Style GreyStyle = new TextStyle(Brushes.Gray, null, FontStyle.Italic);
        Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Regular);
        Style PurpleStyle = new TextStyle(Brushes.Purple, null, FontStyle.Regular);
        Style PinkStyle = new TextStyle(Brushes.HotPink, null, FontStyle.Regular);
        Style OrangeStyle = new TextStyle(Brushes.DarkOrange, null, FontStyle.Regular);
        Style CyanStyle = new TextStyle(Brushes.DarkCyan, null, FontStyle.Regular);
        Style RedStyle = new TextStyle(Brushes.DarkRed, null, FontStyle.Regular);
        Style BlueStyle = new TextStyle(Brushes.DarkBlue, null, FontStyle.Regular);

        private void txtCodeField_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            unchangedCode = false;
            isSaved = false;
            // Regex explanation
            // \b - boundary character (beginning of a word)
            // (?i) - case insensitivity
            // | - OR operator
            // (?!\S) - Matches only whitespace characters ( ,\n,\t)
            // (?<=\bSTRING\s+) - The STRING text must preced the current position (Positive Lookback)

            e.ChangedRange.ClearStyle(GreyStyle);
            e.ChangedRange.ClearStyle(GreenStyle);
            e.ChangedRange.ClearStyle(PurpleStyle);
            e.ChangedRange.ClearStyle(PinkStyle);
            e.ChangedRange.ClearStyle(OrangeStyle);
            e.ChangedRange.ClearStyle(CyanStyle);
            e.ChangedRange.ClearStyle(RedStyle);
            e.ChangedRange.ClearStyle(BlueStyle);
            e.ChangedRange.SetStyle(GreyStyle, @"#.*");
            e.ChangedRange.SetStyle(GreenStyle, "(\".*?\")", RegexOptions.Singleline);
            e.ChangedRange.SetStyle(PurpleStyle, @"\b(?i)(print|input|message|(?<=\binput\s+)with\b|sort|swap)(?!\S)");
            e.ChangedRange.SetStyle(PinkStyle, @"\b(?i)(set|create|add|take|away|multiply|divide|get|remainder|raise|remove)(?!\S)");
            e.ChangedRange.SetStyle(OrangeStyle, @"\b(?i)(count|while|do|repeat|if|else|function|procedure|then|as|times|not|and|or)(?!\S)");
            e.ChangedRange.SetStyle(BlueStyle, @"\b(?i)(true|false)(?!\S)");
            e.ChangedRange.SetStyle(CyanStyle, @"\b(?i)(to|from|with|going|up|down|by|the|power|(?<=\bpower\s+)of|divided|in)(?!\S)");
            e.ChangedRange.SetStyle(RedStyle, @"\b(?i)(end|return)(?!\S)");

            UpdateCaretPosition();
        }
        #endregion

        #region Indentation (Formatting)
        private void txtCodeField_AutoIndentNeeded(object sender, AutoIndentEventArgs e)
        {
            TabPage currentTab = tabCodeControl.SelectedTab as TabPage;
            CustomFastColoredTextBox txtCodeField = currentTab.Controls[0] as CustomFastColoredTextBox;

            string trimmedLine = e.LineText.Trim();

            // Ignoring case in all words
            // \s* - accounts for tabspaces
            // COUNT statement format:
            // COUNT WITH var FROM (num|var) TO (num|var) (GOING (UP|DOWN) BY (num|var))?
            // FUNCTION definition format:
            // FUNCTION subroutineName (var,var,var)
            // subroutineName must begin with a letter or an underscore
            // The list of parameters must end with a single var with no comma after it
            // IF statement format:
            // IF anything (SPECIFY) THEN
            // ELSE statement format:
            // ELSE (IF anything THEN)?
            // REPEAT statement format:
            // REPEAT (num|var) TIMES

            // Variables have the following format [a-zA-Z_][a-zA-Z0-9_], not starting with a digit
            Regex blockStartRegex = new Regex(@"^\s*(count\s+with\s+[a-zA-Z_][a-zA-Z0-9_]*\s+from\s+[a-zA-Z0-9_]+\s+to\s+(([a-zA-Z0-9_]+\s+going\s+(up|down)\s+by\s+[a-zA-Z0-9_])|[a-zA-Z0-9_]+)|function\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\((([a-zA-Z_][a-zA-Z0-9_]*,)?[a-zA-Z_][a-zA-Z0-9_]*)?\)|procedure\s+[a-zA-Z_][a-zA-Z0-9_]*\s*\((([a-zA-Z_][a-zA-Z0-9_]*,)?[a-zA-Z_][a-zA-Z0-9_]*)?\)|if\s+.+then|repeat\s+[a-zA-Z0-9_]+\stimes|while\s+.+then|do)$", RegexOptions.IgnoreCase);
            Regex blockEndRegex = new Regex(@"^\s*end", RegexOptions.IgnoreCase);
            Regex blockElseRegex = new Regex(@"^\s*else", RegexOptions.IgnoreCase);

            if (blockEndRegex.IsMatch(trimmedLine))
            {
                e.Shift = -e.TabLength;
                e.ShiftNextLines = -e.TabLength;
                return;
            }
            else if (blockElseRegex.IsMatch(trimmedLine))
            {
                e.Shift = -e.TabLength;
            }
            else
            {
                int lineNumber = txtCodeField.Selection.Start.iLine;
                if (lineNumber > 0)
                {
                    string prevLine = txtCodeField.Lines[lineNumber - 1].Trim();
                    if (blockStartRegex.IsMatch(prevLine))
                    {
                        e.ShiftNextLines = e.TabLength;
                        return;
                    }
                }
            }
        }

        private void stripFormat_Click(object sender, EventArgs e)
        {
            int selectionStart = txtCodeField.SelectionStart;
            int selectionLength = txtCodeField.SelectionLength;
            string usersCode = txtCodeField.Text;

            // Get Tokens and sort by need for capitalisation
            machine = new Machine(usersCode, txtConsole.Text);

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

            txtCodeField.SelectionStart = selectionStart;
            txtCodeField.SelectionLength = selectionLength;
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
        #endregion

        #region Tab Deletion
        private void tabCodeControl_MouseClick(object sender, MouseEventArgs e)
        {
            Graphics g = tabCodeControl.CreateGraphics();

            if (tabCodeControl == null) return;

            for (int i = 0; i < tabCodeControl.TabCount; i++)
            {
                // Entire tab as a rectangle
                Rectangle tabRect = tabCodeControl.GetTabRect(i);

                TabPage tab = tabCodeControl.TabPages[i];

                // Size of the text excluding the X
                SizeF textSize = g.MeasureString(tab.Text.Substring(0, tab.Text.Length - 2), tabCodeControl.Font);

                // Represents the clickable (to select) part of the tab
                Rectangle textRect = new Rectangle(tabRect.X,tabRect.Y, (int)textSize.Width, tabRect.Height);

                // If the click is not on the selecting area of the tab -> Delete the tab
                if (!textRect.Contains(e.Location) && tabRect.Contains(e.Location))
                {
                    int prevTabIndex = tabCodeControl.SelectedIndex;
                    tabCodeControl.SelectedIndex = i;
                    tabCodeControl_SelectedIndexChanged(sender, e);
                    if (PromptToSaveChanges())
                    {
                        tabCodeControl.Controls.RemoveAt(i);
                    }
                    tabCodeControl.SelectedIndex = prevTabIndex;
                    break;
                }
            }
        }
        #endregion

        private void stripNewFile_Click(object sender, EventArgs e)
        {
            tsFileNew_Click(sender, e);
        }

        private void stripOpenFile_Click(object sender, EventArgs e)
        {
            tsFileOpen_Click(sender, e);
        }

        private void stripSave_Click(object sender, EventArgs e)
        {
            tsFileSave_Click(sender, e);
        }

        private void stripSaveAs_Click(object sender, EventArgs e)
        {
            tsFileSaveAs_Click(sender, e);
        }

        private void stripPaste_Click(object sender, EventArgs e)
        {
            tsEditPaste_Click(sender, e);
        }
    }
}