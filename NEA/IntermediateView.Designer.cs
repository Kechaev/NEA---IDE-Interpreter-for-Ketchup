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
            this.tableMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDescription
            // 
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDescription.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(3, 514);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(644, 194);
            this.txtDescription.TabIndex = 2;
            this.txtDescription.Text = "Lorum Ipsum";
            this.txtDescription.Visible = false;
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
            this.txtIntermediateCode.Size = new System.Drawing.Size(644, 475);
            this.txtIntermediateCode.TabIndex = 3;
            this.txtIntermediateCode.Text = "";
            // 
            // tableMain
            // 
            this.tableMain.ColumnCount = 1;
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMain.Controls.Add(this.txtIntermediateCode, 0, 0);
            this.tableMain.Controls.Add(this.txtDescription, 0, 2);
            this.tableMain.Controls.Add(this.lblName, 0, 1);
            this.tableMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableMain.Location = new System.Drawing.Point(0, 0);
            this.tableMain.Name = "tableMain";
            this.tableMain.RowCount = 3;
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableMain.Size = new System.Drawing.Size(650, 711);
            this.tableMain.TabIndex = 4;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblName.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(5, 722);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(966, 45);
            this.lblName.TabIndex = 4;
            this.lblName.Text = "[Click on a line]";
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
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.RichTextBox txtIntermediateCode;
        private System.Windows.Forms.TableLayoutPanel tableMain;
        private System.Windows.Forms.Label lblName;
    }
}