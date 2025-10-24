using Shifr.WorkFile.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Shifr.WorkFile
{
    public enum CipherType
    {
        Caesar,
        Vigenere,
        Playfair
    }

    internal class ShifrClient
    {
        private readonly JsonCryption _jsonManager;
        private readonly FileEncryptor _textCryption;

        public ShifrClient()
        {
            _jsonManager = new JsonCryption();
            _textCryption = new FileEncryptor();
        }

        public void EncryptFile(string filePath, string key, CipherType algorithm)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");

            string extension = Path.GetExtension(filePath).ToLower();
            string[] textExtensions = {
                ".txt", ".csv", ".xml", ".json", ".log", ".ini", ".config",
                ".html", ".htm", ".css", ".js", ".sql", ".md", ".rtf",
                ".bat", ".ps1", ".py", ".java", ".cpp", ".c", ".h", ".cs",
                ".php", ".rb", ".pl", ".sh", ".yaml", ".yml", ".properties",
                ".doc", ".docx"
            };

            if (!textExtensions.Contains(extension))
                throw new InvalidOperationException("Можно шифровать только текстовые файлы.");

            string content;
            if (extension == ".doc" || extension == ".docx")
                content = WordHelper.ReadWordText(filePath);
            else
                content = File.ReadAllText(filePath);

            string encrypted = _textCryption.ProcessText(content, algorithm, true, key);

            if (extension == ".doc" || extension == ".docx")
                WordHelper.WriteWordText(filePath, encrypted);
            else
                File.WriteAllText(filePath, encrypted);

            LogOperation(filePath, true, algorithm, key);
        }

        public void DecryptFile(string filePath, string key, CipherType algorithm)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");

            string extension = Path.GetExtension(filePath).ToLower();
            string[] textExtensions = {
                ".txt", ".csv", ".xml", ".json", ".log", ".ini", ".config",
                ".html", ".htm", ".css", ".js", ".sql", ".md", ".rtf",
                ".bat", ".ps1", ".py", ".java", ".cpp", ".c", ".h", ".cs",
                ".php", ".rb", ".pl", ".sh", ".yaml", ".yml", ".properties",
                ".doc", ".docx"
            };
            if (!textExtensions.Contains(extension))
                throw new InvalidOperationException("Можно дешифровать только текстовые файлы.");

            string encrypted = File.ReadAllText(filePath);
            string decrypted = _textCryption.ProcessText(encrypted, algorithm, false, key);
            File.WriteAllText(filePath, decrypted);

            LogOperation(filePath, false, algorithm, key);
        }

        public string ProcessText(string text, CipherType type, bool isEncrypt, string key, string filePath = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Текст не может быть пустым.");

            string result = _textCryption.ProcessText(text, type, isEncrypt, key);

            if (!string.IsNullOrEmpty(filePath))
                LogOperation(filePath, isEncrypt, type, key);

            return result;
        }

        public List<DataRecord> GetOperationHistory()
        {
            var allRecords = _jsonManager.GetRecords();
            return allRecords.OrderByDescending(r => r.Date).ToList();
        }

        public List<DataRecord> GetFileHistory(string filePath)
        {
            var allRecords = _jsonManager.GetRecords();
            return allRecords
                .Where(r => r.File == Path.GetFileName(filePath) || r.Path == Path.GetDirectoryName(filePath))
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        public void ClearHistory() => _jsonManager.ClearHistory();

        private void LogOperation(string filePath, bool isEncrypt, CipherType cipherType, string key)
        {
            var record = new DataRecord
            {
                Path = Path.GetDirectoryName(filePath),
                File = Path.GetFileName(filePath),
                Date = DateTime.Now,
                CipherType = cipherType,
                Key = key,
                IsEncryption = isEncrypt
            };

            _jsonManager.AddRecord(record);
        }
    }
}
