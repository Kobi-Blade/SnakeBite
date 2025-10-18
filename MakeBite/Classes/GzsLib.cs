// SYNC to MakeBite
using GzsTool.Core.Common;
using GzsTool.Core.Common.Interfaces;
using GzsTool.Core.Fpk;
using GzsTool.Core.Qar;
using GzsTool.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SnakeBite.GzsTool
{
    public static class GzsLib
    {
        public static uint zeroFlags = 3150304;
        public static uint oneFlags = 3150048;
        public static uint chunk0Flags = 3146208;
        public static uint chunk7Flags = 3146208;
        public static uint texture7Flags = 3150048;
        private static readonly Dictionary<string, List<string>> archiveExtensions = new Dictionary<string, List<string>> {
            {"dat",new List<string> {
                "bnk",
                "dat",
                "ffnt",
                "fmtt",
                "fpk",
                "fpkd",
                "fsm",
                "fsop",
                "ftex",
                "ftexs",
                "json",
                "lua",
                "pftxs",
                "sbp",
                "subp",
                "wem",
            }},
            {"fpk",new List<string> {
                "caar",
                "fnt",
                "atsh",
                "frig",
                "adm",
                "frt",
                "fpkl",
                "fsm",
                "ftdp",
                "geobv",
                "ftex",
                "geoms",
                "gimr",
                "gpfp",
                "grxla",
                "grxoc",
                "htre",
                "lba",
                "lpsh",
                "mog",
                "mtar",
                "nav2",
                "nta",
                "rdf",
                "ends",
                "sand",
                "mbl",
                "tcvp",
                "spch",
                "trap",
                "uigb",
                "uilb",
                "pcsp",
                "tre2",
                "fstb",
                "twpf",
                "fv2t",
                "fmdl",
                "geom",
                "gskl",
                "fcnp",
                "frdv",
                "fdes",
                "fclo",
                "uif",
                "uia",
                "subp",
                "sani",
                "ladb",
                "frl",
                "fv2",
                "obr",
                "lng2",
                "mtard",
                "obrb",
                "dfrm"
            }},
              {"fpkd",new List<string> {
                "fox2",
                "evf",
                "parts",
                "vfxlb",
                "vfx",
                "vfxlf",
                "veh",
                "frld",
                "des",
                "bnd",
                "tgt",
                "phsd",
                "ph",
                "sim",
                "clo",
                "fsd",
                "sdf",
                "lua",
                "lng",
            }},
        };

        private static readonly Dictionary<string, string> extensionToType = new Dictionary<string, string> {
            {"dat", "QarFile"},
            {"fpk", "FpkFile" },
            {"fpkd", "FpkFile" },
        };
        public static List<string> ExtractArchive<T>(string FileName, string OutputPath) where T : ArchiveFile, new()
        {
            if (!File.Exists(FileName))
            {
                Debug.LogLine($"[GzsLib] File not found: {FileName}");
                throw new FileNotFoundException();
            }
            else
            {
                string name = Path.GetFileName(FileName);
                Debug.LogLine($"[GzsLib] Extracting {name} to {OutputPath} ({Tools.GetFileSizeKB(FileName)} KB)");

                using (FileStream archiveFile = new FileStream(FileName, FileMode.Open))
                {
                    List<string> outFiles = new List<string>();
                    T archive = new T
                    {
                        Name = Path.GetFileName(FileName)
                    };
                    archive.Read(archiveFile);
                    IEnumerable<FileDataStreamContainer> exportedFiles = archive.ExportFiles(archiveFile);
                    foreach (FileDataStreamContainer v in exportedFiles)
                    {
                        string outDirectory = Path.Combine(OutputPath, Path.GetDirectoryName(v.FileName));
                        string outFileName = Path.Combine(OutputPath, v.FileName);
                        if (!Directory.Exists(outDirectory))
                        {
                            Directory.CreateDirectory(outDirectory);
                        }

                        using (FileStream outStream = new FileStream(outFileName, FileMode.Create))
                        {
                            v.DataStream().CopyTo(outStream);
                            outFiles.Add(v.FileName);
                        }
                    }
                    Debug.LogLine($"[GzsLib] Extracted {outFiles.Count} files from {name}");
                    return outFiles;
                }
            }
        }
        public static bool ExtractFile<T>(string SourceArchive, string FilePath, string OutputFile) where T : ArchiveFile, new()
        {
            if (!File.Exists(SourceArchive))
            {
                Debug.LogLine($"[GzsLib] File not found: {SourceArchive}");
                throw new FileNotFoundException();
            }
            else
            {
                Debug.LogLine(string.Format("[GzsLib] Extracting file {1}: {0} -> {2}", FilePath, SourceArchive, OutputFile));
                ulong fileHash = Tools.NameToHash(FilePath);

                using (FileStream archiveFile = new FileStream(SourceArchive, FileMode.Open))
                {
                    T archive = new T
                    {
                        Name = Path.GetFileName(SourceArchive)
                    };
                    archive.Read(archiveFile);
                    FileDataStreamContainer outFile = archive.ExportFiles(archiveFile).FirstOrDefault(entry => Tools.NameToHash(entry.FileName) == fileHash);

                    if (outFile != null)
                    {
                        string path = Path.GetDirectoryName(Path.GetFullPath(OutputFile));
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        using (FileStream outStream = new FileStream(OutputFile, FileMode.Create))
                        {
                            outFile.DataStream().CopyTo(outStream);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

        }
        public static bool ExtractFileByHash<T>(string SourceArchive, ulong FileHash, string OutputFile) where T : ArchiveFile, new()
        {
            if (!File.Exists(SourceArchive))
            {
                Debug.LogLine($"[GzsLib] File not found: {SourceArchive}");
                throw new FileNotFoundException();
            }
            else
            {
                Debug.LogLine(string.Format("[GzsLib] Extracting file from {1}: hash {0} -> {2}", FileHash, SourceArchive, OutputFile));
                ulong fileHash = FileHash;

                using (FileStream archiveFile = new FileStream(SourceArchive, FileMode.Open))
                {
                    T archive = new T
                    {
                        Name = Path.GetFileName(SourceArchive)
                    };
                    archive.Read(archiveFile);
                    FileDataStreamContainer outFile = archive.ExportFiles(archiveFile).FirstOrDefault(entry => Tools.NameToHash(entry.FileName) == fileHash);

                    if (outFile != null)
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(OutputFile)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(OutputFile));
                        }

                        using (FileStream outStream = new FileStream(OutputFile, FileMode.Create))
                        {
                            outFile.DataStream().CopyTo(outStream);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

        }

        public static T ReadArchive<T>(string FileName) where T : ArchiveFile, new()
        {
            if (!File.Exists(FileName))
            {
                Debug.LogLine($"[GzsLib] File not found: {FileName}");
                throw new FileNotFoundException();
            }
            else
            {
                string name = Path.GetFileName(FileName);
                Debug.LogLine($"[GzsLib] Reading {name})");

                using (FileStream archiveFile = new FileStream(FileName, FileMode.Open))
                {
                    List<string> outFiles = new List<string>();
                    T archive = new T
                    {
                        Name = Path.GetFileName(FileName)
                    };
                    archive.Read(archiveFile);
                    return archive;
                }//using fileStream
            }//if File.Exists
        }//ReadArchive

        public static List<string> GetFpkReferences(string fpkPath)
        {
            List<string> fpkReferences = new List<string>();
            FpkFile fpkFile = GzsLib.ReadArchive<FpkFile>(fpkPath);
            foreach (FpkReference reference in fpkFile.References)
            {
                fpkReferences.Add(reference.FilePath);
            }
            return fpkReferences;
        }//GetFpkReferences
        public static List<GameFile> ListArchiveHashes<T>(string ArchiveName) where T : ArchiveFile, new()
        {
            if (!File.Exists(ArchiveName))
            {
                Debug.LogLine($"[GzsLib] File not found: {ArchiveName}");
                throw new FileNotFoundException();
            }
            else
            {
                string name = Path.GetFileName(ArchiveName);
                Debug.LogLine($"[GzsLib] Reading archive contents: {name} ({Tools.GetFileSizeKB(ArchiveName)} KB)");
                using (FileStream archiveFile = new FileStream(ArchiveName, FileMode.Open))
                {
                    List<GameFile> archiveContents = new List<GameFile>();
                    T archive = new T
                    {
                        Name = Path.GetFileName(ArchiveName)
                    };
                    archive.Read(archiveFile);
                    foreach (FileDataStreamContainer x in archive.ExportFiles(archiveFile))
                    {
                        archiveContents.Add(new GameFile() { FilePath = x.FileName, FileHash = Tools.NameToHash(x.FileName), QarFile = archive.Name });
                    }
                    return archiveContents;
                }
            }
        }
        public static Dictionary<ulong, GameFile> GetQarGameFiles(string qarPath)
        {
            if (!File.Exists(qarPath))
            {
                Debug.LogLine($"[GzsLib] File not found: {qarPath}");
                throw new FileNotFoundException();
            }
            else
            {
                string name = Path.GetFileName(qarPath);
                Debug.LogLine($"[GzsLib] Reading archive contents: {name}");
                using (FileStream archiveFile = new FileStream(qarPath, FileMode.Open))
                {
                    Dictionary<ulong, GameFile> qarFiles = new Dictionary<ulong, GameFile>();
                    QarFile qarFile = new QarFile
                    {
                        Name = Path.GetFileName(qarPath)
                    };
                    qarFile.Read(archiveFile);
                    foreach (QarEntry entry in qarFile.Entries)
                    {
                        qarFiles[entry.Hash] = new GameFile() { FilePath = entry.FilePath, FileHash = entry.Hash, QarFile = qarFile.Name };
                    }
                    return qarFiles;
                }
            }
        }
        public static List<string> ListArchiveContents<T>(string ArchiveName) where T : ArchiveFile, new()
        {
            if (!File.Exists(ArchiveName))
            {
                Debug.LogLine($"[GzsLib] File not found: {ArchiveName}");
                throw new FileNotFoundException();
            }
            else
            {
                string name = Path.GetFileName(ArchiveName);
                Debug.LogLine($"[GzsLib] Reading archive contents: {name}");
                using (FileStream archiveFile = new FileStream(ArchiveName, FileMode.Open))
                {
                    List<string> archiveContents = new List<string>();
                    T archive = new T
                    {
                        Name = Path.GetFileName(ArchiveName)
                    };
                    archive.Read(archiveFile);
                    foreach (FileDataStreamContainer x in archive.ExportFiles(archiveFile))
                    {
                        archiveContents.Add(x.FileName);
                    }
                    return archiveContents;
                }
            }
        }
        public static void LoadDictionaries()
        {
            Debug.LogLine("[GzsLib] Loading base dictionaries");
            Hashing.ReadDictionary("qar_dictionary.txt");
            Hashing.ReadMd5Dictionary("fpk_dictionary.txt");
            HashingExtended.ReadDictionary();

#if SNAKEBITE
            LoadModDictionaries();
#endif
        }

#if SNAKEBITE
        public static void LoadModDictionaries()
        {
            SettingsManager manager = new SettingsManager(GamePaths.SnakeBiteSettings);
            var QarNames = manager.GetModQarFiles(true);
            File.WriteAllLines("mod_qar_dict.txt", QarNames);
            Hashing.ReadDictionary("mod_qar_dict.txt");
        }
        public static void LoadModDictionary(ModEntry modEntry)
        {
            Debug.LogLine("[GzsLib] Loading mod dictionary");

            List<string> qarNames = new List<string>();
            foreach (ModQarEntry qarFile in modEntry.ModQarEntries)
            {
                string fileName = Tools.ToQarPath(qarFile.FilePath.Substring(0, qarFile.FilePath.IndexOf(".")));
                qarNames.Add(fileName);
            }

            File.WriteAllLines("mod_qar_dict.txt", qarNames);
            Hashing.ReadDictionary("mod_qar_dict.txt");
        }
        public static List<Dictionary<ulong, GameFile>> ReadBaseData()
        {
            Debug.LogLine("[GzsLib] Acquiring base game data");

            var baseDataFiles = new List<Dictionary<ulong, GameFile>>();
            string dataDir = Path.Combine(GamePaths.GameDir, "master");
            var qarFileNames = new List<string> {
                "a_chunk7.dat",
                "data1.dat",
                "chunk0.dat",
                "chunk1.dat",
                "chunk2.dat",
                "chunk3.dat",
                "chunk4.dat",
                "chunk5_mgo0.dat",
                "chunk6_gzs0.dat",
            };

            foreach (var qarFileName in qarFileNames)
            {
                var path = Path.Combine(dataDir, qarFileName);
                if (!File.Exists(path))
                {
                    Debug.LogLine($"[GzsLib] Could not find {path}");
                } else
                {
                    var qarGameFiles = GetQarGameFiles(Path.Combine(dataDir, path));
                    baseDataFiles.Add(qarGameFiles);
                }
            }

            return baseDataFiles;
        }
#endif
        public static void WriteFpkArchive(string FileName, string SourceDirectory, List<string> Files, List<string> references)
        {
            Debug.LogLine(string.Format("[GzsLib] Writing FPK archive: {0}", FileName));
            string fpkType = FileName.EndsWith(".fpkd") ? "fpkd" : "fpk";
            List<string> fpkFilesSorted = SortFpksFiles(fpkType, Files);

            FpkFile q = new FpkFile() { Name = FileName, FpkType = fpkType == "fpkd" ? FpkType.Fpkd : FpkType.Fpk };
            foreach (string s in fpkFilesSorted)
            {
                q.Entries.Add(new FpkEntry() { FilePath = Tools.ToQarPath(s) });
            }
            foreach (string fpk in references)
            {
                FpkReference reference = new FpkReference()
                {
                    ReferenceFilePath = new FpkString() { Value = fpk }
                };
                q.References.Add(reference);
            }

            using (FileStream outFile = new FileStream(FileName, FileMode.Create))
            {
                IDirectory fileDirectory = new FileSystemDirectory(SourceDirectory);
                q.Write(outFile, fileDirectory);
            }
        }
        public static void WriteQarArchive(string FileName, string SourceDirectory, List<string> Files, uint Flags)
        {
            Debug.LogLine($"[GzsLib] Writing {Path.GetFileName(FileName)}");
            List<QarEntry> qarEntries = new List<QarEntry>();
            foreach (string s in Files)
            {
                if (s.EndsWith("_unknown")) { continue; }
                qarEntries.Add(new QarEntry() { FilePath = s, Hash = Tools.NameToHash(s), Compressed = (Path.GetExtension(s).EndsWith(".fpk") || Path.GetExtension(s).EndsWith(".fpkd")) });
            }

            QarFile q = new QarFile() { Entries = qarEntries, Flags = Flags, Name = FileName };

            using (FileStream outFile = new FileStream(FileName, FileMode.Create))
            {
                IDirectory fileDirectory = new FileSystemDirectory(SourceDirectory);
                q.Write(outFile, fileDirectory);
            }
        }

        public static void PromoteQarArchive(string sourcePath, string destinationPath)
        {
            if (File.Exists(sourcePath))
            {
                Debug.LogLine($"[GzsLib] Promoting {Path.GetFileName(sourcePath)} to {Path.GetFileName(destinationPath)} ({Tools.GetFileSizeKB(sourcePath)} KB)");
                File.Delete(destinationPath);
                File.Move(sourcePath, destinationPath);
            }
            else
            {
                Debug.LogLine($"[GzsLib] {sourcePath} not found");
            }
        }
        public static List<string> SortFpksFiles(string FpkType, List<string> fpkFiles)
        {
            if (fpkFiles.Count <= 1)
            {
                return fpkFiles;
            }
            if (FpkType == "fpk")
            {
                fpkFiles.Sort(StringComparer.Ordinal);
            }
            else
            {
                fpkFiles.Sort((a, b) => string.CompareOrdinal(b, a));
            }
            List<string> fpkFilesSorted = new List<string>();
            foreach (string archiveExtension in archiveExtensions[FpkType])
            {
                foreach (string fileName in fpkFiles)
                {
                    string fileExtension = Path.GetExtension(fileName).Substring(1);
                    if (archiveExtension == fileExtension)
                    {
                        fpkFilesSorted.Add(fileName);
                    }
                }
            }
            return fpkFilesSorted;
        }// SortFpksFiles

        public static bool IsExtensionValidForArchive(string fileName, string archiveName)
        {
            string archiveExtension = Path.GetExtension(archiveName).TrimStart('.');
            List<string> validExtensions = archiveExtensions[archiveExtension];
            string ext = Path.GetExtension(fileName).TrimStart('.');
            bool isValid = false;
            foreach (string validExt in validExtensions)
            {
                if (ext == validExt)
                {
                    isValid = true;
                    break;
                }
            }
            return isValid;
        }
    }
    public static class HashingExtended
    {
        private static readonly Dictionary<ulong, string> HashNameDictionary = new Dictionary<ulong, string>();

        private const ulong MetaFlag = 0x4000000000000;

        public static void ReadDictionary(string path = "qar_dictionary.txt")
        {
            foreach (string line in File.ReadAllLines(path))
            {
                ulong hash = HashFileName(line) & 0x3FFFFFFFFFFFF;
                if (HashNameDictionary.ContainsKey(hash) == false)
                {
                    HashNameDictionary.Add(hash, line);
                }
            }
        }

        public static string UpdateName(string inputFile)
        {
            string filename = Path.GetFileNameWithoutExtension(inputFile);
            string ext = Path.GetExtension(inputFile);
            string extInner = "";
            if (filename.Contains("."))
            {
                extInner = Path.GetExtension(filename);
                filename = Path.GetFileNameWithoutExtension(filename);
            }

            if (TryGetFileNameHash(filename, out ulong fileNameHash))
            {
                if (TryGetFilePathFromHash(fileNameHash, out string foundFileNoExt))
                {
                    return foundFileNoExt + extInner + ext;
                }

            }

            return null;
        }

        private static ulong HashFileName(string text, bool removeExtension = true)
        {
            if (removeExtension)
            {
                int index = text.IndexOf('.');
                text = index == -1 ? text : text.Substring(0, index);
            }

            bool metaFlag = false;
            const string assetsConstant = "/Assets/";
            if (text.StartsWith(assetsConstant))
            {
                text = text.Substring(assetsConstant.Length);

                if (text.StartsWith("tpptest"))
                {
                    metaFlag = true;
                }
            }
            else
            {
                metaFlag = true;
            }

            text = text.TrimStart('/');

            const ulong seed0 = 0x9ae16a3b2f90404f;
            byte[] seed1Bytes = new byte[sizeof(ulong)];
            for (int i = text.Length - 1, j = 0; i >= 0 && j < sizeof(ulong); i--, j++)
            {
                seed1Bytes[j] = Convert.ToByte(text[i]);
            }
            ulong seed1 = BitConverter.ToUInt64(seed1Bytes, 0);
            ulong maskedHash = CityHash.CityHash.CityHash64WithSeeds(text, seed0, seed1) & 0x3FFFFFFFFFFFF;

            return metaFlag
                ? maskedHash | MetaFlag
                : maskedHash;
        }

        private static bool TryGetFilePathFromHash(ulong hash, out string filePath)
        {
            bool foundFileName = true;
            ulong pathHash = hash & 0x3FFFFFFFFFFFF;

            if (!HashNameDictionary.TryGetValue(pathHash, out filePath))
            {
                foundFileName = false;
            }

            return foundFileName;
        }

        private static bool TryGetFileNameHash(string filename, out ulong fileNameHash)
        {
            bool isConverted = true;
            try
            {
                fileNameHash = Convert.ToUInt64(filename, 16);
            }
            catch (FormatException)
            {
                isConverted = false;
                fileNameHash = 0;
            }
            return isConverted;
        }
    }
}