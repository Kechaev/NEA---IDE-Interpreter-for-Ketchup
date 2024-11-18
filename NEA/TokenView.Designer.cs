namespace NEA.Classes
{
    partial class TokenView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TokenView));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtIntermediateCode = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.txtIntermediateCode, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(5, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(502, 685);
            this.tableLayoutPanel1.TabIndex = 0;
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
            this.txtIntermediateCode.Size = new System.Drawing.Size(496, 426);
            this.txtIntermediateCode.TabIndex = 4;
            this.txtIntermediateCode.Text = "";
            // 
            // TokenView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 695);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TokenView";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "Token View";
            this.Load += new System.EventHandler(this.TokenView_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RichTextBox txtIntermediateCode;
    }
}