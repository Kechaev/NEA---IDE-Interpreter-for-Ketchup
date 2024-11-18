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
            this.lblKeyword = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtIntermediateCode = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // lblKeyword
            // 
            this.lblKeyword.AutoSize = true;
            this.lblKeyword.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblKeyword.Location = new System.Drawing.Point(12, 444);
            this.lblKeyword.Name = "lblKeyword";
            this.lblKeyword.Size = new System.Drawing.Size(0, 29);
            this.lblKeyword.TabIndex = 1;
            // 
            // txtDescription
            // 
            this.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDescription.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDescription.Location = new System.Drawing.Point(17, 491);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(334, 79);
            this.txtDescription.TabIndex = 2;
            this.txtDescription.Visible = false;
            // 
            // txtIntermediateCode
            // 
            this.txtIntermediateCode.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtIntermediateCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtIntermediateCode.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIntermediateCode.Location = new System.Drawing.Point(15, 15);
            this.txtIntermediateCode.Name = "txtIntermediateCode";
            this.txtIntermediateCode.ReadOnly = true;
            this.txtIntermediateCode.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.txtIntermediateCode.Size = new System.Drawing.Size(331, 426);
            this.txtIntermediateCode.TabIndex = 3;
            this.txtIntermediateCode.Text = "";
            // 
            // IntermediateView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 582);
            this.Controls.Add(this.txtIntermediateCode);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblKeyword);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IntermediateView";
            this.Text = "Intermediate";
            this.Load += new System.EventHandler(this.IntermediateView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblKeyword;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.RichTextBox txtIntermediateCode;
    }
}