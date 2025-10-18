using GzsTool.Core.Fpk;
using GzsTool.Core.Qar;
using ICSharpCode.SharpZipLib.Zip;
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
    internal static class InstallManager
    {
        public static bool InstallMods(List<string> ModFiles, bool skipCleanup = false)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Debug.LogLine("[Install] Start", Debug.LogLevel.Basic);
            ModManager.ClearBuildFiles(ZeroPath, OnePath, SnakeBiteSettings, SavePresetPath);
            ModManager.ClearSBGameDir();
            ModManager.CleanupFolders();

            if (Properties.Settings.Default.AutosaveRevertPreset == true)
            {
                PresetManager.SavePreset(SavePresetPath + build_ext);
            }
            else
            {
                Debug.LogLine("[Install] Skipping RevertChanges.MGSVPreset Save", Debug.LogLevel.Basic);
            }
            File.Copy(SnakeBiteSettings, SnakeBiteSettings + build_ext, true);

            GzsLib.LoadDictionaries();
            List<ModEntry> installEntryList = new List<ModEntry>();
            foreach (string modFile in ModFiles)
            {
                installEntryList.Add(Tools.ReadMetaData(modFile));
            }

            List<string> zeroFiles = new List<string>();
            bool hasQarZero = ModManager.hasQarZeroFiles(installEntryList);
            if (hasQarZero)
            {
                zeroFiles = GzsLib.ExtractArchive<QarFile>(ZeroPath, "_working0");
            }

            List<string> oneFiles = null;
            bool hasFtexs = ModManager.foundLooseFtexs(installEntryList);
            if (hasFtexs)
            {
                oneFiles = GzsLib.ExtractArchive<QarFile>(OnePath, "_working1");
            }

            SettingsManager SBBuildManager = new SettingsManager(SnakeBiteSettings + build_ext);
            GameData gameData = SBBuildManager.GetGameData();
            ModManager.ValidateGameData(ref gameData, ref zeroFiles);

            HashSet<string> zeroFilesHashSet = new HashSet<string>(zeroFiles);

            Debug.LogLine("[Install] Building gameFiles lists", Debug.LogLevel.Basic);
            List<Dictionary<ulong, GameFile>> baseGameFiles = GzsLib.ReadBaseData();
            List<Dictionary<ulong, GameFile>> allQarGameFiles = new List<Dictionary<ulong, GameFile>>();
            allQarGameFiles.AddRange(baseGameFiles);


            try
            {
                ModManager.PrepGameDirFiles();

                Debug.LogLine("[Install] Writing FPK data to Settings", Debug.LogLevel.Basic);
                AddToSettingsFpk(installEntryList, SBBuildManager, allQarGameFiles, out List<string> pullFromVanillas, out List<string> pullFromMods, out Dictionary<string, bool> pathUpdatesExist);
                InstallMods(ModFiles, SBBuildManager, pullFromVanillas, pullFromMods, ref zeroFilesHashSet, ref oneFiles, pathUpdatesExist);

                if (hasQarZero)
                {
                    zeroFiles = zeroFilesHashSet.ToList();
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

                stopwatch.Stop();
                Debug.LogLine($"[Install] Installation finished in {stopwatch.ElapsedMilliseconds} ms", Debug.LogLevel.Basic);
                return true;
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                Debug.LogLine($"[Install] Installation failed at {stopwatch.ElapsedMilliseconds} ms", Debug.LogLevel.Basic);
                Debug.LogLine("[Install] Exception: " + e, Debug.LogLevel.Basic);
                MessageBox.Show("An error has occurred during the installation process and SnakeBite could not install the selected mod(s).\nException: " + e, "Mod(s) could not be installed", MessageBoxButtons.OK, MessageBoxIcon.Error);

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
        }
        private static void InstallMods(List<string> modFilePaths, SettingsManager manager, List<string> pullFromVanillas, List<string> pullFromMods, ref HashSet<string> zeroFiles, ref List<string> oneFilesList, Dictionary<string, bool> pathUpdatesExist)
        {
            FastZip unzipper = new FastZip();
            GameData gameData = manager.GetGameData();

            bool isAddingWMV = false;
            foreach (string modFilePath in modFilePaths)
            {
                Debug.LogLine($"[Install] Installation started: {Path.GetFileName(modFilePath)}", Debug.LogLevel.Basic);

                Debug.LogLine($"[Install] Unzipping mod .mgsv ({Tools.GetFileSizeKB(modFilePath)} KB)", Debug.LogLevel.Basic);
                unzipper.ExtractZip(modFilePath, "_extr", "(.*?)");

                Debug.LogLine("[Install] Load mod metadata", Debug.LogLevel.Basic);
                ModEntry extractedModEntry = new ModEntry("_extr\\metadata.xml");
                if (pathUpdatesExist[extractedModEntry.Name])
                {
                    Debug.LogLine(string.Format("[Install] Checking for Qar path updates: {0}", extractedModEntry.Name), Debug.LogLevel.Basic);
                    foreach (ModQarEntry modQar in extractedModEntry.ModQarEntries.Where(entry => !entry.FilePath.StartsWith("/Assets/")))
                    {
                        string unhashedName = HashingExtended.UpdateName(modQar.FilePath);
                        if (unhashedName != null)
                        {
                            Debug.LogLine(string.Format("[Install] Update successful: {0} -> {1}", modQar.FilePath, unhashedName), Debug.LogLevel.Basic);

                            string workingOldPath = Path.Combine("_extr", Tools.ToWinPath(modQar.FilePath));
                            string workingNewPath = Path.Combine("_extr", Tools.ToWinPath(unhashedName));
                            if (!Directory.Exists(Path.GetDirectoryName(workingNewPath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(workingNewPath));
                            }

                            if (!File.Exists(workingNewPath))
                            {
                                File.Move(workingOldPath, workingNewPath);
                            }

                            modQar.FilePath = unhashedName;

                        }
                    }
                }

                GzsLib.LoadModDictionary(extractedModEntry);
                ValidateModEntries(ref extractedModEntry);

                Debug.LogLine("[Install] Check mod FPKs against game .dat fpks", Debug.LogLevel.Basic);
                zeroFiles.UnionWith(MergePacks(extractedModEntry, pullFromVanillas, pullFromMods));

                Debug.LogLine("[Install] Copying loose textures to 01.", Debug.LogLevel.Basic);
                InstallLooseFtexs(extractedModEntry, ref oneFilesList);

                Debug.LogLine("[Install] Copying game dir files", Debug.LogLevel.Basic);
                InstallGameDirFiles(extractedModEntry, ref gameData);
                if (!isAddingWMV)
                {
                    if (extractedModEntry.ModWmvEntries.Count > 0)
                    {
                        isAddingWMV = true;
                    }
                }
            }

            manager.SetGameData(gameData);
            if (isAddingWMV)
            {
                ModManager.UpdateFoxfs(manager.GetInstalledMods());
            }
        }

        private static void ValidateModEntries(ref ModEntry modEntry)
        {
            Debug.LogLine("[ValidateModEntries] Validating qar entries", Debug.LogLevel.Basic);
            for (int i = modEntry.ModQarEntries.Count - 1; i >= 0; i--)
            {
                ModQarEntry qarEntry = modEntry.ModQarEntries[i];
                if (!GzsLib.IsExtensionValidForArchive(qarEntry.FilePath, ".dat"))
                {
                    Debug.LogLine($"[ValidateModEntries] Found invalid file entry {qarEntry.FilePath} for archive {qarEntry.SourceName}", Debug.LogLevel.Basic);
                    modEntry.ModQarEntries.RemoveAt(i);
                }
            }
            Debug.LogLine("[ValidateModEntries] Validating fpk entries", Debug.LogLevel.Basic);
            for (int i = modEntry.ModFpkEntries.Count - 1; i >= 0; i--)
            {
                ModFpkEntry fpkEntry = modEntry.ModFpkEntries[i];
                if (!GzsLib.IsExtensionValidForArchive(fpkEntry.FilePath, fpkEntry.FpkFile))
                {
                    Debug.LogLine($"[ValidateModEntries] Found invalid file entry {fpkEntry.FilePath} for archive {fpkEntry.FpkFile}", Debug.LogLevel.Basic);
                    modEntry.ModFpkEntries.RemoveAt(i);
                }
            }
        }

        private static HashSet<string> MergePacks(ModEntry extractedModEntry, List<string> pullFromVanillas, List<string> pullFromMods)
        {
            HashSet<string> modQarZeroPaths = new HashSet<string>();
            foreach (ModQarEntry modQar in extractedModEntry.ModQarEntries)
            {
                string workingDestination = Path.Combine("_working0", Tools.ToWinPath(modQar.FilePath));
                if (!Directory.Exists(Path.GetDirectoryName(workingDestination)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(workingDestination));
                }

                string modQarSource = Path.Combine("_extr", Tools.ToWinPath(modQar.FilePath));
                string existingQarSource;

                if (pullFromMods.FirstOrDefault(e => e == modQar.FilePath) != null)
                {
                    existingQarSource = workingDestination;
                }
                else
                {
                    int indexToRemove = pullFromVanillas.FindIndex(m => m == modQar.FilePath);
                    if (indexToRemove >= 0)
                    {
                        existingQarSource = Path.Combine("_gameFpk", Tools.ToWinPath(modQar.FilePath));
                        pullFromVanillas.RemoveAt(indexToRemove); pullFromMods.Add(modQar.FilePath);
                    }
                    else
                    {
                        existingQarSource = null;
                        if (modQar.FilePath.EndsWith(".fpk") || modQar.FilePath.EndsWith(".fpkd"))
                        {
                            pullFromMods.Add(modQar.FilePath);
                        }
                    }
                }

                if (existingQarSource != null)
                {
                    List<string> pulledPack = GzsLib.ExtractArchive<FpkFile>(existingQarSource, "_build");
                    List<string> extrPack = GzsLib.ExtractArchive<FpkFile>(modQarSource, "_build");
                    pulledPack = pulledPack.Union(extrPack).ToList();
                    List<string> fpkReferences = GzsLib.GetFpkReferences(existingQarSource);
                    GzsLib.WriteFpkArchive(workingDestination, "_build", pulledPack, fpkReferences);
                }
                else
                {
                    File.Copy(modQarSource, workingDestination, true);
                }

                if (!modQar.FilePath.Contains(".ftex"))
                {
                    modQarZeroPaths.Add(Tools.ToWinPath(modQar.FilePath));
                }
            }

            return modQarZeroPaths;
        }
        private static void InstallLooseFtexs(ModEntry modEntry, ref List<string> oneFilesList)
        {
            foreach (ModQarEntry modQarEntry in modEntry.ModQarEntries)
            {
                if (modQarEntry.FilePath.Contains(".ftex"))
                {
                    if (!oneFilesList.Contains(Tools.ToWinPath(modQarEntry.FilePath)))
                    {
                        oneFilesList.Add(Tools.ToWinPath(modQarEntry.FilePath));
                    }
                    string sourceFile = Path.Combine("_extr", Tools.ToWinPath(modQarEntry.FilePath));
                    string destFile = Path.Combine("_working1", Tools.ToWinPath(modQarEntry.FilePath));
                    string destDir = Path.GetDirectoryName(destFile);
                    Debug.LogLine(string.Format("[Install] Copying texture file: {0}", modQarEntry.FilePath), Debug.LogLevel.All);
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    File.Copy(sourceFile, destFile, true);
                }
            }
        }
        private static void InstallGameDirFiles(ModEntry modEntry, ref GameData gameData)
        {
            foreach (ModFileEntry fileEntry in modEntry.ModFileEntries)
            {
                bool skipFile = false;
                foreach (string ignoreFile in Tools.ignoreFileList)
                {
                    if (fileEntry.FilePath.Contains(ignoreFile))
                    {
                        skipFile = true;
                    }
                }
                if (skipFile == false)
                {
                    string sourceFile = Path.Combine("_extr\\GameDir", Tools.ToWinPath(fileEntry.FilePath));
                    string destFile = Path.Combine(GameDirSB_Build, Tools.ToWinPath(fileEntry.FilePath));
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                    File.Copy(sourceFile, destFile, true);

                    if (gameData.GameFileEntries.FirstOrDefault(e => e.FilePath == fileEntry.FilePath) == null)
                    {
                        gameData.GameFileEntries.Add(fileEntry);
                    }
                }
            }
        }//InstallGameDirFiles

        private static void AddToSettingsFpk(List<ModEntry> installEntryList, SettingsManager manager, List<Dictionary<ulong, GameFile>> allQarGameFiles, out List<string> PullFromVanillas, out List<string> pullFromMods, out Dictionary<string, bool> pathUpdatesExist)
        {
            GameData gameData = manager.GetGameData();
            pathUpdatesExist = new Dictionary<string, bool>();

            List<string> newModQarEntries = new List<string>();
            List<string> modQarFiles = manager.GetModQarFiles();
            pullFromMods = new List<string>();
            foreach (ModEntry modToInstall in installEntryList)
            {
                Dictionary<string, string> newNameDictionary = new Dictionary<string, string>();
                int foundUpdate = 0;
                foreach (ModQarEntry modQar in modToInstall.ModQarEntries.Where(entry => !entry.FilePath.StartsWith("/Assets/")))
                {
                    string unhashedName = HashingExtended.UpdateName(modQar.FilePath);
                    if (unhashedName != null)
                    {
                        newNameDictionary.Add(modQar.FilePath, unhashedName);
                        foundUpdate++;

                        modQar.FilePath = unhashedName;
                        if (!pathUpdatesExist.ContainsKey(modToInstall.Name))
                        {
                            pathUpdatesExist.Add(modToInstall.Name, true);
                        }
                        else
                        {
                            pathUpdatesExist[modToInstall.Name] = true;
                        }
                    }
                }
                if (foundUpdate > 0)
                {
                    foreach (ModFpkEntry modFpkEntry in modToInstall.ModFpkEntries)
                    {
                        if (newNameDictionary.TryGetValue(modFpkEntry.FpkFile, out string unHashedName))
                        {
                            modFpkEntry.FpkFile = unHashedName;
                        }
                    }
                }
                else if (!pathUpdatesExist.ContainsKey(modToInstall.Name))
                {
                    pathUpdatesExist.Add(modToInstall.Name, false);
                }

                manager.AddMod(modToInstall);
                foreach (ModQarEntry modQarEntry in modToInstall.ModQarEntries)
                {
                    string modQarFilePath = modQarEntry.FilePath;
                    if (!(modQarFilePath.EndsWith(".fpk") || modQarFilePath.EndsWith(".fpkd")))
                    {
                        continue;
                    }

                    if (modQarFiles.Any(entry => entry == modQarFilePath))
                    {
                        pullFromMods.Add(modQarFilePath);
                    }
                    else if (!newModQarEntries.Contains(modQarFilePath))
                    {
                        newModQarEntries.Add(modQarFilePath);
                    }

                }
            }
            List<ModFpkEntry> newModFpkEntries = new List<ModFpkEntry>();
            foreach (ModEntry modToInstall in installEntryList)
            {
                foreach (ModFpkEntry modFpkEntry in modToInstall.ModFpkEntries)
                {

                    if (newModQarEntries.Contains(modFpkEntry.FpkFile))
                    {
                        newModFpkEntries.Add(modFpkEntry);
                    }
                    else
                    {
                        int indexToRemove = gameData.GameFpkEntries.FindIndex(m => m.FilePath == Tools.ToWinPath(modFpkEntry.FilePath));
                        if (indexToRemove >= 0)
                        {
                            gameData.GameFpkEntries.RemoveAt(indexToRemove);
                        }
                    }
                }
            }
            HashSet<ulong> mergeFpkHashes = new HashSet<ulong>();
            PullFromVanillas = new List<string>();
            List<ModFpkEntry> repairFpkEntries = new List<ModFpkEntry>();
            foreach (ModFpkEntry newFpkEntry in newModFpkEntries)
            {
                ulong packHash = Tools.NameToHash(newFpkEntry.FpkFile);
                if (mergeFpkHashes.Contains(packHash))
                {
                    continue;
                }

                foreach (Dictionary<ulong, GameFile> archiveQarGameFiles in allQarGameFiles)
                {
                    if (archiveQarGameFiles.Count > 0)
                    {
                        archiveQarGameFiles.TryGetValue(packHash, out GameFile existingPack);
                        if (existingPack != null)
                        {
                            mergeFpkHashes.Add(packHash);
                            gameData.GameQarEntries.Add(new ModQarEntry
                            {
                                FilePath = newFpkEntry.FpkFile,
                                SourceType = FileSource.Merged,
                                SourceName = existingPack.QarFile,
                                Hash = existingPack.FileHash
                            });
                            PullFromVanillas.Add(newFpkEntry.FpkFile);

                            string windowsFilePath = Tools.ToWinPath(newFpkEntry.FpkFile);
                            string sourceArchive = Path.Combine(GameDir, "master\\" + existingPack.QarFile);
                            string workingPath = Path.Combine("_gameFpk", windowsFilePath);
                            if (!Directory.Exists(Path.GetDirectoryName(workingPath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(workingPath));
                            }

                            GzsLib.ExtractFileByHash<QarFile>(sourceArchive, existingPack.FileHash, workingPath);
                            foreach (string listedFile in GzsLib.ListArchiveContents<FpkFile>(workingPath))
                            {
                                repairFpkEntries.Add(new ModFpkEntry
                                {
                                    FpkFile = newFpkEntry.FpkFile,
                                    FilePath = listedFile,
                                    SourceType = FileSource.Merged,
                                    SourceName = existingPack.QarFile
                                });
                            }
                            break;
                        }
                    }
                }
            }
            foreach (ModFpkEntry newFpkEntry in newModFpkEntries)
            {
                int indexToRemove = repairFpkEntries.FindIndex(m => m.FilePath == Tools.ToWinPath(newFpkEntry.FilePath));
                if (indexToRemove >= 0)
                {
                    repairFpkEntries.RemoveAt(indexToRemove);
                }
            }
            gameData.GameFpkEntries = gameData.GameFpkEntries.Union(repairFpkEntries).ToList();
            manager.SetGameData(gameData);
        }
    }
}
