using GzsTool.Core.Fpk;
using GzsTool.Core.Qar;
using SnakeBite.GzsTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static SnakeBite.GamePaths;

namespace SnakeBite
{
    internal class UninstallManager
    {
        private static readonly SettingsManager SBBuildManager = new SettingsManager(SnakeBiteSettings + build_ext);

        public static bool UninstallMods(CheckedListBox.CheckedIndexCollection modIndices, bool skipCleanup = false)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Debug.LogLine("[Uninstall] Start", Debug.LogLevel.Basic);
            ModManager.ClearBuildFiles(ZeroPath, OnePath, SnakeBiteSettings, SavePresetPath);
            ModManager.ClearSBGameDir();
            ModManager.CleanupFolders();
            if (Properties.Settings.Default.AutosaveRevertPreset == true)
            {
                PresetManager.SavePreset(SavePresetPath + build_ext);
            }
            else
            {
                Debug.LogLine("[Uninstall] Skipping RevertChanges.MGSVPreset Save", Debug.LogLevel.Basic);
            }

            GzsLib.LoadDictionaries();
            File.Copy(SnakeBiteSettings, SnakeBiteSettings + build_ext, true);
            List<ModEntry> mods = SBBuildManager.GetInstalledMods();

            List<ModEntry> selectedMods = new List<ModEntry>();
            foreach (int index in modIndices)
            {
                ModEntry mod = mods[index];
                selectedMods.Add(mod);
            }

            List<string> zeroFiles = new List<string>();
            bool hasQarZero = ModManager.hasQarZeroFiles(selectedMods);
            if (hasQarZero)
            {
                zeroFiles = GzsLib.ExtractArchive<QarFile>(ZeroPath, "_working0");
                zeroFiles.RemoveAll(file => file.EndsWith("_unknown"));

            }

            List<string> oneFiles = null;
            bool hasFtexs = ModManager.foundLooseFtexs(selectedMods);
            if (hasFtexs)
            {
                oneFiles = GzsLib.ExtractArchive<QarFile>(OnePath, "_working1");
                oneFiles.RemoveAll(file => file.EndsWith("_unknown"));
            }
            GameData gameData = SBBuildManager.GetGameData();
            ModManager.ValidateGameData(ref gameData, ref zeroFiles);

            Debug.LogLine("[Uninstall] Building gameFiles lists", Debug.LogLevel.Basic);
            List<Dictionary<ulong, GameFile>> baseGameFiles = GzsLib.ReadBaseData();
            try
            {
                ModManager.PrepGameDirFiles();
                UninstallMods(selectedMods, ref zeroFiles, ref oneFiles);

                if (hasQarZero)
                {
                    zeroFiles.Sort();
                    GzsLib.WriteQarArchive(ZeroPath + build_ext, "_working0", zeroFiles, GzsLib.zeroFlags);
                }

                if (hasFtexs)
                {
                    oneFiles.Sort();
                    GzsLib.WriteQarArchive(OnePath + build_ext, "_working1", oneFiles, GzsLib.oneFlags);
                }
                ModManager.PromoteGameDirFiles();
                ModManager.PromoteBuildFiles(ZeroPath, OnePath, SnakeBiteSettings, SavePresetPath);

                if (!skipCleanup)
                {
                    ModManager.CleanupFolders();
                    ModManager.ClearSBGameDir();
                }

                Debug.LogLine("[Uninstall] Uninstall complete", Debug.LogLevel.Basic);
                stopwatch.Stop();
                Debug.LogLine($"[Uninstall] Uninstall took {stopwatch.ElapsedMilliseconds} ms", Debug.LogLevel.Basic);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogLine("[Uninstall] Exception: " + e, Debug.LogLevel.Basic);
                stopwatch.Stop();
                Debug.LogLine($"[Uninstall] Uninstall failed at {stopwatch.ElapsedMilliseconds} ms", Debug.LogLevel.Basic);
                MessageBox.Show("An error has occurred during the uninstallation process and SnakeBite could not uninstall the selected mod(s).\nException: " + e);
                ModManager.ClearBuildFiles(ZeroPath, OnePath, SnakeBiteSettings, SavePresetPath);
                ModManager.CleanupFolders();

                bool restoreRetry = false;
                do
                {
                    try
                    {
                        ModManager.RestoreBackupGameDir(SBBuildManager);
                    }
                    catch (Exception f)
                    {
                        Debug.LogLine("[Uninstall] Exception: " + f, Debug.LogLevel.Basic);
                        restoreRetry = DialogResult.Retry == MessageBox.Show("SnakeBite could not restore Game Directory mod files due to the following exception: {f} \nWould you like to retry?", "Exception Occurred", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    }
                } while (restoreRetry);

                ModManager.ClearSBGameDir();
                return false;
            }
        }//UninstallMod batch

        private static void UninstallMods(List<ModEntry> uninstallMods, ref List<string> zeroFiles, ref List<string> oneFilesList)
        {
            Debug.LogLine(string.Format("[Uninstall] Bulk uninstall started"), Debug.LogLevel.Basic);

            Debug.LogLine("[Uninstall] Building fpk removal lists", Debug.LogLevel.Basic);
            GetFpkRemovalLists(uninstallMods, out List<string> fullRemoveQarPaths, out List<ModQarEntry> partialEditQarEntries, out List<ModFpkEntry> partialRemoveFpkEntries);

            Debug.LogLine("[Uninstall] Unmerging any fpk entries", Debug.LogLevel.Basic);
            UnmergePackFiles(partialEditQarEntries, partialRemoveFpkEntries);

            Debug.LogLine("[Uninstall] Removing any unmodified fpk entries", Debug.LogLevel.Basic);
            RemoveDemoddedQars(ref zeroFiles, fullRemoveQarPaths);

            bool isRemovingWMV = false;
            GameData gameData = SBBuildManager.GetGameData();
            foreach (ModEntry uninstallMod in uninstallMods)
            {
                SBBuildManager.RemoveMod(uninstallMod);

                Debug.LogLine(string.Format("[Uninstall] Removing any game dir file entries for {0}", uninstallMod.Name), Debug.LogLevel.Basic);
                UninstallGameDirEntries(uninstallMod, ref gameData);

                Debug.LogLine(string.Format("[Uninstall] Removing any loose textures for {0}", uninstallMod.Name), Debug.LogLevel.Basic);
                UninstallLooseFtexs(uninstallMod, ref oneFilesList, ref gameData);
                if (!isRemovingWMV)
                {
                    if (uninstallMod.ModWmvEntries.Count > 0)
                    {
                        isRemovingWMV = true;
                    }
                }
            }

            SBBuildManager.SetGameData(gameData);
            if (isRemovingWMV)
            {
                ModManager.UpdateFoxfs(SBBuildManager.GetInstalledMods());
            }
        }

        private static void GetFpkRemovalLists(List<ModEntry> uninstallMods, out List<string> fullRemoveQarPaths, out List<ModQarEntry> partialEditQarEntries, out List<ModFpkEntry> partialEditFpkEntries)
        {
            List<string> fpkFilesToUninstall = new List<string>();
            foreach (ModEntry modToUninstall in uninstallMods)
            {
                foreach (ModFpkEntry fpkToUninstall in modToUninstall.ModFpkEntries)
                {
                    fpkFilesToUninstall.Add(fpkToUninstall.FilePath + fpkToUninstall.FpkFile);
                }
            }

            List<string> remainingModQarPaths = new List<string>();
            foreach (ModEntry mod in SBBuildManager.GetInstalledMods())
            {
                if (uninstallMods.Any(e => e.Name == mod.Name))
                {
                    continue;
                }

                foreach (ModFpkEntry remainingFpk in mod.ModFpkEntries)
                {
                    string remainingFpkString = remainingFpk.FilePath + remainingFpk.FpkFile;
                    if (fpkFilesToUninstall.Any(entry => entry == remainingFpkString))
                    {
                        continue;
                    }

                    if (!remainingModQarPaths.Contains(remainingFpk.FpkFile))
                    {
                        remainingModQarPaths.Add(remainingFpk.FpkFile);
                    }
                }
            }
            fullRemoveQarPaths = new List<string>();
            partialEditQarEntries = new List<ModQarEntry>();
            partialEditFpkEntries = new List<ModFpkEntry>();

            GameData gameData = SBBuildManager.GetGameData();
            foreach (ModEntry uninstallMod in uninstallMods)
            {
                foreach (ModQarEntry uninstallQarEntry in uninstallMod.ModQarEntries)
                {
                    string uninstallQarFilePath = uninstallQarEntry.FilePath;
                    if (partialEditQarEntries.Any(entry => entry.FilePath == uninstallQarFilePath) || fullRemoveQarPaths.Contains(uninstallQarFilePath))
                    {
                        continue;
                    }

                    if (!(uninstallQarFilePath.EndsWith(".fpk") || uninstallQarFilePath.EndsWith(".fpkd")))
                    {
                        fullRemoveQarPaths.Add(uninstallQarFilePath);
                        continue;
                    }

                    ModQarEntry existingGameQar = gameData.GameQarEntries.FirstOrDefault(entry => entry.FilePath == uninstallQarFilePath);
                    if (existingGameQar == null)
                    {
                        existingGameQar = uninstallQarEntry;
                    }

                    if (remainingModQarPaths.Contains(uninstallQarFilePath))
                    {
                        partialEditQarEntries.Add(existingGameQar);
                    }
                    else
                    {
                        fullRemoveQarPaths.Add(uninstallQarFilePath);
                    }
                }
                foreach (ModQarEntry partialEditQarEntry in partialEditQarEntries)
                {
                    foreach (ModFpkEntry modFpkEntry in uninstallMod.ModFpkEntries)
                    {
                        if (modFpkEntry.FpkFile == partialEditQarEntry.FilePath)
                        {
                            modFpkEntry.FpkFile = modFpkEntry.FpkFile;
                            partialEditFpkEntries.Add(modFpkEntry);
                            Debug.LogLine(string.Format("[RemovalList] Fpk flagged for removal: {0}", modFpkEntry.FilePath), Debug.LogLevel.Basic);
                        }
                    }
                }
            }
        }

        private static void UnmergePackFiles(List<ModQarEntry> partialEditQarEntries, List<ModFpkEntry> partialRemoveFpkEntries)
        {
            GameData gameData = SBBuildManager.GetGameData();
            List<ModFpkEntry> addedRepairFpkEntries = new List<ModFpkEntry>();

            foreach (ModQarEntry partialEditQarEntry in partialEditQarEntries)
            {
                List<string> fpkPathsForThisQar = partialRemoveFpkEntries.Where(entry => entry.FpkFile == partialEditQarEntry.FilePath).Select(fpkEntry => Tools.ToWinPath(fpkEntry.FilePath)).ToList();
                List<string> fpkReferences = new List<string>();//tex references in fpk that need to be preserved/transfered to the rebuilt fpk
                string winQarEntryPath = Tools.ToWinPath(partialEditQarEntry.FilePath);
                string gameQarPath = Path.Combine("_gameFpk", winQarEntryPath);
                if (partialEditQarEntry.SourceName != null)
                {
                    string vanillaArchivePath = Path.Combine(GameDir, "master\\" + partialEditQarEntry.SourceName);
                    GzsLib.ExtractFileByHash<QarFile>(vanillaArchivePath, partialEditQarEntry.Hash, gameQarPath);
                    fpkReferences = GzsLib.GetFpkReferences(gameQarPath);
                }
                string workingZeroQarPath = Path.Combine("_working0", winQarEntryPath);
                List<string> moddedFpkFiles = GzsLib.ExtractArchive<FpkFile>(workingZeroQarPath, "_build");
                List<string> repairFilePathList = new List<string>();
                if (partialEditQarEntry.SourceName != null)
                {
                    repairFilePathList = GzsLib.ListArchiveContents<FpkFile>(gameQarPath).Intersect(fpkPathsForThisQar).ToList();
                }

                List<string> removeFilePathList = fpkPathsForThisQar.Except(repairFilePathList).ToList();

                foreach (string repairFilePath in repairFilePathList)
                {
                    string fpkBuildPath = Path.Combine("_build", repairFilePath);
                    Debug.LogLine(string.Format("[Unmerge Fpk] Extracting repair file: {0}", repairFilePath), Debug.LogLevel.Basic);
                    GzsLib.ExtractFile<FpkFile>(gameQarPath, repairFilePath, fpkBuildPath);
                    ModFpkEntry repairEntry = new ModFpkEntry
                    {
                        FpkFile = partialEditQarEntry.FilePath,
                        FilePath = repairFilePath,
                        SourceType = FileSource.Merged,
                        SourceName = partialEditQarEntry.SourceName
                    };
                    gameData.GameFpkEntries.Add(repairEntry);
                    addedRepairFpkEntries.Add(repairEntry);
                }

                List<string> buildFiles = moddedFpkFiles.Except(removeFilePathList).ToList();
                if (fpkReferences.Count == 0)
                {
                    fpkReferences = GzsLib.GetFpkReferences(workingZeroQarPath);
                }
                GzsLib.WriteFpkArchive(workingZeroQarPath, "_build", buildFiles, fpkReferences);
                foreach (string removeFilePath in removeFilePathList)
                {
                    int indexToRemove = gameData.GameFpkEntries.FindIndex(entry => entry.FilePath == removeFilePath);
                    if (indexToRemove >= 0)
                    {
                        gameData.GameFpkEntries.RemoveAt(indexToRemove);
                    }
                }
            }

            List<ModEntry> installedMods = SBBuildManager.GetInstalledMods();
            foreach (ModEntry installedMod in installedMods)
            {
                List<string> qarPathsFound = new List<string>();
                foreach (ModFpkEntry addedRepairEntry in addedRepairFpkEntries)
                {
                    if (installedMod.ModQarEntries.FirstOrDefault(entry => entry.FilePath == addedRepairEntry.FpkFile) == null)
                    {
                        continue;
                    }
                    if (installedMod.ModFpkEntries.RemoveAll(entry => entry.FilePath == Tools.ToQarPath(addedRepairEntry.FilePath) && entry.FpkFile == addedRepairEntry.FpkFile) > 0)
                    {
                        if (!qarPathsFound.Contains(addedRepairEntry.FpkFile))
                        {
                            qarPathsFound.Add(addedRepairEntry.FpkFile);
                        }
                    }
                }

                foreach (string qarPathFound in qarPathsFound)
                {
                    if (installedMod.ModFpkEntries.FirstOrDefault(entry => entry.FpkFile == qarPathFound) == null)
                    {
                        installedMod.ModQarEntries.RemoveAll(entry => entry.FilePath == qarPathFound);
                    }
                }
            }
            SBBuildManager.SetInstalledMods(installedMods);
            SBBuildManager.SetGameData(gameData);
        }

        private static void RemoveDemoddedQars(ref List<string> zeroFiles, List<string> fullRemoveQarPaths)
        {
            zeroFiles = zeroFiles.Except(fullRemoveQarPaths.Select(entry => Tools.ToWinPath(entry))).ToList();
            List<ModEntry> installedMods = SBBuildManager.GetInstalledMods();
            GameData gameData = SBBuildManager.GetGameData();
            foreach (string fullRemoveQarPath in fullRemoveQarPaths)
            {
                int indexToRemove = gameData.GameQarEntries.FindIndex(entry => entry.FilePath == fullRemoveQarPath);
                if (indexToRemove >= 0)
                {
                    gameData.GameQarEntries.RemoveAt(indexToRemove);
                }

                gameData.GameFpkEntries = gameData.GameFpkEntries.Where(entry => entry.FpkFile != fullRemoveQarPath).ToList();
                foreach (ModEntry installedMod in installedMods)
                {
                    indexToRemove = installedMod.ModQarEntries.FindIndex(entry => entry.FilePath == fullRemoveQarPath);
                    if (indexToRemove >= 0)
                    {
                        installedMod.ModQarEntries.RemoveAt(indexToRemove);
                    }

                    installedMod.ModFpkEntries = installedMod.ModFpkEntries.Where(entry => entry.FpkFile != fullRemoveQarPath).ToList();
                }
            }
            SBBuildManager.SetInstalledMods(installedMods);
            SBBuildManager.SetGameData(gameData);

        }

        private static void UninstallGameDirEntries(ModEntry mod, ref GameData gameData)
        {
            HashSet<string> fileEntryDirs = new HashSet<string>();
            foreach (ModFileEntry fileEntry in mod.ModFileEntries)
            {
                string destFile = Path.Combine(GameDirSB_Build, Tools.ToWinPath(fileEntry.FilePath));
                string dir = Path.GetDirectoryName(destFile);
                fileEntryDirs.Add(dir);
                if (File.Exists(destFile))
                {
                    try
                    {
                        File.Delete(destFile);
                    }
                    catch
                    {
                        Console.WriteLine("[Uninstall] Could not delete: " + destFile);
                    }
                }
                gameData.GameFileEntries.RemoveAll(file => Tools.CompareHashes(file.FilePath, fileEntry.FilePath));
            }//foreach ModFileEntries
            foreach (string fileEntryDir in fileEntryDirs)
            {
                if (Directory.Exists(fileEntryDir) && Directory.GetFiles(fileEntryDir).Length == 0)
                {
                    Debug.LogLine(string.Format("[Uninstall] deleting empty folder: {0}", fileEntryDir), Debug.LogLevel.All);
                    try
                    {
                        Directory.Delete(fileEntryDir);
                    }
                    catch
                    {
                        Console.WriteLine("[Uninstall] Could not delete: " + fileEntryDir);
                    }
                }
            }//foreach fileEntryDirs
        }//UninstallGameDirEntries

        private static void UninstallLooseFtexs(ModEntry mod, ref List<string> oneFilesList, ref GameData gameData)
        {
            foreach (ModQarEntry qarEntry in mod.ModQarEntries)
            {
                if (qarEntry.FilePath.Contains(".ftex"))
                {
                    string destFile = Path.Combine("_working1", qarEntry.FilePath);
                    if (File.Exists(destFile))
                    {
                        try
                        {
                            File.Delete(destFile);
                        }
                        catch
                        {
                            Console.WriteLine("[Uninstall] Could not delete: " + destFile);
                        }
                    }
                    gameData.GameQarEntries.RemoveAll(file => Tools.CompareHashes(file.FilePath, qarEntry.FilePath));
                    oneFilesList.RemoveAll(file => Tools.CompareHashes(file, qarEntry.FilePath));
                }
            }
        }//UninstallLooseFtexs
    }
}
