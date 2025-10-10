using Shifr.Modul.Data;
using Shifr.WorkFile.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shifr.WorkFile
{
    internal class ShifrClient
    {
        private readonly JsonCryption _jsonManager;
        private readonly FileEncryptor _fileEncryptor;

        public ShifrClient()
        {
            _jsonManager = new JsonCryption();
            _fileEncryptor = new FileEncryptor();
        }

        public void EncryptFile(string filePath, string password)
        {
            _fileEncryptor.EncryptFile(filePath, password);
            LogOperation(filePath, true);
        }

        public bool DecryptFile(string encryptedFileFullPath, string password)
        {
            if (string.IsNullOrWhiteSpace(encryptedFileFullPath))
                throw new ArgumentNullException(nameof(encryptedFileFullPath));

            // Нормализуем путь
            string full = encryptedFileFullPath;
            try { full = Path.GetFullPath(encryptedFileFullPath); } catch { }
            if (!File.Exists(full) && File.Exists(full + ".enc"))
                full = full + ".enc";

            // Если файл не существует — выбросим
            if (!File.Exists(full))
                throw new FileNotFoundException("Файл для дешифровки не найден: " + full);

            // Дешифруем
            _fileEncryptor.DecryptFile(full, password);

            // Обновляем статус в истории — первым аргументом передаём полный путь к зашифрованному файлу
            bool updated = _jsonManager.UpdateEncryptionStatus(full, false);

            // Если не нашлось по полному пути — как запасной вариант пробуем по имени
            if (!updated)
                updated = _jsonManager.UpdateEncryptionStatus(Path.GetFileName(full), false);

            return true;
        }

        private void LogOperation(string targetPath, bool isEncrypt)
        {
            var record = new IData(
                System.IO.Path.GetDirectoryName(targetPath),
                System.IO.Path.GetFileName(targetPath),
                DateTime.Now
            )
            {
                IsEncryption = isEncrypt
            };

            _jsonManager.AddRecord(record);
        }

        public List<IData> GetOperationHistory()
        {
            var allRecords = _jsonManager.GetRecords();
            return allRecords.Where(r => r.IsEncryption).ToList();
        }
    }
}

