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
            this.tsDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDebugTraceTable = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDebugSyntaxCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDebugBreakpoints = new System.Windows.Forms.ToolStripMenuItem();
            this.tsConsole = new System.Windows.Forms.ToolStripMenuItem();
            this.tsClear = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.stripUndo = new System.Windows.Forms.ToolStripButton();
            this.stripRedo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.stripRun = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.stripComment = new System.Windows.Forms.ToolStripButton();
            this.stripFormat = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.stripCopy = new System.Windows.Forms.ToolStripButton();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusLineInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusColumnInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.txtLineNumber = new System.Windows.Forms.RichTextBox();
            this.txtCodeField = new System.Windows.Forms.RichTextBox();
            this.tableMain = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tableCodeSpace = new System.Windows.Forms.TableLayoutPanel();
            this.tableConsole = new System.Windows.Forms.TableLayoutPanel();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblConsole = new System.Windows.Forms.Label();
            this.btnCopyLastProgram = new System.Windows.Forms.Button();
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.tableMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tableCodeSpace.SuspendLayout();
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
            this.tsDebug,
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
            // tsDebug
            // 
            this.tsDebug.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsDebugTraceTable,
            this.tsDebugSyntaxCheck,
            this.tsDebugBreakpoints});
            this.tsDebug.Name = "tsDebug";
            resources.ApplyResources(this.tsDebug, "tsDebug");
            // 
            // tsDebugTraceTable
            // 
            this.tsDebugTraceTable.Name = "tsDebugTraceTable";
            resources.ApplyResources(this.tsDebugTraceTable, "tsDebugTraceTable");
            this.tsDebugTraceTable.Click += new System.EventHandler(this.tsDebugTraceTable_Click);
            // 
            // tsDebugSyntaxCheck
            // 
            this.tsDebugSyntaxCheck.Name = "tsDebugSyntaxCheck";
            resources.ApplyResources(this.tsDebugSyntaxCheck, "tsDebugSyntaxCheck");
            this.tsDebugSyntaxCheck.Click += new System.EventHandler(this.tsDebugSyntaxCheck_Click);
            // 
            // tsDebugBreakpoints
            // 
            this.tsDebugBreakpoints.Name = "tsDebugBreakpoints";
            resources.ApplyResources(this.tsDebugBreakpoints, "tsDebugBreakpoints");
            this.tsDebugBreakpoints.Click += new System.EventHandler(this.tsDebugBreakpoints_Click);
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
            // toolStrip
            // 
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stripUndo,
            this.stripRedo,
            this.toolStripSeparator2,
            this.stripRun,
            this.toolStripSeparator1,
            this.stripComment,
            this.stripFormat,
            this.toolStripSeparator3,
            this.stripCopy});
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
            this.stripFormat.Image = global::NEA.Properties.Resources.Indent_Icon;
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
            // txtLineNumber
            // 
            this.txtLineNumber.BackColor = System.Drawing.SystemColors.Control;
            this.txtLineNumber.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.txtLineNumber, "txtLineNumber");
            this.txtLineNumber.ForeColor = System.Drawing.SystemColors.WindowText;
            this.txtLineNumber.Name = "txtLineNumber";
            this.txtLineNumber.ReadOnly = true;
            // 
            // txtCodeField
            // 
            this.txtCodeField.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.txtCodeField, "txtCodeField");
            this.txtCodeField.Name = "txtCodeField";
            this.txtCodeField.ContentsResized += new System.Windows.Forms.ContentsResizedEventHandler(this.txtCodeField_ContentsResized);
            this.txtCodeField.SelectionChanged += new System.EventHandler(this.txtCodeField_SelectionChanged_1);
            this.txtCodeField.VScroll += new System.EventHandler(this.txtCodeField_VScroll);
            this.txtCodeField.TextChanged += new System.EventHandler(this.txtCodeField_TextChanged);
            this.txtCodeField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCodeField_KeyDown);
            // 
            // tableMain
            // 
            resources.ApplyResources(this.tableMain, "tableMain");
            this.tableMain.Controls.Add(this.splitContainer, 1, 1);
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
            this.tableCodeSpace.Controls.Add(this.txtLineNumber, 0, 0);
            this.tableCodeSpace.Controls.Add(this.txtCodeField, 1, 0);
            this.tableCodeSpace.Name = "tableCodeSpace";
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
            // btnClear
            // 
            resources.ApplyResources(this.btnClear, "btnClear");
            this.btnClear.Name = "btnClear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click_1);
            // 
            // lblConsole
            // 
            resources.ApplyResources(this.lblConsole, "lblConsole");
            this.lblConsole.Name = "lblConsole";
            // 
            // btnCopyLastProgram
            // 
            resources.ApplyResources(this.btnCopyLastProgram, "btnCopyLastProgram");
            this.btnCopyLastProgram.Name = "btnCopyLastProgram";
            this.btnCopyLastProgram.UseVisualStyleBackColor = true;
            this.btnCopyLastProgram.Click += new System.EventHandler(this.btnCopyLastProgram_Click);
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
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IDE_MainWindow_FormClosing);
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
        private System.Windows.Forms.ToolStripMenuItem tsDebug;
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
        private RichTextBox txtLineNumber;
        private RichTextBox txtCodeField;
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
    }
}

