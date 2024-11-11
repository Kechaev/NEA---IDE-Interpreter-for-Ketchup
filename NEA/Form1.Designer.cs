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
            this.tsDebug = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDebugTraceTable = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDebugSyntaxCheck = new System.Windows.Forms.ToolStripMenuItem();
            this.tsDebugBreakpoints = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.stripUndo = new System.Windows.Forms.ToolStripButton();
            this.stripRedo = new System.Windows.Forms.ToolStripButton();
            this.stripRun = new System.Windows.Forms.ToolStripButton();
            this.stripComment = new System.Windows.Forms.ToolStripButton();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusLineInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusColumnInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnMinusFont = new System.Windows.Forms.Button();
            this.btnPlusFont = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.txtConsole = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLineNumber = new System.Windows.Forms.RichTextBox();
            this.txtCodeField = new System.Windows.Forms.RichTextBox();
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.statusBar.SuspendLayout();
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
            this.tsDebug});
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
            // 
            // tsFileOpen
            // 
            this.tsFileOpen.Name = "tsFileOpen";
            resources.ApplyResources(this.tsFileOpen, "tsFileOpen");
            // 
            // tsFileSave
            // 
            this.tsFileSave.Name = "tsFileSave";
            resources.ApplyResources(this.tsFileSave, "tsFileSave");
            // 
            // tsFileSaveAs
            // 
            this.tsFileSaveAs.Name = "tsFileSaveAs";
            resources.ApplyResources(this.tsFileSaveAs, "tsFileSaveAs");
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
            this.tsViewConsoleView});
            this.tsView.Name = "tsView";
            resources.ApplyResources(this.tsView, "tsView");
            // 
            // tsViewConsoleView
            // 
            this.tsViewConsoleView.Name = "tsViewConsoleView";
            resources.ApplyResources(this.tsViewConsoleView, "tsViewConsoleView");
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
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stripUndo,
            this.stripRedo,
            this.stripRun,
            this.stripComment});
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
            // stripRun
            // 
            this.stripRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripRun, "stripRun");
            this.stripRun.Name = "stripRun";
            this.stripRun.Click += new System.EventHandler(this.stripRun_Click);
            // 
            // stripComment
            // 
            this.stripComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.stripComment, "stripComment");
            this.stripComment.Name = "stripComment";
            this.stripComment.Click += new System.EventHandler(this.stripComment_Click);
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
            // btnClear
            // 
            resources.ApplyResources(this.btnClear, "btnClear");
            this.btnClear.Name = "btnClear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnMinusFont
            // 
            resources.ApplyResources(this.btnMinusFont, "btnMinusFont");
            this.btnMinusFont.Name = "btnMinusFont";
            this.btnMinusFont.UseVisualStyleBackColor = true;
            this.btnMinusFont.Click += new System.EventHandler(this.btnMinusFont_Click);
            // 
            // btnPlusFont
            // 
            resources.ApplyResources(this.btnPlusFont, "btnPlusFont");
            this.btnPlusFont.Name = "btnPlusFont";
            this.btnPlusFont.UseVisualStyleBackColor = true;
            this.btnPlusFont.Click += new System.EventHandler(this.btnPlusFont_Click);
            // 
            // btnReset
            // 
            resources.ApplyResources(this.btnReset, "btnReset");
            this.btnReset.Name = "btnReset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // txtConsole
            // 
            this.txtConsole.BackColor = System.Drawing.SystemColors.MenuBar;
            this.txtConsole.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            resources.ApplyResources(this.txtConsole, "txtConsole");
            this.txtConsole.Name = "txtConsole";
            this.txtConsole.ReadOnly = true;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
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
            this.txtCodeField.BackColor = System.Drawing.SystemColors.Window;
            this.txtCodeField.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.txtCodeField, "txtCodeField");
            this.txtCodeField.Name = "txtCodeField";
            this.txtCodeField.VScroll += new System.EventHandler(this.txtCodeField_VScroll);
            this.txtCodeField.TextChanged += new System.EventHandler(this.txtCodeField_TextChanged);
            this.txtCodeField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCodeField_KeyDown);
            // 
            // IDE_MainWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtCodeField);
            this.Controls.Add(this.txtLineNumber);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.btnPlusFont);
            this.Controls.Add(this.btnMinusFont);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.txtConsole);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "IDE_MainWindow";
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
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
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnMinusFont;
        private System.Windows.Forms.Button btnPlusFont;
        private System.Windows.Forms.Button btnReset;
        private TextBox txtConsole;
        private Label label1;
        private RichTextBox txtLineNumber;
        private RichTextBox txtCodeField;
    }
}

