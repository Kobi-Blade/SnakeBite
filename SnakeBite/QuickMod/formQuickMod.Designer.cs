namespace SnakeBite.QuickMod
{
    partial class formQuickMod
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.panelContent = new System.Windows.Forms.Panel();
            this.labelHeader = new System.Windows.Forms.Label();
            this.buttonNext = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.panelContent.Location = new System.Drawing.Point(17, 47);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(360, 180);
            this.panelContent.TabIndex = 0;
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.Location = new System.Drawing.Point(12, 9);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(83, 30);
            this.labelHeader.TabIndex = 1;
            this.labelHeader.Text = "Header";
            this.buttonNext.Location = new System.Drawing.Point(302, 233);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(75, 23);
            this.buttonNext.TabIndex = 2;
            this.buttonNext.Text = "Next";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(396, 266);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.labelHeader);
            this.Controls.Add(this.panelContent);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formQuickMod";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Zip Installer";
            this.Load += new System.EventHandler(this.formQuickMod_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.Button buttonNext;
    }
}