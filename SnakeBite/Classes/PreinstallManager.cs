using ICSharpCode.SharpZipLib.Zip;
using SnakeBite.Forms;
using SnakeBite.GzsTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SnakeBite
{
    internal static class PreinstallManager
    {
        public static void RemoveFromXml(PreinstallEntry mod)
        {
            new SettingsManager("_extr\\buildInfo.xml").RemoveMod(mod.modInfo);
        }

        public static void AddModsToXml(params PreinstallEntry[] modsArrary)
        {
            HashingExtended.ReadDictionary();
            foreach (PreinstallEntry mod in modsArrary)
            {
                FastZip unzipper = new FastZip();
                unzipper.ExtractZip(mod.filename, "_extr", "metadata.xml");
                ModEntry metaData = new ModEntry("_extr\\metadata.xml");

                Dictionary<string, string> newNameDictionary = new Dictionary<string, string>();
                int foundUpdate = 0;

                Debug.LogLine(string.Format("[PreinstallCheck] Checking for Qar path updates: {0}", metaData.Name), Debug.LogLevel.Basic);
                foreach (ModQarEntry modQar in metaData.ModQarEntries.Where(entry => !entry.FilePath.StartsWith("/Assets/")))
                {
                    string unhashedName = HashingExtended.UpdateName(modQar.FilePath);
                    if (unhashedName != null)
                    {
                        Debug.LogLine(string.Format("[PreinstallCheck] Update successful: {0} -> {1}", modQar.FilePath, unhashedName), Debug.LogLevel.Basic);
                        newNameDictionary.Add(modQar.FilePath, unhashedName);
                        modQar.FilePath = unhashedName;
                        foundUpdate++;
                    }
                }
                if (foundUpdate > 0)
                {
                    foreach (ModFpkEntry modFpkEntry in metaData.ModFpkEntries)
                    {
                        if (newNameDictionary.TryGetValue(modFpkEntry.FpkFile, out string unHashedName))
                        {
                            modFpkEntry.FpkFile = unHashedName;
                        }
                    }
                }

                new SettingsManager("_extr\\buildInfo.xml").AddMod(metaData);
                mod.modInfo = metaData;
            }

        }

        public static void RefreshAllXml(List<PreinstallEntry> mods)
        {
            new SettingsManager("_extr\\buildInfo.xml").ClearAllMods();
            AddModsToXml(mods.ToArray());
        }

        public static List<ModEntry> getModEntries()
        {
            return new SettingsManager("_extr\\buildInfo.xml").GetInstalledMods();
        }

        public static void getAllConflicts(List<PreinstallEntry> allMods)
        {
            new List<PreinstallEntry>();
            foreach (PreinstallEntry modA in allMods)
            {
                Debug.LogLine(string.Format("[PreinstallCheck] Checking for conflicts: {0}", modA.modInfo.Name), Debug.LogLevel.Basic);
                bool Skip = true;
                foreach (PreinstallEntry modB in allMods)
                {
                    if (modA.Equals(modB)) { Skip = false; continue; }
                    if (Skip) { continue; }

                    if (hasConflict(modA.modInfo, modB.modInfo))
                    {
                        modA.ModConflicts.Add(modB.modInfo.Name);
                        modB.ModConflicts.Add(modA.modInfo.Name);
                    }

                }
            }
        }

        public static void GetConflicts(PreinstallEntry addedMod, List<PreinstallEntry> listedMods)
        {

            Debug.LogLine(string.Format("[PreinstallCheck] Checking for conflicts: {0}", addedMod.modInfo.Name), Debug.LogLevel.Basic);
            foreach (PreinstallEntry listedMod in listedMods)
            {
                if (addedMod.Equals(listedMod) || listedMod.ModConflicts.Contains(addedMod.modInfo.Name))
                {
                    continue;
                }

                if (hasConflict(addedMod.modInfo, listedMod.modInfo))
                {
                    addedMod.ModConflicts.Add(listedMod.modInfo.Name);
                    listedMod.ModConflicts.Add(addedMod.modInfo.Name);
                }

            }
        }

        public static bool hasConflict(ModEntry mod1, ModEntry mod2)
        {
            foreach (ModQarEntry qarEntry in mod1.ModQarEntries)
            {
                if (qarEntry.FilePath.EndsWith(".fpk") || qarEntry.FilePath.EndsWith(".fpkd"))
                {
                    continue;
                }

                ModQarEntry conflicts = mod2.ModQarEntries.FirstOrDefault(entry => Tools.CompareHashes(entry.FilePath, qarEntry.FilePath));
                if (conflicts != null)
                {
                    Debug.LogLine(string.Format("[PreinstallCheck] Conflict found between {0} and {1}: {2}", mod1.Name, mod2.Name, conflicts.FilePath), Debug.LogLevel.Basic);
                    return true;
                }
            }

            foreach (ModFpkEntry fpkEntry in mod1.ModFpkEntries)
            {
                ModFpkEntry conflicts = mod2.ModFpkEntries.FirstOrDefault(entry => Tools.CompareHashes(entry.FpkFile, fpkEntry.FpkFile) &&
                                                                                       Tools.CompareHashes(entry.FilePath, fpkEntry.FilePath));
                if (conflicts != null)
                {
                    return true;
                }
            }

            foreach (ModFileEntry fileEntry in mod1.ModFileEntries)
            {
                ModFileEntry conflicts = mod2.ModFileEntries.FirstOrDefault(entry => Tools.CompareHashes(entry.FilePath, fileEntry.FilePath));
                if (conflicts != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static void FilterModValidity(List<string> ModFiles)// checks if mods are too old for snakebite, or if snakebite is too old for mods, and whether mods were for an older version of the game.
        {

            ModEntry metaData;
            for (int i = ModFiles.Count() - 1; i >= 0; i--)
            {
                metaData = Tools.ReadMetaData(ModFiles[i]);
                if (metaData == null)
                {
                    MessageBox.Show(string.Format("{0} does not contain a metadata.xml and cannot be installed.", ModFiles[i]));
                    ModFiles.RemoveAt(i);
                    continue;
                }
                Version SBVersion = ModManager.GetSBVersion();
                ModManager.GetMGSVersion();
                new Version();
                new Version();
                Version modSBVersion;
                try
                {
                    modSBVersion = metaData.SBVersion.AsVersion();
                    Version modMGSVersion = metaData.MGSVersion.AsVersion();
                }
                catch
                {
                    MessageBox.Show(string.Format("The selected version of {0} was created with an older version of SnakeBite and is no longer compatible, please download the latest version and try again.", metaData.Name), "Mod update required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ModFiles.RemoveAt(i);
                    continue;
                }
                if (modSBVersion > SBVersion)
                {
                    if (DialogResult.Yes != MessageBox.Show($"'{metaData.Name}' was created with SnakeBite {metaData.SBVersion.AsString()} and may not be compatible with {SBVersion}.\n\nContinue?", "Update required", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
                    {
                        ModFiles.RemoveAt(i);
                    }
                    continue;
                }

                if (modSBVersion < new Version(0, 8, 0, 0))
                {
                    MessageBox.Show(string.Format("The selected version of {0} was created with an older version of SnakeBite and is no longer compatible, please download the latest version and try again.", metaData.Name), "Mod update required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ModFiles.RemoveAt(i);
                    continue;
                }
            }
        }

        public static void FilterModConflicts(List<string> ModFiles)//checks if the mods in the list conflict with installed mods or with the game files
        {
            int confCounter;
            int confIndex;
            SettingsManager manager = new SettingsManager(GamePaths.SnakeBiteSettings);
            List<ModEntry> mods = manager.GetInstalledMods();
            List<string> conflictingMods;
            ModEntry metaData;
            formModConflict conflictForm = new formModConflict();
            for (int i = ModFiles.Count() - 1; i >= 0; i--)
            {
                metaData = Tools.ReadMetaData(ModFiles[i]);
                confCounter = 0;
                confIndex = -1;
                conflictingMods = new List<string>();

                Debug.LogLine(string.Format("[PreinstallCheck] Checking conflicts for {0}", metaData.Name), Debug.LogLevel.Basic);
                foreach (ModEntry mod in mods)
                {
                    foreach (ModFileEntry fileEntry in metaData.ModFileEntries)
                    {
                        ModFileEntry conflicts = mod.ModFileEntries.FirstOrDefault(entry => Tools.CompareHashes(entry.FilePath, fileEntry.FilePath));
                        if (conflicts != null)
                        {
                            if (confIndex == -1)
                            {
                                confIndex = mods.IndexOf(mod);
                            }

                            if (!conflictingMods.Contains(mod.Name))
                            {
                                conflictingMods.Add(mod.Name);
                            }

                            Debug.LogLine(string.Format("[{0}] Conflict in 00.dat: {1}", mod.Name, conflicts.FilePath), Debug.LogLevel.Basic);
                            confCounter++;
                        }
                    }

                    foreach (ModQarEntry qarEntry in metaData.ModQarEntries)
                    {
                        if (qarEntry.FilePath.EndsWith(".fpk") || qarEntry.FilePath.EndsWith(".fpkd"))
                        {
                            continue;
                        }

                        ModQarEntry conflicts = mod.ModQarEntries.FirstOrDefault(entry => Tools.CompareHashes(entry.FilePath, qarEntry.FilePath));
                        if (conflicts != null)
                        {
                            if (confIndex == -1)
                            {
                                confIndex = mods.IndexOf(mod);
                            }

                            if (!conflictingMods.Contains(mod.Name))
                            {
                                conflictingMods.Add(mod.Name);
                            }

                            Debug.LogLine(string.Format("[{0}] Conflict in 00.dat: {1}", mod.Name, conflicts.FilePath), Debug.LogLevel.Basic);
                            confCounter++;
                        }
                    }

                    foreach (ModFpkEntry fpkEntry in metaData.ModFpkEntries)
                    {
                        ModFpkEntry conflicts = mod.ModFpkEntries.FirstOrDefault(entry => Tools.CompareHashes(entry.FpkFile, fpkEntry.FpkFile) &&
                                                                                               Tools.CompareHashes(entry.FilePath, fpkEntry.FilePath));
                        if (conflicts != null)
                        {
                            if (confIndex == -1)
                            {
                                confIndex = mods.IndexOf(mod);
                            }

                            if (!conflictingMods.Contains(mod.Name))
                            {
                                conflictingMods.Add(mod.Name);
                            }

                            Debug.LogLine(string.Format("[{0}] Conflict in {2}: {1}", mod.Name, conflicts.FilePath, Path.GetFileName(conflicts.FpkFile)), Debug.LogLevel.Basic);
                            confCounter++;
                        }
                    }
                }

                if (conflictingMods.Count > 0)
                {
                    Debug.LogLine(string.Format("[Mod] Found {0} conflicts", confCounter), Debug.LogLevel.Basic);
                    string msgboxtext = string.Format("\"{0}\" conflicts with mods that are already installed:\n", Tools.ReadMetaData(ModFiles[i]).Name);
                    foreach (string Conflict in conflictingMods)
                    {
                        msgboxtext += string.Format("\n\"{0}\"", Conflict);
                    }
                    DialogResult userInput = conflictForm.ShowDialog(msgboxtext);
                    if (userInput == DialogResult.Cancel)
                    {
                        ModFiles.RemoveAt(i);
                        continue;
                    }
                }

                Debug.LogLine("[Mod] No conflicts found", Debug.LogLevel.Basic);

                bool sysConflict = false;
                GameData gameData = manager.GetGameData();
                foreach (ModQarEntry gameQarFile in gameData.GameQarEntries.FindAll(entry => entry.SourceType == FileSource.System))
                {
                    if (metaData.ModQarEntries.Count(entry => Tools.ToQarPath(entry.FilePath) == Tools.ToQarPath(gameQarFile.FilePath)) > 0)
                    {
                        sysConflict = true;
                    }
                }

                foreach (ModFpkEntry gameFpkFile in gameData.GameFpkEntries.FindAll(entry => entry.SourceType == FileSource.System))
                {
                    if (metaData.ModFpkEntries.Count(entry => entry.FilePath == gameFpkFile.FilePath && entry.FpkFile == gameFpkFile.FpkFile) > 0)
                    {
                        sysConflict = true;
                    }
                }
                if (sysConflict)
                {
                    string msgboxtext = string.Format("\"{0}\" conflicts with existing MGSV system files,\n", Tools.ReadMetaData(ModFiles[i]).Name);
                    msgboxtext += "or the snakebite.xml base entries has become corrupt.\n";
                    msgboxtext += "Please use the 'Restore Backup Game Files' option in Snakebite settings and re-run snakebite\n";
                    DialogResult userInput = conflictForm.ShowDialog(msgboxtext);
                    if (userInput == DialogResult.Cancel)
                    {
                        ModFiles.RemoveAt(i);
                        continue;
                    }
                }
            }
        }

        public static bool CheckConflicts(string ModFile)
        {
            ModEntry metaData = Tools.ReadMetaData(ModFile);
            if (metaData == null)
            {
                return false;
            }
            Version SBVersion = ModManager.GetSBVersion();
            Version MGSVersion = ModManager.GetMGSVersion();

            SettingsManager manager = new SettingsManager(GamePaths.SnakeBiteSettings);
            Version modSBVersion = new Version();
            Version modMGSVersion = new Version();
            try
            {
                modSBVersion = metaData.SBVersion.AsVersion();
                modMGSVersion = metaData.MGSVersion.AsVersion();
            }
            catch
            {
                MessageBox.Show(string.Format("The selected version of {0} was created with an older version of SnakeBite and is no longer compatible, please download the latest version and try again.", metaData.Name), "Mod update required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (modSBVersion > SBVersion)
            {
                MessageBox.Show(string.Format("{0} requires SnakeBite version {1} or newer. Please follow the link on the Settings page to get the latest version.", metaData.Name, metaData.SBVersion), "Update required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (modSBVersion < new Version(0, 8, 0, 0))
            {
                MessageBox.Show(string.Format("The selected version of {0} was created with an older version of SnakeBite and is no longer compatible, please download the latest version and try again.", metaData.Name), "Mod update required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!manager.IsUpToDate(modMGSVersion))
            {
                if (MGSVersion > modMGSVersion)
                {
                    DialogResult contInstall = MessageBox.Show(string.Format("{0} appears to be for an older version of MGSV. It is recommended that you check for an updated version before installing.\n\nContinue installation?", metaData.Name), "Game version mismatch", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (contInstall == DialogResult.No)
                    {
                        return false;
                    }
                }
                if (MGSVersion < modMGSVersion)
                {
                    MessageBox.Show(string.Format("{0} requires MGSV version {1}, but your installation is version {2}. Please update MGSV and try again.", metaData.Name, modMGSVersion, MGSVersion), "Update required", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }


            Debug.LogLine(string.Format("[Mod] Checking conflicts for {0}", metaData.Name), Debug.LogLevel.Basic);
            int confCounter = 0;
            List<ModEntry> mods = manager.GetInstalledMods();
            List<string> conflictingMods = new List<string>();
            int confIndex = -1;
            foreach (ModEntry mod in mods)
            {
                foreach (ModFileEntry fileEntry in metaData.ModFileEntries)
                {
                    ModFileEntry conflicts = mod.ModFileEntries.FirstOrDefault(entry => Tools.CompareHashes(entry.FilePath, fileEntry.FilePath));
                    if (conflicts != null)
                    {
                        if (confIndex == -1)
                        {
                            confIndex = mods.IndexOf(mod);
                        }

                        if (!conflictingMods.Contains(mod.Name))
                        {
                            conflictingMods.Add(mod.Name);
                        }

                        Debug.LogLine(string.Format("[{0}] Conflict in 00.dat: {1}", mod.Name, conflicts.FilePath), Debug.LogLevel.Basic);
                        confCounter++;
                    }
                }

                foreach (ModQarEntry qarEntry in metaData.ModQarEntries)
                {
                    if (qarEntry.FilePath.EndsWith(".fpk") || qarEntry.FilePath.EndsWith(".fpkd"))
                    {
                        continue;
                    }

                    ModQarEntry conflicts = mod.ModQarEntries.FirstOrDefault(entry => Tools.CompareHashes(entry.FilePath, qarEntry.FilePath));
                    if (conflicts != null)
                    {
                        if (confIndex == -1)
                        {
                            confIndex = mods.IndexOf(mod);
                        }

                        if (!conflictingMods.Contains(mod.Name))
                        {
                            conflictingMods.Add(mod.Name);
                        }

                        Debug.LogLine(string.Format("[{0}] Conflict in 00.dat: {1}", mod.Name, conflicts.FilePath), Debug.LogLevel.Basic);
                        confCounter++;
                    }
                }

                foreach (ModFpkEntry fpkEntry in metaData.ModFpkEntries)
                {
                    ModFpkEntry conflicts = mod.ModFpkEntries.FirstOrDefault(entry => Tools.CompareHashes(entry.FpkFile, fpkEntry.FpkFile) &&
                                                                                           Tools.CompareHashes(entry.FilePath, fpkEntry.FilePath));
                    if (conflicts != null)
                    {
                        if (confIndex == -1)
                        {
                            confIndex = mods.IndexOf(mod);
                        }

                        if (!conflictingMods.Contains(mod.Name))
                        {
                            conflictingMods.Add(mod.Name);
                        }

                        Debug.LogLine(string.Format("[{0}] Conflict in {2}: {1}", mod.Name, conflicts.FilePath, Path.GetFileName(conflicts.FpkFile)), Debug.LogLevel.Basic);
                        confCounter++;
                    }
                }
            }

            if (conflictingMods.Count > 0)
            {
                Debug.LogLine(string.Format("[Mod] Found {0} conflicts", confCounter), Debug.LogLevel.Basic);
                string msgboxtext = "The selected mod conflicts with these mods:\n";
                foreach (string Conflict in conflictingMods)
                {
                    msgboxtext += Conflict + "\n";
                }
                msgboxtext += "\nMore information regarding the conflicts has been output to the logfile.";
                MessageBox.Show(msgboxtext, "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            Debug.LogLine("[Mod] No conflicts found", Debug.LogLevel.Basic);

            bool sysConflict = false;
            GameData gameData = manager.GetGameData();
            foreach (ModQarEntry gameQarFile in gameData.GameQarEntries.FindAll(entry => entry.SourceType == FileSource.System))
            {
                if (metaData.ModQarEntries.Count(entry => Tools.ToQarPath(entry.FilePath) == Tools.ToQarPath(gameQarFile.FilePath)) > 0)
                {
                    sysConflict = true;
                }
            }

            foreach (ModFpkEntry gameFpkFile in gameData.GameFpkEntries.FindAll(entry => entry.SourceType == FileSource.System))
            {
                if (metaData.ModFpkEntries.Count(entry => entry.FilePath == gameFpkFile.FilePath && entry.FpkFile == gameFpkFile.FpkFile) > 0)
                {
                    sysConflict = true;
                }
            }
            if (sysConflict)
            {
                string msgboxtext = "The selected mod conflicts with existing MGSV system files,\n";
                msgboxtext += "or the snakebite.xml base entries has become corrupt.\n";
                msgboxtext += "Please use the 'Restore Backup Game Files' option in Snakebite settings and re-run snakebite\n";
                MessageBox.Show(msgboxtext, "SnakeBite", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
    }
    public class PreinstallEntry
    {
        public string filename { get; set; }

        public ModEntry modInfo { get; set; }

        public List<string> ModConflicts = new List<string>();

    }
}



