using Shifr.WorkFile.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Shifr.WorkFile
{
    internal class JsonCryption
    {

        private readonly string _appPath;
        private readonly string _historyFile;
        private readonly object _fileLock = new object();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions();

        public JsonCryption()
        {
            string myAppPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Shifr"
            );

            Directory.CreateDirectory(myAppPath);
            _historyFile = Path.Combine(myAppPath, "history.json");
        }

        public void AddRecord(IData data)
        {
            try
            {
                List<IData> history = new List<IData>();
                if (File.Exists(_historyFile))
                {
                    string json = File.ReadAllText(_historyFile);
                    history = JsonSerializer.Deserialize<List<IData>>(json) ?? new List<IData>();
                }

                history.Add(data);
                string jsonData = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_historyFile, jsonData);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при добавлении записи: {ex.Message}");
            }
        }

        public List<IData> GetRecords()
        {
            try
            {
                if (!File.Exists(_historyFile))
                    return new List<IData>();

                string json = File.ReadAllText(_historyFile);
                return JsonSerializer.Deserialize<List<IData>>(json) ?? new List<IData>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при чтении истории: {ex.Message}");
            }
        }

        public bool UpdateEncryptionStatus(string filePathOrFileName, bool isEncrypted)
        {
            if (string.IsNullOrWhiteSpace(filePathOrFileName))
                return false;

            lock (_fileLock)
            {
                var history = ReadHistoryInternal();
                if (history.Count == 0) return false;

                // Нормализуем вход
                string inputFull = null;
                try
                {
                    if (Path.IsPathRooted(filePathOrFileName))
                        inputFull = Path.GetFullPath(filePathOrFileName);
                }
                catch { inputFull = null; }

                string inputFileName = Path.GetFileName(filePathOrFileName);
                string inputFileNoEnc = inputFileName;
                if (!string.IsNullOrEmpty(inputFileNoEnc) && inputFileNoEnc.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                    inputFileNoEnc = inputFileNoEnc.Substring(0, inputFileNoEnc.Length - 4);
                string inputFileWithEnc = inputFileName.EndsWith(".enc", StringComparison.OrdinalIgnoreCase) ? inputFileName : inputFileName + ".enc";

                var matches = new List<IData>();

                // 1) По полному пути
                if (!string.IsNullOrEmpty(inputFull))
                {
                    foreach (var r in history)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(r.File)) continue;
                            string recFull = Path.GetFullPath(Path.Combine(r.Path ?? "", r.File));
                            if (string.Equals(recFull, inputFull, StringComparison.OrdinalIgnoreCase))
                            {
                                matches.Add(r);
                            }
                        }
                        catch { /* игнорируем */ }
                    }
                }

                // 2) По имени файла (и вариантах с/без .enc)
                if (matches.Count == 0 && !string.IsNullOrEmpty(inputFileName))
                {
                    matches = history.Where(r =>
                    {
                        if (string.IsNullOrEmpty(r.File)) return false;
                        var rf = r.File;
                        if (string.Equals(rf, inputFileName, StringComparison.OrdinalIgnoreCase)) return true;
                        if (string.Equals(rf, inputFileNoEnc, StringComparison.OrdinalIgnoreCase)) return true;
                        if (string.Equals(rf, inputFileWithEnc, StringComparison.OrdinalIgnoreCase)) return true;
                        return false;
                    }).ToList();
                }

                if (matches.Count == 0) return false;

                foreach (var m in matches)
                {
                    m.IsEncryption = isEncrypted;
                    m.Date = DateTime.Now;
                }

                WriteHistoryInternal(history);
                return true;
            }
        }

        public void ClearHistory()
        {
            lock (_fileLock)
            {
                if (File.Exists(_historyFile)) File.Delete(_historyFile);
            }
        }

        #region IO helpers
        private List<IData> ReadHistoryInternal()
        {
            try
            {
                if (!File.Exists(_historyFile)) return new List<IData>();
                string json = File.ReadAllText(_historyFile);
                var list = JsonSerializer.Deserialize<List<IData>>(json, _jsonOptions);
                return list ?? new List<IData>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка чтения history.json: {ex.Message}", ex);
            }
        }

        private void WriteHistoryInternal(List<IData> history)
        {
            try
            {
                string json = JsonSerializer.Serialize(history, _jsonOptions);
                File.WriteAllText(_historyFile, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка записи history.json: {ex.Message}", ex);
            }
        }
        #endregion
    }
}


