using ProgressionGear.JSON;
using ProgressionGear.Utils;
using GTFO.API.Utilities;
using MTFO.API;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using EWC.Dependencies;

namespace ProgressionGear.ProgressionLock
{
    public sealed class GearToggleManager
    {
        public static readonly GearToggleManager Current = new();

        private readonly Dictionary<string, List<GearToggleData>> _fileToData = new();
        private readonly Dictionary<uint, ToggleInfo> _toggleInfos = new();

        private readonly LiveEditListener _liveEditListener;

        private void FileChanged(LiveEditEventArgs e)
        {
            PWLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                ReadFileContent(e.FullPath, content);
            });
        }

        private void FileDeleted(LiveEditEventArgs e)
        {
            PWLogger.Warning($"LiveEdit File Removed: {e.FullPath}");

            _fileToData.Remove(e.FullPath);
            ResetToggleInfos();
            RefreshLocks();
        }

        private void FileCreated(LiveEditEventArgs e)
        {
            PWLogger.Warning($"LiveEdit File Created: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                ReadFileContent(e.FullPath, content);
            });
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
                PWLogger.Error("Error parsing progression lock json " + file);
                PWLogger.Error(ex.Message);
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
                PWLogger.Log("No GearToggle directory detected. Creating template.");
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(PWJson.Serialize(new List<GearToggleData>() { new() }));
                file.Flush();
                file.Close();
            }
            else
                PWLogger.Log("GearToggle directory detected.");

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                string content = File.ReadAllText(confFile);
                ReadFileContent(confFile, content);
            }

            _liveEditListener = LiveEdit.CreateListener(DEFINITION_PATH, "*.json", true);
            _liveEditListener.FileCreated += FileCreated;
            _liveEditListener.FileChanged += FileChanged;
            _liveEditListener.FileDeleted += FileDeleted;
        }

        internal void Init()
        {
            MTFOHotReloadAPI.OnHotReload += ResetToggleInfos;
        }

        public bool IsVisibleID(uint id) => !_toggleInfos.TryGetValue(id, out var toggleInfo) || toggleInfo.ids[0] == id;
        public bool HasRelatedIDs(uint id) => _toggleInfos.ContainsKey(id);
        public ToggleInfo GetToggleInfo(uint id) => _toggleInfos.GetValueOrDefault(id);
        public bool TryGetToggleInfo(uint id, [MaybeNullWhen(false)] out ToggleInfo toggleInfo) => _toggleInfos.TryGetValue(id, out toggleInfo);

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
            if (!_toggleInfos.TryGetValue(id, out ToggleInfo toggleInfo)) return;

            toggleInfo.ids.Remove(id);
            if (toggleInfo.ids.Count == 1)
                _toggleInfos.Remove(toggleInfo.ids[0]);
            _toggleInfos.Remove(id);
        }

        public void ResetToggleInfos()
        {
            _toggleInfos.Clear();
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
                        PWLogger.Warning($"Duplicate ID {id} detected in toggle data. Removed from {data.Name}");
                }
                if (relatedIDs.Count <= 1) continue;

                RemoveInvalidGear(relatedIDs, data.Name);
                if (relatedIDs.Count <= 1) continue;

                ToggleInfo toggleInfo = new() { ids = relatedIDs, text = data.ButtonText };
                foreach (uint id in relatedIDs)
                    _toggleInfos[id] = toggleInfo;
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
                            PWLogger.Warning($"ID {id} removed from toggle data {name} since it is not of type {inventorySlot}");
                            relatedIDs.RemoveAt(i);
                        }
                    }
                    return;
                }
                // ID not found on any loaded gear slot. Does not exist.
                PWLogger.Warning($"ID {relatedIDs[0]} removed from toggle data {name} since it does not exist.");
                relatedIDs.RemoveAt(0);
            }
        }
    }

    public struct ToggleInfo
    {
        public List<uint> ids;
        public Localization.LocalizedText text;
    }
}
