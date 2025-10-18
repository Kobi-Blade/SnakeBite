using SnakeBite.ModPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace SnakeBite
{
    public partial class formSettings : Form
    {
        private readonly List<string> themeFiles = new List<string>() { "" };
        private readonly SettingsManager manager = new SettingsManager(GamePaths.SnakeBiteSettings);
        private readonly LogPage log = new LogPage();

        public formSettings()
        {
            InitializeComponent();
        }

        private void CheckBackupState()
        {
            if (BackupManager.OriginalsExist())
            {
                labelNoBackups.Text = "";
                buttonRestoreOriginals.Enabled = true;
            }
            else
            {
                if (BackupManager.OriginalZeroOneExist())
                {
                    labelNoBackups.Text = "chunk0 backup not detected.\nCannot restore backup game files.";
                    buttonRestoreOriginals.Enabled = false;
                    picModToggle.Enabled = true;
                }
                else
                {
                    labelNoBackups.Text = "No backups detected.\nCertain features are unavailable.";
                    buttonRestoreOriginals.Enabled = false;
                    picModToggle.Enabled = false;
                    picModToggle.Image = Properties.Resources.toggledisabled;
                }
            }
        }

        private void buttonRestoreOriginals_Click(object sender, EventArgs e)
        {
            DialogResult restoreData = MessageBox.Show("Your saved backup files will be restored, and any SnakeBite settings and mods will be completely removed.\n\nAre you sure you want to continue?", "SnakeBite", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (restoreData != DialogResult.Yes)
            {
                return;
            }

            BackupManager.RestoreOriginals();
            try
            {
                manager.DeleteSettings();
                MessageBox.Show("Backups restored. SnakeBite will now close.", "SnakeBite", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch { }
            Application.Exit();
        }

        private void UpdateModToggle()
        {

            if (BackupManager.ModsDisabled())
            {
                picModToggle.Image = Properties.Resources.toggleoff;
                picModToggle.Enabled = true;
            }
            else
            {
                picModToggle.Image = Properties.Resources.toggleon;
                picModToggle.Enabled = true;
            }
        }

        private void buttonSetup(object sender, EventArgs e)
        {
            SetupWizard.SetupWizard setupWizard = new SetupWizard.SetupWizard
            {
                Tag = "closable"
            };
            setupWizard.ShowDialog(Application.OpenForms[0]);
            UpdateModToggle();
            CheckBackupState();
        }

        private void linkNexusLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(labelNexusLink.Text);
        }

        private void buttonFindMGSV_Click(object sender, EventArgs e)
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

            string filePath = findMGSV.FileName.Substring(0, findMGSV.FileName.LastIndexOf("\\"));
            if (filePath != textInstallPath.Text)
            {
                textInstallPath.Text = filePath;
                Properties.Settings.Default.InstallPath = filePath;
                Properties.Settings.Default.Save();
                MessageBox.Show("SnakeBite will now restart.", "SnakeBite", MessageBoxButtons.OK, MessageBoxIcon.Information);
                System.Diagnostics.Process.Start("SnakeBite.exe");
                Application.Exit();
            }
        }

        private void formSettings_Load(object sender, EventArgs e)
        {
            textInstallPath.Text = Properties.Settings.Default.InstallPath;
            checkEnableSound.Checked = Properties.Settings.Default.EnableSound;
            checkBoxSaveRevertPreset.Checked = Properties.Settings.Default.AutosaveRevertPreset;
            checkBoxCloseOnStart.Checked = Properties.Settings.Default.CloseSnakeBiteOnLaunch;
            UpdateModToggle();
            CheckBackupState();
        }

        private void checkEnableSound_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableSound = checkEnableSound.Checked;
            Properties.Settings.Default.Save();
        }

        private void checkBoxSaveRevertPreset_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutosaveRevertPreset = checkBoxSaveRevertPreset.Checked;
            Properties.Settings.Default.Save();
        }

        private void buttonOpenLogDir_Click(object sender, EventArgs e)
        {
            Debug.OpenLogDirectory();
        }

        private void picModToggle_Click(object sender, EventArgs e)
        {
            if (BackupManager.ModsDisabled())
            {
                ProgressWindow.Show("Working", "Enabling mods, please wait...", new Action(BackupManager.SwitchToMods), log);
            }
            else
            {
                ProgressWindow.Show("Working", "Disabling mods, please wait...", new Action(BackupManager.SwitchToOriginal), log);
            }
            UpdateModToggle();
        }

        private void checkBoxCloseOnStart_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CloseSnakeBiteOnLaunch = checkBoxCloseOnStart.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
