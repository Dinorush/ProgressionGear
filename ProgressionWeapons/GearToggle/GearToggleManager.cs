﻿using ProgressionWeapons.JSON;
using ProgressionWeapons.Utils;
using GTFO.API.Utilities;
using MTFO.API;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace ProgressionWeapons.ProgressionLock
{
    public sealed class GearToggleManager
    {
        public static readonly GearToggleManager Current = new();

        private readonly Dictionary<string, List<GearToggleData>> _fileToData = new();
        private readonly Dictionary<uint, List<uint>> _relatedIDs = new();

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
            ResetRelatedIDs();
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

            ResetRelatedIDs();
        }

        private GearToggleManager()
        {
            string DEFINITION_PATH = Path.Combine(MTFOPathAPI.CustomPath, EntryPoint.MODNAME, "GearToggle");
            if (!Directory.Exists(DEFINITION_PATH))
            {
                PWLogger.Log("No directory detected. Creating " + DEFINITION_PATH + "/Template.json");
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(PWJson.Serialize(new List<GearToggleData>() { new() }));
                file.Flush();
                file.Close();
            }
            else
                PWLogger.Log("Directory detected. " + DEFINITION_PATH);

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

        internal void Init() { }

        public bool IsVisibleID(uint id) => !_relatedIDs.TryGetValue(id, out var relatedIDs) || relatedIDs[0] == id;
        public bool HasRelatedIDs(uint id) => _relatedIDs.ContainsKey(id);
        public List<uint>? GetRelatedIDs(uint id) => _relatedIDs.GetValueOrDefault(id);
        public bool TryGetRelatedIDs(uint id, [MaybeNullWhen(false)] out List<uint> relatedIDs) => _relatedIDs.TryGetValue(id, out relatedIDs);

        public List<GearToggleData> GetData()
        {
            List<GearToggleData> dataList = new(_fileToData.Values.Sum(list => list.Count));
            IEnumerator<GearToggleData> dataEnumerator = GetEnumerator();
            while(dataEnumerator.MoveNext())
                dataList.Add(dataEnumerator.Current);
            return dataList;
        }

        public IEnumerator<GearToggleData> GetEnumerator() => new DictListEnumerator<GearToggleData>(_fileToData);

        public void RemoveFromRelatedIDs(uint id)
        {
            if (!_relatedIDs.TryGetValue(id, out List<uint>? relatedIDs)) return;

            relatedIDs.Remove(id);
            if (relatedIDs.Count == 1)
                _relatedIDs.Remove(relatedIDs[0]);
            _relatedIDs.Remove(id);
        }

        public void ResetRelatedIDs()
        {
            _relatedIDs.Clear();
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

                foreach (uint id in relatedIDs)
                    _relatedIDs[id] = relatedIDs;
            }
        }

        private void RemoveInvalidGear(List<uint> relatedIDs, string name)
        {
            if (GearLockManager.Instance.VanillaGearManager == null) return;

            foreach (var (inventorySlot, loadedGears) in GearLockManager.Instance.GearSlots)
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
            }
        }
    }
}
