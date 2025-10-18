namespace SnakeBite.SetupWizard
{
    partial class FindInstallPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindInstallPage));
            this.panelContent = new System.Windows.Forms.Panel();
            this.buttonRevalidate = new System.Windows.Forms.Button();
            this.labelWarning = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.textInstallPath = new System.Windows.Forms.TextBox();
            this.labelSelectDir = new System.Windows.Forms.Label();
            this.labelHeader = new System.Windows.Forms.Label();
            this.panelContent.SuspendLayout();
            this.SuspendLayout();
            this.panelContent.Controls.Add(this.buttonRevalidate);
            this.panelContent.Controls.Add(this.labelWarning);
            this.panelContent.Controls.Add(this.buttonBrowse);
            this.panelContent.Controls.Add(this.textInstallPath);
            this.panelContent.Controls.Add(this.labelSelectDir);
            this.panelContent.Controls.Add(this.labelHeader);
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(440, 340);
            this.panelContent.TabIndex = 3;
            this.buttonRevalidate.Location = new System.Drawing.Point(140, 268);
            this.buttonRevalidate.Name = "buttonRevalidate";
            this.buttonRevalidate.Size = new System.Drawing.Size(164, 23);
            this.buttonRevalidate.TabIndex = 9;
            this.buttonRevalidate.Text = "Revalidate Game Files";
            this.buttonRevalidate.UseVisualStyleBackColor = true;
            this.buttonRevalidate.Click += new System.EventHandler(this.buttonValidate_Click);
            this.labelWarning.Location = new System.Drawing.Point(8, 95);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new System.Drawing.Size(429, 170);
            this.labelWarning.TabIndex = 8;
            this.labelWarning.Text = resources.GetString("labelWarning.Text");
            this.labelWarning.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.buttonBrowse.Location = new System.Drawing.Point(400, 68);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(32, 25);
            this.buttonBrowse.TabIndex = 7;
            this.buttonBrowse.Tag = "";
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            this.textInstallPath.Location = new System.Drawing.Point(8, 69);
            this.textInstallPath.Name = "textInstallPath";
            this.textInstallPath.ReadOnly = true;
            this.textInstallPath.Size = new System.Drawing.Size(389, 23);
            this.textInstallPath.TabIndex = 6;
            this.labelSelectDir.AutoSize = true;
            this.labelSelectDir.Location = new System.Drawing.Point(5, 51);
            this.labelSelectDir.Name = "labelSelectDir";
            this.labelSelectDir.Size = new System.Drawing.Size(313, 15);
            this.labelSelectDir.TabIndex = 5;
            this.labelSelectDir.Text = "Please select your Metal Gear Solid V installation directory:";
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.Location = new System.Drawing.Point(3, 0);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(245, 30);
            this.labelHeader.TabIndex = 4;
            this.labelHeader.Text = "Find Metal Gear Solid V";
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelContent);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "FindInstallPage";
            this.Size = new System.Drawing.Size(440, 340);
            this.panelContent.ResumeLayout(false);
            this.panelContent.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox textInstallPath;
        private System.Windows.Forms.Label labelSelectDir;
        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.Button buttonRevalidate;
        private System.Windows.Forms.Label labelWarning;
    }
}
