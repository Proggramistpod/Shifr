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
