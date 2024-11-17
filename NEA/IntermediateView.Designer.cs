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
            this.txtIntermediateCode = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtIntermediateCode
            // 
            this.txtIntermediateCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtIntermediateCode.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtIntermediateCode.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtIntermediateCode.Location = new System.Drawing.Point(15, 15);
            this.txtIntermediateCode.Multiline = true;
            this.txtIntermediateCode.Name = "txtIntermediateCode";
            this.txtIntermediateCode.ReadOnly = true;
            this.txtIntermediateCode.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtIntermediateCode.Size = new System.Drawing.Size(336, 426);
            this.txtIntermediateCode.TabIndex = 0;
            // 
            // IntermediateView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 578);
            this.Controls.Add(this.txtIntermediateCode);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "IntermediateView";
            this.Text = "IntermediateView";
            this.Load += new System.EventHandler(this.IntermediateView_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtIntermediateCode;
    }
}