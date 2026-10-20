using GTFO.API.Utilities;
using MTFO.API;
using ProgressionGear.Dependencies;
using ProgressionGear.JSON;
using ProgressionGear.ProgressionLock;
using ProgressionGear.Utils;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ProgressionGear.GearToggle
{
    public sealed class GearToggleManager
    {
        public static readonly GearToggleManager Current = new();

        private readonly Dictionary<string, List<GearToggleData>> _fileToData = new();
        private readonly Dictionary<uint, ToggleInfo> _idToInfo = new();

        private void FileChanged(FileEventArgs e)
        {
            DinoLogger.Warning($"File Changed: {e.FileName}");
            ReadFileContent(e.FullPath, e.ReadContent());
        }

        private void FileDeleted(FileEventArgs e)
        {
            DinoLogger.Warning($"File Removed: {e.FileName}");

            _fileToData.Remove(e.FullPath);
            ResetToggleInfos();
            RefreshLocks();
        }

        private void FileCreated(FileEventArgs e)
        {
            DinoLogger.Warning($"File Created: {e.FileName}");
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

            List<GearToggleData>? dataList = null;
            try
            {
                dataList = PWJson.Deserialize<List<GearToggleData>>(content);
            }
            catch (JsonException ex)
            {
                DinoLogger.Error("Error parsing progression lock json " + file);
                DinoLogger.Error(ex.Message);
            }

            if (dataList == null) return;

            _fileToData[file] = dataList;

            ResetToggleInfos();
            RefreshLocks();
        }

        private static void RefreshLocks() => GearLockManager.Current.SetupAllowedGearsForActiveRundown();

        private GearToggleManager()
        {
            string DEFINITION_PATH = Path.Combine(MTFOWrapper.CustomPath, EntryPoint.MODNAME, "GearToggle");
            if (!Directory.Exists(DEFINITION_PATH))
            {
                DinoLogger.Log("No GearToggle directory detected. Creating template.");
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(PWJson.Serialize(GearToggleData.Template));
                file.Flush();
                file.Close();
            }
            else
                DinoLogger.Log("GearToggle directory detected.");

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

        internal void Init()
        {
            MTFOHotReloadAPI.OnHotReload += ResetToggleInfos;
        }

        public bool IsVisibleID(uint id) => !_idToInfo.TryGetValue(id, out var toggleInfo) || toggleInfo.ids[0] == id;
        public bool HasRelatedIDs(uint id) => _idToInfo.ContainsKey(id);
        public ToggleInfo GetToggleInfo(uint id) => _idToInfo.GetValueOrDefault(id);
        public bool TryGetToggleInfo(uint id, [MaybeNullWhen(false)] out ToggleInfo toggleInfo) => _idToInfo.TryGetValue(id, out toggleInfo);

        public List<GearToggleData> GetData()
        {
            List<GearToggleData> dataList = new(_fileToData.Values.Sum(list => list.Count));
            IEnumerator<GearToggleData> dataEnumerator = GetEnumerator();
            while(dataEnumerator.MoveNext())
                dataList.Add(dataEnumerator.Current);
            return dataList;
        }

        public IEnumerator<GearToggleData> GetEnumerator() => new DictListEnumerator<GearToggleData>(_fileToData);

        public void RemoveFromToggleInfos(uint id)
        {
            if (!_idToInfo.TryGetValue(id, out ToggleInfo info)) return;

            // Remove from toggle list
            info.ids.Remove(id);

            // Remove the toggle list if only a single element remains
            if (info.ids.Count == 1)
                _idToInfo.Remove(info.ids[0]);
            _idToInfo.Remove(id);
        }

        public void ResetToggleInfos()
        {
            _idToInfo.Clear();
            HashSet<uint> seen = new();

            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                GearToggleData data = enumerator.Current;
                if (data.OfflineIDs.Count <= 1) continue;

                List<uint> relatedIDs = new(data.OfflineIDs.Count);

                // Prevent duplicate IDs across lists/the same list
                foreach (uint id in data.OfflineIDs)
                {
                    if (seen.Add(id))
                        relatedIDs.Add(id);
                    else
                        DinoLogger.Warning($"Duplicate ID {id} detected in toggle data. Removed from {data.Name}");
                }
                if (relatedIDs.Count <= 1) continue;

                RemoveInvalidGear(relatedIDs, data.Name);
                if (relatedIDs.Count <= 1) continue;

                ToggleInfo toggleInfo = new() { ids = relatedIDs, text = data.ButtonText, reverse = data.ReverseOrder };
                foreach (uint id in relatedIDs)
                    _idToInfo[id] = toggleInfo;
            }
        }

        private void RemoveInvalidGear(List<uint> relatedIDs, string name)
        {
            if (GearLockManager.Current.VanillaGearManager == null) return;

            while (relatedIDs.Count > 1)
            {
                foreach (var (inventorySlot, loadedGears) in GearLockManager.Current.GearSlots)
                {
                    // Using the first ID as the intended inventory slot
                    if (!loadedGears.ContainsKey(relatedIDs[0])) continue;

                    for (int i = relatedIDs.Count - 1; i >= 1; i--)
                    {
                        uint id = relatedIDs[i];
                        if (!loadedGears.ContainsKey(id))
                        {
                            DinoLogger.Warning($"ID {id} removed from toggle data {name} since it is not of type {inventorySlot}");
                            relatedIDs.RemoveAt(i);
                        }
                    }
                    return;
                }
                // ID not found on any loaded gear slot. Does not exist.
                DinoLogger.Warning($"ID {relatedIDs[0]} removed from toggle data {name} since it does not exist.");
                relatedIDs.RemoveAt(0);
            }
        }
    }

    public struct ToggleInfo
    {
        public List<uint> ids;
        public Localization.LocalizedText[] text;
        public bool reverse;
    }
}
