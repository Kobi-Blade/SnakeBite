namespace SnakeBite
{
    partial class formProgress
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
            this.StatusText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.StatusText.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StatusText.Location = new System.Drawing.Point(12, 9);
            this.StatusText.Name = "StatusText";
            this.StatusText.Size = new System.Drawing.Size(335, 38);
            this.StatusText.TabIndex = 0;
            this.StatusText.Text = "SnakeBite is working, please wait...";
            this.StatusText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.StatusText.UseWaitCursor = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(359, 56);
            this.ControlBox = false;
            this.Controls.Add(this.StatusText);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formProgress";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Processing...";
            this.UseWaitCursor = true;
            this.VisibleChanged += new System.EventHandler(this.formProgress_VisibleChanged);
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.Label StatusText;
    }
}