namespace NEA
{
    partial class IntermediateView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntermediateView));
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtIntermediateCode = new System.Windows.Forms.RichTextBox();
            this.tableMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblName = new System.Windows.Forms.Label();
            this.tabControlIntermediate = new System.Windows.Forms.TabControl();
            this.tabMain = new System.Windows.Forms.TabPage();
            this.tableMain.SuspendLayout();
            this.tabControlIntermediate.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDescription
            // 
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(3, 514);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(644, 194);
            this.txtDescription.TabIndex = 2;
            // 
            // txtIntermediateCode
            // 
            this.txtIntermediateCode.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtIntermediateCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtIntermediateCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtIntermediateCode.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIntermediateCode.Location = new System.Drawing.Point(3, 3);
            this.txtIntermediateCode.Name = "txtIntermediateCode";
            this.txtIntermediateCode.ReadOnly = true;
            this.txtIntermediateCode.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtIntermediateCode.Size = new System.Drawing.Size(630, 436);
            this.txtIntermediateCode.TabIndex = 3;
            this.txtIntermediateCode.Text = "";
            this.txtIntermediateCode.Click += new System.EventHandler(this.txtIntermediateCode_Click);
            // 
            // tableMain
            // 
            this.tableMain.ColumnCount = 1;
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMain.Controls.Add(this.txtDescription, 0, 2);
            this.tableMain.Controls.Add(this.lblName, 0, 1);
            this.tableMain.Controls.Add(this.tabControlIntermediate, 0, 0);
            this.tableMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableMain.Location = new System.Drawing.Point(0, 0);
            this.tableMain.Name = "tableMain";
            this.tableMain.RowCount = 3;
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableMain.Size = new System.Drawing.Size(650, 711);
            this.tableMain.TabIndex = 4;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblName.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(3, 481);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(644, 30);
            this.lblName.TabIndex = 4;
            this.lblName.Text = "[Click on a line]";
            // 
            // tabControlIntermediate
            // 
            this.tabControlIntermediate.Controls.Add(this.tabMain);
            this.tabControlIntermediate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlIntermediate.Location = new System.Drawing.Point(3, 3);
            this.tabControlIntermediate.Name = "tabControlIntermediate";
            this.tabControlIntermediate.SelectedIndex = 0;
            this.tabControlIntermediate.Size = new System.Drawing.Size(644, 475);
            this.tabControlIntermediate.TabIndex = 5;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.txtIntermediateCode);
            this.tabMain.Location = new System.Drawing.Point(4, 29);
            this.tabMain.Name = "tabMain";
            this.tabMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabMain.Size = new System.Drawing.Size(636, 442);
            this.tabMain.TabIndex = 0;
            this.tabMain.Text = "Main Branch";
            this.tabMain.UseVisualStyleBackColor = true;
            // 
            // IntermediateView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 711);
            this.Controls.Add(this.tableMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IntermediateView";
            this.Text = "Intermediate View";
            this.Load += new System.EventHandler(this.IntermediateView_Load);
            this.tableMain.ResumeLayout(false);
            this.tableMain.PerformLayout();
            this.tabControlIntermediate.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.RichTextBox txtIntermediateCode;
        private System.Windows.Forms.TableLayoutPanel tableMain;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TabControl tabControlIntermediate;
        private System.Windows.Forms.TabPage tabMain;
    }
}