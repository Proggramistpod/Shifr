using Shifr.WorkFile.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shifr.WorkFile
{
    internal class JsonCryption
    {
        private readonly string _historyFile;
        private readonly object _fileLock = new object();

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public JsonCryption()
        {
            string appPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Shifr"
            );

            Directory.CreateDirectory(appPath);
            _historyFile = Path.Combine(appPath, "history.json");

            if (!File.Exists(_historyFile))
                File.WriteAllText(_historyFile, "[]");
        }

        public void AddRecord(DataRecord record)
        {
            lock (_fileLock)
            {
                var history = ReadHistoryInternal();
                history.Add(record);
                WriteHistoryInternal(history);
            }
        }

        public List<DataRecord> GetRecords()
        {
            lock (_fileLock)
            {
                return ReadHistoryInternal();
            }
        }

        public void ClearHistory()
        {
            lock (_fileLock)
            {
                if (File.Exists(_historyFile))
                    File.WriteAllText(_historyFile, "[]");
            }
        }

        private List<DataRecord> ReadHistoryInternal()
        {
            try
            {
                if (!File.Exists(_historyFile))
                    return new List<DataRecord>();

                string json = File.ReadAllText(_historyFile);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<DataRecord>();

                var list = JsonSerializer.Deserialize<List<DataRecord>>(json, _jsonOptions);
                return list ?? new List<DataRecord>();
            }
            catch (Exception ex)
            {
                File.AppendAllText("error_log.txt", $"[{DateTime.Now}] Read error: {ex}\n");
                return new List<DataRecord>();
            }
        }

        private void WriteHistoryInternal(List<DataRecord> history)
        {
            try
            {
                string json = JsonSerializer.Serialize(history, _jsonOptions);
                File.WriteAllText(_historyFile, json);
            }
            catch (Exception ex)
            {
                File.AppendAllText("error_log.txt", $"[{DateTime.Now}] Write error: {ex}\n");
            }
        }
    }
}
