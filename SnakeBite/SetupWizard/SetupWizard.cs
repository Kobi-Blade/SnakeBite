using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using static SnakeBite.GamePaths;

namespace SnakeBite.SetupWizard
{
    public partial class SetupWizard : Form
    {
        private readonly IntroPage introPage = new IntroPage();
        private readonly FindInstallPage findInstallPage = new FindInstallPage();
        private readonly CreateBackupPage createBackupPage = new CreateBackupPage();
        private readonly MergeDatPage mergeDatPage = new MergeDatPage();
        private int displayPage = 0;
        private bool setupComplete = false;
        private SettingsManager manager = new SettingsManager(SnakeBiteSettings);

        public SetupWizard()
        {
            InitializeComponent();
            FormClosing += formSetupWizard_Closing;
        }

        private void formSetupWizard_Load(object sender, EventArgs e)
        {
            buttonSkip.Visible = false;
            contentPanel.Controls.Add(introPage);
        }

        private void formSetupWizard_Closing(object sender, FormClosingEventArgs e)
        {
            if ((string)Tag == "noclose" && !(displayPage == 5))
            {
                e.Cancel = true;
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            switch (displayPage)
            {
                case -1:
                    buttonBack.Visible = false;
                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(introPage);
                    displayPage = 0;
                    break;

                case 0:
                    buttonBack.Visible = true;
                    buttonSkip.Visible = false;
                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(findInstallPage);
                    displayPage = 1;
                    break;

                case 1:
                    manager = new SettingsManager(SnakeBiteSettings);
                    if (!manager.ValidInstallPath)
                    {
                        MessageBox.Show("Please select a valid installation directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (!BackupManager.GameFilesExist())
                    {
                        MessageBox.Show("Some game data appears to be missing. If you have just revalidated the game data, please wait for Steam to finish downloading the new files before continuing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    createBackupPage.panelProcessing.Visible = false;
                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(createBackupPage);
                    buttonSkip.Visible = true;

                    displayPage = 2;
                    break;

                case 2:
                    manager = new SettingsManager(SnakeBiteSettings);
                    if (!(manager.IsVanilla0001Size() || manager.IsVanilla0001DatHash()) && (SettingsManager.IntendedGameVersion >= ModManager.GetMGSVersion()))
                    {
                        DialogResult overWrite = MessageBox.Show(string.Format("Your existing game data contains unexpected filesizes, and is likely already modified or predates Game Version {0}." +
                            "\n\nIt is recommended that you do NOT store these files as backups, unless you are absolutely certain that they can reliably restore your game to a safe state!" +
                            "\n\nAre you sure you want to save these as backup data?", SettingsManager.IntendedGameVersion), "Unexpected 00.dat / 01.dat Filesizes", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (overWrite != DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    string overWriteMessage;

                    if (BackupManager.BackupExists())
                    {
                        overWriteMessage = SettingsManager.IntendedGameVersion < ModManager.GetMGSVersion()
                            ? string.Format("Some backup data already exists. Since this version of SnakeBite is intended for MGSV Version {0} and is now MGSV Version {1}, it is recommended that you overwrite your old backup files with new data.", SettingsManager.IntendedGameVersion, ModManager.GetMGSVersion()) +
                                "\n\nContinue?"
                            : "Some backup data already exists. Continuing will overwrite your existing backups." +
                            "\n\nAre you sure you want to continue?";

                        DialogResult overWrite = MessageBox.Show(overWriteMessage, "Overwrite Existing Files?", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (overWrite != DialogResult.Yes)
                        {
                            return;
                        }
                    }
                    buttonSkip.Visible = false;
                    buttonBack.Visible = false;
                    buttonNext.Enabled = false;
                    createBackupPage.panelProcessing.Visible = true;
                    Application.UseWaitCursor = true;
                    BackgroundWorker backupProcessor = new BackgroundWorker();
                    backupProcessor.DoWork += new DoWorkEventHandler(BackupManager.backgroundWorker_CopyBackupFiles);
                    backupProcessor.WorkerReportsProgress = true;
                    backupProcessor.ProgressChanged += new ProgressChangedEventHandler(backupProcessor_ProgressChanged);
                    backupProcessor.RunWorkerAsync();

                    while (backupProcessor.IsBusy)
                    {
                        Application.DoEvents();
                        Thread.Sleep(10);
                    }
                    mergeDatPage.panelProcessing.Visible = false;
                    Application.UseWaitCursor = false;

                    contentPanel.Controls.Clear();
                    contentPanel.Controls.Add(mergeDatPage);
                    buttonNext.Enabled = true;

                    displayPage = 3;
                    break;

                case 3:
                    buttonNext.Enabled = false;
                    buttonBack.Visible = false;
                    Tag = "noclose";
                    mergeDatPage.panelProcessing.Visible = true;
                    Application.UseWaitCursor = true;

                    BackgroundWorker mergeProcessor = new BackgroundWorker
                    {
                        WorkerSupportsCancellation = true
                    };
                    mergeProcessor.DoWork += new DoWorkEventHandler(ModManager.backgroundWorker_MergeAndCleanup);
                    mergeProcessor.WorkerReportsProgress = true;
                    mergeProcessor.ProgressChanged += new ProgressChangedEventHandler(mergeProcessor_ProgressChanged);
                    mergeProcessor.RunWorkerCompleted += new RunWorkerCompletedEventHandler(mergeProcessor_Completed);
                    mergeProcessor.RunWorkerAsync();

                    while (mergeProcessor.IsBusy)
                    {
                        Application.DoEvents();
                        Thread.Sleep(40);
                    }

                    if (setupComplete)
                    {
                        Debug.LogLine("[Setup Wizard] Setup Complete. Snakebite is configured and ready to use.");
                        mergeDatPage.panelProcessing.Visible = false;
                        Application.UseWaitCursor = false;

                        mergeDatPage.labelWelcome.Text = "Setup complete";
                        mergeDatPage.labelWelcomeText.Text = "SnakeBite is configured and ready to use.";

                        buttonNext.Text = "Do&ne";
                        buttonNext.Enabled = true;

                        displayPage = 4;
                    }
                    else
                    {
                        Debug.LogLine("[Setup Wizard] Setup Incomplete.");
                        Tag = null;
                        GoToMergeDatPage();

                        buttonNext.Text = "Retry";
                    }
                    break;

                case 4:
                    displayPage = 5;
                    DialogResult = DialogResult.OK;
                    Close();
                    break;
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            displayPage -= 2;
            buttonNext_Click(null, null);
        }

        private void buttonSkip_Click(object sender, EventArgs e)
        {
            GoToMergeDatPage();
        }

        private void GoToMergeDatPage()
        {
            buttonSkip.Visible = false;
            mergeDatPage.panelProcessing.Visible = false;
            Application.UseWaitCursor = false;

            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(mergeDatPage);
            buttonNext.Enabled = true;

            displayPage = 3;
        }

        private void backupProcessor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            createBackupPage.labelWorking.Text = "Backing up " + (string)e.UserState + ". Please Wait...";
        }

        private void mergeProcessor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mergeDatPage.labelWorking.Text = (string)e.UserState + ". Please Wait...";
        }

        private void mergeProcessor_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            setupComplete = !e.Cancelled;

        }
    }
}