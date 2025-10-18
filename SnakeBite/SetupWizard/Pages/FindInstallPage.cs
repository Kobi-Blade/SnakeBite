using System;
using System.IO;
using System.Windows.Forms;

namespace SnakeBite.SetupWizard
{
    public partial class FindInstallPage : UserControl
    {
        public FindInstallPage()
        {
            InitializeComponent();
            if (Directory.Exists(Properties.Settings.Default.InstallPath))
            {
                textInstallPath.Text = Properties.Settings.Default.InstallPath;
            }
        }

        private void buttonValidate_Click(object sender, EventArgs e)
        {
            DialogResult doValidate = MessageBox.Show("Please wait until the Steam validation window says it's complete.", "SnakeBite", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (doValidate == DialogResult.Cancel)
            {
                return;
            }

            System.Diagnostics.Process.Start("steam://validate/287700/");
            BackupManager.DeleteOriginals();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog findMGSV = new OpenFileDialog
            {
                Filter = "Metal Gear Solid V|MGSVTPP.exe"
            };
            DialogResult findResult = findMGSV.ShowDialog();
            if (findResult != DialogResult.OK)
            {
                return;
            }

            string fileDir = Path.GetDirectoryName(findMGSV.FileName);
            textInstallPath.Text = fileDir;
            Properties.Settings.Default.InstallPath = fileDir;
            Properties.Settings.Default.Save();
        }
    }
}