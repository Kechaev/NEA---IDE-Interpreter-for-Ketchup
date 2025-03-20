using System.Windows.Forms;

namespace NEA
{
    partial class IDE_MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IDE_MainWindow));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.tsFile = new System.Windows.Forms.ToolStripMenuItem();
            this.tsFileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.tsFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.tsFileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.tsFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.tsFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsEditCut = new System.Windows.Forms.ToolStripMenuItem();
            this.tsEditCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.tsEditPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.tsView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsViewConsoleView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsIntermediateCodeView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsTokenView = new System.Windows.Forms.ToolStripMenuItem();
            this.tsConsole = new System.Windows.Forms.ToolStripMenuItem();
            this.tsClear = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDebugTraceTable = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDebugSyntaxCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDebugBreakpoints = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.stripUndo = new System.Windows.Forms.ToolStripButton();
            this.stripRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.stripNewFile = new System.Windows.Forms.ToolStripButton();
            this.stripOpenFile = new System.Windows.Forms.ToolStripButton();
            this.stripSave = new System.Windows.Forms.ToolStripButton();
            this.stripSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.stripRun = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.stripComment = new System.Windows.Forms.ToolStripButton();
            this.stripFormat = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.stripCopy = new System.Windows.Forms.ToolStripButton();
            this.stripPaste = new System.Windows.Forms.ToolStripButton();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusLineInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusColumnInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.tableMain = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tableCodeSpace = new System.Windows.Forms.TableLayoutPanel();
            this.tabCodeControl = new System.Windows.Forms.TabControl();
            this.tab0 = new System.Windows.Forms.TabPage();
            this.txtCodeField = new NEA.Classes.CustomFastColoredTextBox();
            this.tableConsole = new System.Windows.Forms.TableLayoutPanel();
            this.btnCopy = new System.Windows.Forms.Button();
            this.lblConsole = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCopyLastProgram = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.tableMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tableCodeSpace.SuspendLayout();
            this.tabCodeControl.SuspendLayout();
            this.tab0.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtCodeField)).BeginInit();
            this.tableConsole.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsFile,
            this.tsEdit,
            this.tsView,
            this.tsConsole});
            resources.ApplyResources(this.menuStrip, "menuStrip");
            this.menuStrip.Name = "menuStrip";
            // 
            // tsFile
            // 
            this.tsFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsFileNew,
            this.tsFileOpen,
            this.tsFileSave,
            this.tsFileSaveAs,
            this.tsFileExit});
            this.tsFile.Name = "tsFile";
            resources.ApplyResources(this.tsFile, "tsFile");
            // 
            // tsFileNew
            // 
            this.tsFileNew.Name = "tsFileNew";
            resources.ApplyResources(this.tsFileNew, "tsFileNew");
            this.tsFileNew.Click += new System.EventHandler(this.tsFileNew_Click);
            // 
            // tsFileOpen
            // 
            this.tsFileOpen.Name = "tsFileOpen";
            resources.ApplyResources(this.tsFileOpen, "tsFileOpen");
            this.tsFileOpen.Click += new System.EventHandler(this.tsFileOpen_Click);
            // 
            // tsFileSave
            // 
            this.tsFileSave.Name = "tsFileSave";
            resources.ApplyResources(this.tsFileSave, "tsFileSave");
            this.tsFileSave.Click += new System.EventHandler(this.tsFileSave_Click);
            // 
            // tsFileSaveAs
            // 
            this.tsFileSaveAs.Name = "tsFileSaveAs";
            resources.ApplyResources(this.tsFileSaveAs, "tsFileSaveAs");
            this.tsFileSaveAs.Click += new System.EventHandler(this.tsFileSaveAs_Click);
            // 
            // tsFileExit
            // 
            this.tsFileExit.Name = "tsFileExit";
            resources.ApplyResources(this.tsFileExit, "tsFileExit");
            this.tsFileExit.Click += new System.EventHandler(this.tsFileExit_Click);
            // 
            // tsEdit
            // 
            this.tsEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsEditCut,
            this.tsEditCopy,
            this.tsEditPaste});
            this.tsEdit.Name = "tsEdit";
            resources.ApplyResources(this.tsEdit, "tsEdit");
            // 
            // tsEditCut
            // 
            this.tsEditCut.Name = "tsEditCut";
            resources.ApplyResources(this.tsEditCut, "tsEditCut");
            this.tsEditCut.Click += new System.EventHandler(this.tsEditCut_Click);
            // 
            // tsEditCopy
            // 
            this.tsEditCopy.Name = "tsEditCopy";
            resources.ApplyResources(this.tsEditCopy, "tsEditCopy");
            this.tsEditCopy.Click += new System.EventHandler(this.tsEditCopy_Click);
            // 
            // tsEditPaste
            // 
            this.tsEditPaste.Name = "tsEditPaste";
            resources.ApplyResources(this.tsEditPaste, "tsEditPaste");
            this.tsEditPaste.Click += new System.EventHandler(this.tsEditPaste_Click);
            // 
            // tsView
            // 
            this.tsView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsViewConsoleView,
            this.tsIntermediateCodeView,
            this.tsTokenView});
            this.tsView.Name = "tsView";
            resources.ApplyResources(this.tsView, "tsView");
            // 
            // tsViewConsoleView
            // 
            this.tsViewConsoleView.Name = "tsViewConsoleView";
            resources.ApplyResources(this.tsViewConsoleView, "tsViewConsoleView");
            // 
            // tsIntermediateCodeView
            // 
            this.tsIntermediateCodeView.Name = "tsIntermediateCodeView";
            resources.ApplyResources(this.tsIntermediateCodeView, "tsIntermediateCodeView");
            this.tsIntermediateCodeView.Click += new System.EventHandler(this.tsIntermediateView_Click);
            // 
            // tsTokenView
            // 
            this.tsTokenView.Name = "tsTokenView";
            resources.ApplyResources(this.tsTokenView, "tsTokenView");
            this.tsTokenView.Click += new System.EventHandler(this.tsTokenView_Click);
            // 
            // tsConsole
            // 
            this.tsConsole.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsClear});
            this.tsConsole.Name = "tsConsole";
            resources.ApplyResources(this.tsConsole, "tsConsole");
            // 
            // tsClear
            // 
            this.tsClear.Name = "tsClear";
            resources.ApplyResources(this.tsClear, "tsClear");
            this.tsClear.Click += new System.EventHandler(this.tsClear_Click);
            // 
            // tsDebugTraceTable
            // 
            this.tsDebugTraceTable.Name = "tsDebugTraceTable";
            resources.ApplyResources(this.tsDebugTraceTable, "tsDebugTraceTable");
            // 
            // tsDebugSyntaxCheck
            // 
            this.tsDebugSyntaxCheck.Name = "tsDebugSyntaxCheck";
            resources.ApplyResources(this.tsDebugSyntaxCheck, "tsDebugSyntaxCheck");
            // 
            // tsDebugBreakpoints
            // 
            this.tsDebugBreakpoints.Name = "tsDebugBreakpoints";
            resources.ApplyResources(this.tsDebugBreakpoints, "tsDebugBreakpoints");
            // 
            // toolStrip
            // 
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stripUndo,
            this.stripRedo,
            this.toolStripSeparator4,
            this.stripNewFile,
            this.stripOpenFile,
            this.stripSave,
            this.stripSaveAs,
            this.toolStripSeparator2,
            this.stripRun,
            this.toolStripSeparator1,
            this.stripComment,
            this.stripFormat,
            this.toolStripSeparator3,
            this.stripCopy,
            this.stripPaste});
            resources.ApplyResources(this.toolStrip, "toolStrip");
            this.toolStrip.Name = "toolStrip";
            // 
            // stripUndo
            // 
            this.stripUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripUndo, "stripUndo");
            this.stripUndo.Name = "stripUndo";
            this.stripUndo.Click += new System.EventHandler(this.stripUndo_Click);
            // 
            // stripRedo
            // 
            this.stripRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripRedo, "stripRedo");
            this.stripRedo.Name = "stripRedo";
            this.stripRedo.Click += new System.EventHandler(this.stripRedo_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // stripNewFile
            // 
            this.stripNewFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripNewFile, "stripNewFile");
            this.stripNewFile.Name = "stripNewFile";
            this.stripNewFile.Click += new System.EventHandler(this.stripNewFile_Click);
            // 
            // stripOpenFile
            // 
            this.stripOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripOpenFile, "stripOpenFile");
            this.stripOpenFile.Name = "stripOpenFile";
            this.stripOpenFile.Click += new System.EventHandler(this.stripOpenFile_Click);
            // 
            // stripSave
            // 
            this.stripSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripSave, "stripSave");
            this.stripSave.Name = "stripSave";
            this.stripSave.Click += new System.EventHandler(this.stripSave_Click);
            // 
            // stripSaveAs
            // 
            this.stripSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripSaveAs, "stripSaveAs");
            this.stripSaveAs.Name = "stripSaveAs";
            this.stripSaveAs.Click += new System.EventHandler(this.stripSaveAs_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // stripRun
            // 
            this.stripRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripRun, "stripRun");
            this.stripRun.Name = "stripRun";
            this.stripRun.Click += new System.EventHandler(this.stripRun_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // stripComment
            // 
            this.stripComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripComment, "stripComment");
            this.stripComment.Name = "stripComment";
            this.stripComment.Click += new System.EventHandler(this.stripComment_Click);
            // 
            // stripFormat
            // 
            this.stripFormat.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripFormat, "stripFormat");
            this.stripFormat.Name = "stripFormat";
            this.stripFormat.Click += new System.EventHandler(this.stripFormat_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // stripCopy
            // 
            this.stripCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripCopy, "stripCopy");
            this.stripCopy.Name = "stripCopy";
            this.stripCopy.Click += new System.EventHandler(this.stripCopy_Click);
            // 
            // stripPaste
            // 
            this.stripPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripPaste, "stripPaste");
            this.stripPaste.Name = "stripPaste";
            this.stripPaste.Click += new System.EventHandler(this.stripPaste_Click);
            // 
            // statusBar
            // 
            this.statusBar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLineInfo,
            this.statusColumnInfo});
            resources.ApplyResources(this.statusBar, "statusBar");
            this.statusBar.Name = "statusBar";
            // 
            // statusLineInfo
            // 
            this.statusLineInfo.Name = "statusLineInfo";
            resources.ApplyResources(this.statusLineInfo, "statusLineInfo");
            // 
            // statusColumnInfo
            // 
            this.statusColumnInfo.Name = "statusColumnInfo";
            resources.ApplyResources(this.statusColumnInfo, "statusColumnInfo");
            // 
            // txtConsole
            // 
            this.txtConsole.BackColor = System.Drawing.SystemColors.MenuBar;
            this.txtConsole.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableConsole.SetColumnSpan(this.txtConsole, 5);
            resources.ApplyResources(this.txtConsole, "txtConsole");
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            // 
            // tableMain
            // 
            resources.ApplyResources(this.tableMain, "tableMain");
            this.tableMain.Controls.Add(this.splitContainer, 0, 1);
            this.tableMain.Name = "tableMain";
            // 
            // splitContainer
            // 
            resources.ApplyResources(this.splitContainer, "splitContainer");
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.tableCodeSpace);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.tableConsole);
            // 
            // tableCodeSpace
            // 
            this.tableCodeSpace.BackColor = System.Drawing.SystemColors.ButtonFace;
            resources.ApplyResources(this.tableCodeSpace, "tableCodeSpace");
            this.tableCodeSpace.Controls.Add(this.tabCodeControl, 0, 0);
            this.tableCodeSpace.Name = "tableCodeSpace";
            // 
            // tabCodeControl
            // 
            this.tabCodeControl.Controls.Add(this.tab0);
            resources.ApplyResources(this.tabCodeControl, "tabCodeControl");
            this.tabCodeControl.Name = "tabCodeControl";
            this.tabCodeControl.SelectedIndex = 0;
            this.tabCodeControl.SelectedIndexChanged += new System.EventHandler(this.tabCodeControl_SelectedIndexChanged);
            this.tabCodeControl.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tabCodeControl_MouseClick);
            // 
            // tab0
            // 
            this.tab0.Controls.Add(this.txtCodeField);
            resources.ApplyResources(this.tab0, "tab0");
            this.tab0.Name = "tab0";
            this.tab0.UseVisualStyleBackColor = true;
            // 
            // txtCodeField
            // 
            this.txtCodeField.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
            this.txtCodeField.AutoIndentCharsPatterns = "^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>[^;=]+);\r\n^\\s*(case|default)\\s*[^:]*" +
    "(?<range>:)\\s*(?<range>[^;]+);";
            resources.ApplyResources(this.txtCodeField, "txtCodeField");
            this.txtCodeField.BackBrush = null;
            this.txtCodeField.CharHeight = 27;
            this.txtCodeField.CharWidth = 14;
            this.txtCodeField.CommentPrefix = "#";
            this.txtCodeField.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtCodeField.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.txtCodeField.IsReplaceMode = false;
            this.txtCodeField.LineNumberColor = System.Drawing.Color.MidnightBlue;
            this.txtCodeField.Name = "txtCodeField";
            this.txtCodeField.Paddings = new System.Windows.Forms.Padding(0);
            this.txtCodeField.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.txtCodeField.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("txtCodeField.ServiceColors")));
            this.txtCodeField.Zoom = 100;
            this.txtCodeField.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.txtCodeField_TextChanged);
            this.txtCodeField.AutoIndentNeeded += new System.EventHandler<FastColoredTextBoxNS.AutoIndentEventArgs>(this.txtCodeField_AutoIndentNeeded);
            this.txtCodeField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCodeField_KeyDown);
            // 
            // tableConsole
            // 
            resources.ApplyResources(this.tableConsole, "tableConsole");
            this.tableConsole.Controls.Add(this.txtConsole, 0, 1);
            this.tableConsole.Controls.Add(this.btnCopy, 1, 0);
            this.tableConsole.Controls.Add(this.lblConsole, 0, 0);
            this.tableConsole.Controls.Add(this.btnClear, 3, 0);
            this.tableConsole.Controls.Add(this.btnCopyLastProgram, 2, 0);
            this.tableConsole.Name = "tableConsole";
            // 
            // btnCopy
            // 
            resources.ApplyResources(this.btnCopy, "btnCopy");
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // lblConsole
            // 
            resources.ApplyResources(this.lblConsole, "lblConsole");
            this.lblConsole.Name = "lblConsole";
            // 
            // btnClear
            // 
            resources.ApplyResources(this.btnClear, "btnClear");
            this.btnClear.Name = "btnClear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCopyLastProgram
            // 
            resources.ApplyResources(this.btnCopyLastProgram, "btnCopyLastProgram");
            this.btnCopyLastProgram.Name = "btnCopyLastProgram";
            this.btnCopyLastProgram.UseVisualStyleBackColor = true;
            this.btnCopyLastProgram.Click += new System.EventHandler(this.btnCopyLastProgram_Click);
            // 
            // tabPage2
            // 
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // IDE_MainWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.tableMain);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "IDE_MainWindow";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IDE_MainWindow_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCodeField_KeyDown);
            this.Leave += new System.EventHandler(this.tsFileExit_Click);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.tableMain.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.tableCodeSpace.ResumeLayout(false);
            this.tabCodeControl.ResumeLayout(false);
            this.tab0.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtCodeField)).EndInit();
            this.tableConsole.ResumeLayout(false);
            this.tableConsole.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem tsFile;
        private System.Windows.Forms.ToolStripMenuItem tsFileNew;
        private System.Windows.Forms.ToolStripMenuItem tsFileOpen;
        private System.Windows.Forms.ToolStripMenuItem tsFileSave;
        private System.Windows.Forms.ToolStripMenuItem tsFileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem tsFileExit;
        private System.Windows.Forms.ToolStripMenuItem tsEdit;
        private System.Windows.Forms.ToolStripMenuItem tsEditCut;
        private System.Windows.Forms.ToolStripMenuItem tsEditCopy;
        private System.Windows.Forms.ToolStripMenuItem tsEditPaste;
        private System.Windows.Forms.ToolStripMenuItem tsView;
        private System.Windows.Forms.ToolStripMenuItem tsViewConsoleView;
        private System.Windows.Forms.ToolStripMenuItem tsDebugTraceTable;
        private System.Windows.Forms.ToolStripMenuItem tsDebugSyntaxCheck;
        private System.Windows.Forms.ToolStripMenuItem tsDebugBreakpoints;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton stripUndo;
        private System.Windows.Forms.ToolStripButton stripRedo;
        private System.Windows.Forms.ToolStripButton stripRun;
        private System.Windows.Forms.ToolStripButton stripComment;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel statusLineInfo;
        private System.Windows.Forms.ToolStripStatusLabel statusColumnInfo;
        internal TextBox txtConsole;
        private ToolStripMenuItem tsIntermediateCodeView;
        private TableLayoutPanel tableMain;
        private TableLayoutPanel tableCodeSpace;
        private TableLayoutPanel tableConsole;
        private Label lblConsole;
        private ToolStripMenuItem tsTokenView;
        private ToolStripMenuItem tsConsole;
        private ToolStripMenuItem tsClear;
        private ToolStripButton stripFormat;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripButton stripCopy;
        private SplitContainer splitContainer;
        private Button btnCopy;
        private Button btnClear;
        private Button btnCopyLastProgram;
        private TabControl tabCodeControl;
        private TabPage tabPage2;
        private TabPage tab0;
        private ToolStripButton stripNewFile;
        private ToolStripButton stripOpenFile;
        private ToolStripButton stripSave;
        private ToolStripButton stripSaveAs;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton stripPaste;
        private Classes.CustomFastColoredTextBox txtCodeField;
    }
}

