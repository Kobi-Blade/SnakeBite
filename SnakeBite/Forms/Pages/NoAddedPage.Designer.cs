namespace SnakeBite.ModPages
{
    partial class NoAddedPage
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

        #region Component Designer generated code
        private void InitializeComponent()
        {
            this.labelNoMod = new System.Windows.Forms.Label();
            this.panelNoMods = new System.Windows.Forms.Panel();
            this.panelNoMods.SuspendLayout();
            this.SuspendLayout();
            this.labelNoMod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelNoMod.BackColor = System.Drawing.Color.Gray;
            this.labelNoMod.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold);
            this.labelNoMod.Location = new System.Drawing.Point(3, 17);
            this.labelNoMod.Name = "labelNoMod";
            this.labelNoMod.Size = new System.Drawing.Size(394, 55);
            this.labelNoMod.TabIndex = 1;
            this.labelNoMod.Text = "No Mods Added";
            this.labelNoMod.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.panelNoMods.BackColor = System.Drawing.Color.Gray;
            this.panelNoMods.Controls.Add(this.labelNoMod);
            this.panelNoMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelNoMods.Location = new System.Drawing.Point(0, 0);
            this.panelNoMods.Name = "panelNoMods";
            this.panelNoMods.Size = new System.Drawing.Size(400, 424);
            this.panelNoMods.TabIndex = 12;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelNoMods);
            this.Name = "NoAddedPage";
            this.Size = new System.Drawing.Size(400, 424);
            this.panelNoMods.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label labelNoMod;
        private System.Windows.Forms.Panel panelNoMods;
    }
}
