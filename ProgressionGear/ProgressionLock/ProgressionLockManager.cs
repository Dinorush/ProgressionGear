using ProgressionGear.JSON;
using ProgressionGear.Utils;
using GTFO.API.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ProgressionGear.Dependencies;

namespace ProgressionGear.ProgressionLock
{
    public sealed class ProgressionLockManager
    {
        public static readonly ProgressionLockManager Current = new();

        private readonly Dictionary<string, List<ProgressionLockData>> _fileToData = new();

        private void FileChanged(FileEventArgs e)
        {
            DinoLogger.Warning($"File Changed: {e.FullPath}");
            ReadFileContent(e.FullPath, e.ReadContent());
        }

        private void FileDeleted(FileEventArgs e)
        {
            DinoLogger.Warning($"File Removed: {e.FullPath}");

            _fileToData.Remove(e.FullPath);
            RefreshLocks();
        }

        private void FileCreated(FileEventArgs e)
        {
            DinoLogger.Warning($"File Created: {e.FullPath}");
            ReadFileContent(e.FullPath, e.ReadContent());
        }

        private void FileRenamed(FileRenamedEventArgs e)
        {
            DinoLogger.Warning($"File Renamed: {e.OldFileName} -> {e.FileName}");
            if (_fileToData.TryGetValue(e.OldFullPath, out var dataList))
            {
                _fileToData.Remove(e.OldFullPath);
                _fileToData[e.FullPath] = dataList;
            }
        }

        private void ReadFileContent(string file, string content)
        {
            _fileToData.Remove(file);

            List<ProgressionLockData>? dataList = null;
            try
            {
                dataList = PWJson.Deserialize<List<ProgressionLockData>>(content);
            }
            catch (JsonException ex)
            {
                DinoLogger.Error("Error parsing progression lock json " + file);
                DinoLogger.Error(ex.Message);
            }

            if (dataList == null) return;

            _fileToData[file] = dataList;
            RefreshLocks();
        }

        private static void RefreshLocks() => GearLockManager.Current.SetupAllowedGearsForActiveRundown();

        private ProgressionLockManager()
        {
            string DEFINITION_PATH = Path.Combine(MTFOWrapper.CustomPath, EntryPoint.MODNAME, "ProgressionLocks");
            if (!Directory.Exists(DEFINITION_PATH))
            {
                DinoLogger.Log("No ProgressionLocks directory detected. Creating template.");
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(PWJson.Serialize(ProgressionLockData.Template));
                file.Flush();
                file.Close();
            }
            else
                DinoLogger.Log("ProgressionLocks directory detected.");

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                ReadFileContent(confFile, content);
            }

            var sfsw = SafeFileSystemWatcher.Create(DEFINITION_PATH, new string[] { "*.json" }, true);
            sfsw.OnCreated += FileCreated;
            sfsw.OnChanged += FileChanged;
            sfsw.OnDeleted += FileDeleted;
            sfsw.OnRenamed += FileRenamed;
        }

        internal void Init() { }

        public List<ProgressionLockData> GetLockData()
        {
            List<ProgressionLockData> dataList = new(_fileToData.Values.Sum(list => list.Count));
            IEnumerator<ProgressionLockData> dataEnumerator = GetEnumerator();
            while(dataEnumerator.MoveNext())
                dataList.Add(dataEnumerator.Current);
            return dataList;
        }

        public IEnumerator<ProgressionLockData> GetEnumerator() => new DictListEnumerator<ProgressionLockData>(_fileToData);
    }
}
