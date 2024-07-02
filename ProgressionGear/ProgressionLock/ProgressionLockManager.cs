﻿using ProgressionGear.JSON;
using ProgressionGear.Utils;
using GTFO.API.Utilities;
using MTFO.API;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ProgressionGear.ProgressionLock
{
    public sealed class ProgressionLockManager
    {
        public static readonly ProgressionLockManager Current = new();

        private readonly Dictionary<string, List<ProgressionLockData>> _fileToData = new();

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

            List<ProgressionLockData>? dataList = null;
            try
            {
                dataList = PWJson.Deserialize<List<ProgressionLockData>>(content);
            }
            catch (JsonException ex)
            {
                PWLogger.Error("Error parsing progression lock json " + file);
                PWLogger.Error(ex.Message);
            }

            if (dataList == null) return;

            _fileToData[file] = dataList;
        }

        private ProgressionLockManager()
        {
            string DEFINITION_PATH = Path.Combine(MTFOPathAPI.CustomPath, EntryPoint.MODNAME, "ProgressionLocks");
            if (!Directory.Exists(DEFINITION_PATH))
            {
                PWLogger.Log("No directory detected. Creating " + DEFINITION_PATH + "/Template.json");
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(PWJson.Serialize(new List<ProgressionLockData>() { new ProgressionLockData() }));
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