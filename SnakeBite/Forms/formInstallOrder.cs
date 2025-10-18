using SnakeBite.ModPages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SnakeBite.Forms
{
    public partial class formInstallOrder : Form
    {
        public static Point formLocation = new Point(0, 0);
        public static Size formSize = new Size(0, 0);

        private readonly List<PreinstallEntry> Mods = new List<PreinstallEntry>();
        private readonly SettingsManager manager = new SettingsManager(GamePaths.SnakeBiteSettings);
        private int selectedIndex;

        private readonly NoAddedPage noModsNotice = new NoAddedPage();
        private readonly ModDescriptionPage modDescription = new ModDescriptionPage();
        private readonly LogPage log = new LogPage();

        public formInstallOrder()
        {
            InitializeComponent();
            panelContent.Controls.Add(noModsNotice);
            panelContent.Controls.Add(modDescription);
            panelContent.Controls.Add(log);
        }

        public void ShowDialog(List<string> Filenames)
        {
            foreach (string file in Filenames)
            {
                PreinstallEntry mod = new PreinstallEntry
                {
                    filename = file
                };
                Mods.Add(mod);
            }
            ShowDialog();
        }

        private void formInstallOrder_Shown(object sender, EventArgs e)
        {
            SetVisiblePage(log);
            CheckAllModConflicts();
            refreshInstallList();
        }

        private void refreshInstallList()
        {
            listInstallOrder.Items.Clear();
            int modCount = Mods.Count;

            if (modCount > 0)
            {
                buttonContinue.Enabled = true;
                buttonRemove.Enabled = true;
                buttonUp.Enabled = true;
                buttonDown.Enabled = true;
                SetVisiblePage(modDescription);

                foreach (PreinstallEntry mod in Mods)
                {
                    listInstallOrder.Items.Add(mod.modInfo.Name);
                }

                selectedIndex = modCount - 1;
                listInstallOrder.Items[selectedIndex].Selected = true;
                updateModDescription();
            }
            else
            {
                buttonContinue.Enabled = false;
                buttonRemove.Enabled = false;
                buttonUp.Enabled = false;
                buttonDown.Enabled = false;
                SetVisiblePage(noModsNotice);

            }
            TallyConflicts();
            labelModCount.Text = "Total Count: " + modCount;
        }

        private void updateModDescription()
        {
            if (selectedIndex >= 0)
            {
                PreinstallEntry selectedMod = Mods[selectedIndex];
                modDescription.ShowModInfo(selectedMod.modInfo);
                showConflictColors();
            }
        }

        private void CheckAllModConflicts()
        {
            ProgressWindow.Show("Checking Preinstall Conflicts", "Processing mod data, please wait...", new Action((MethodInvoker)delegate
            {
                PreinstallManager.RefreshAllXml(Mods);
                PreinstallManager.getAllConflicts(Mods);
            }), log);
        }

        private void TallyConflicts()
        {
            int conflictCounter = 0;
            for (int i = 0; i < Mods.Count; i++)
            {
                if (Mods[i].ModConflicts.Count > 0)
                {
                    conflictCounter++;
                }
            }
            labelConflictCount.Text = string.Format("Conflicts Detected: {0}", conflictCounter);
        }

        private void RemoveConflict(string modName)
        {
            foreach (PreinstallEntry remainingEntry in Mods.FindAll(entry => entry.ModConflicts.Contains(modName)))
            {
                remainingEntry.ModConflicts.Remove(modName);
            }
        }

        private void showConflictColors()
        {
            int lowestIndex = 0;
            for (int i = 0; i < listInstallOrder.Items.Count; i++)
            {
                if (Mods[selectedIndex].ModConflicts.Contains(listInstallOrder.Items[i].Text))
                {
                    if (i < selectedIndex)
                    {
                        listInstallOrder.Items[i].BackColor = Color.IndianRed;
                    }//if the conflicting mod installs before the selected mod, the contents are overwritten (visualized by a red backcolor)
                    else
                    {
                        listInstallOrder.Items[i].BackColor = Color.MediumSeaGreen;
                        lowestIndex = i;//the last index checked will always be lowest on the list.
                    }//if the conflicting mod installs after the selected mod, the selected mod is overwriten (visualized by a green backcolor)
                }
                else
                {
                    listInstallOrder.Items[i].BackColor = Color.Silver;
                }
            }
            listInstallOrder.Items[selectedIndex].BackColor = lowestIndex > selectedIndex ? Color.IndianRed : Color.MediumSeaGreen;
        }

        private void AddNewPaths(string[] modFilePaths)
        {
            foreach (string filePath in modFilePaths)
            {
                bool skip = false;
                foreach (PreinstallEntry mod in Mods)
                {
                    if (filePath == mod.filename)
                    {
                        skip = true; break;
                    }
                }
                if (skip)
                {
                    continue;
                }

                PreinstallEntry newEntry = new PreinstallEntry
                {
                    filename = filePath
                };
                PreinstallManager.AddModsToXml(newEntry);
                PreinstallManager.GetConflicts(newEntry, Mods);
                Mods.Add(newEntry);
            }
        }

        private void listInstallOrder_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listInstallOrder.SelectedItems.Count == 1)
            {
                selectedIndex = listInstallOrder.SelectedIndices[0];
                updateModDescription();
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (selectedIndex > 0)
            {
                PreinstallEntry mod = Mods[selectedIndex];
                listInstallOrder.Items[selectedIndex].Remove(); Mods.RemoveAt(selectedIndex);
                selectedIndex--;
                listInstallOrder.Items.Insert(selectedIndex, mod.modInfo.Name); Mods.Insert(selectedIndex, mod);
                listInstallOrder.Items[selectedIndex].Selected = true;
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (selectedIndex < listInstallOrder.Items.Count - 1)
            {
                PreinstallEntry mod = Mods[selectedIndex];
                listInstallOrder.Items[selectedIndex].Remove(); Mods.RemoveAt(selectedIndex);
                selectedIndex++;
                listInstallOrder.Items.Insert(selectedIndex, mod.modInfo.Name); Mods.Insert(selectedIndex, mod);
                listInstallOrder.Items[selectedIndex].Selected = true;
            }

        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {

            OpenFileDialog openModFile = new OpenFileDialog
            {
                Filter = "MGSV Mod Files|*.mgsv|All Files|*.*",
                Multiselect = true
            };

            DialogResult ofdResult = openModFile.ShowDialog();
            if (ofdResult != DialogResult.OK)
            {
                return;
            }

            log.ClearPage();
            SetVisiblePage(log);
            ProgressWindow.Show("Checking Preinstall Data", "Processing mod data, please wait...", new Action((MethodInvoker)delegate { AddNewPaths(openModFile.FileNames); }), log);
            refreshInstallList();

        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            string modName = Mods[selectedIndex].modInfo.Name;
            if (listInstallOrder.SelectedItems != null)
            {
                PreinstallManager.RemoveFromXml(Mods[selectedIndex]);
                Mods.RemoveAt(selectedIndex);
                RemoveConflict(modName);
                refreshInstallList();
            }
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            List<string> modFiles = new List<string>();
            foreach (PreinstallEntry entry in Mods)
            {
                modFiles.Add(entry.filename);
            }
            log.ClearPage();
            SetVisiblePage(log);
            ProgressWindow.Show("Checking Validity", "Checking mod validity...", new Action((MethodInvoker)delegate { PreinstallManager.FilterModValidity(modFiles); }), log);
            if (modFiles.Count == 0) { refreshInstallList(); return; }//no valid mods. no mods will be installed

            formLocation = Location;
            formSize = Size;
            ProgressWindow.Show("Checking Conflicts", "Checking for conflicts with installed mods...", new Action((MethodInvoker)delegate { PreinstallManager.FilterModConflicts(modFiles); }), log);
            if (modFiles.Count == 0) { refreshInstallList(); return; }

            string modsToInstall = "";
            for (int i = 0; i < modFiles.Count; i++)
            {
                modsToInstall += "\n" + Tools.ReadMetaData(modFiles[i]).Name;
            }
            DialogResult confirmInstall = MessageBox.Show(string.Format("The following mods will be installed:\n" + modsToInstall), "SnakeBite", MessageBoxButtons.OKCancel);
            if (confirmInstall == DialogResult.OK)
            {
                ProgressWindow.Show("Installing Mod(s)", "Installing, please wait...", new Action((MethodInvoker)delegate { InstallManager.InstallMods(modFiles); }), log);
                Close();
            }
            else
            {
                refreshInstallList();
            }
        }

        private void labelExplainConflict_Click(object sender, EventArgs e)
        {
            MessageBox.Show("A 'Mod Conflict' is when two or more mods attempt to modify the same game file. Whichever mod installs last in the Installation Order will overwrite any conflicting files of the mods above it. " +
       "In other words: The lower the mod, the higher the priority.\n\nThe user can adjust the Installation Order by using the arrow buttons. " +
       "Conflicts can also be resolved by removing mods from the list (removed mods will not be installed). \n\n" +
       "Warning: overwriting a mod's data may cause significant problems in-game, which could affect your enjoyment. Install at your own risk.", "What is a Mod Conflict?", MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void formInstallOrder_FormClosed(object sender, FormClosedEventArgs e)
        {
            ModManager.CleanupFolders();
        }

        private void SetVisiblePage(UserControl visiblePage)
        {
            UserControl[] pages = { log, modDescription, noModsNotice };
            foreach (UserControl page in pages)
            {
                page.Visible = page == visiblePage;
            }
        }
    }
}
