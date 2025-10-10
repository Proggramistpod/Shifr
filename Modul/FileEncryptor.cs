using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace Shifr.WorkFile
{
    internal class FileEncryptor
    {
        private static readonly int SaltSize = 16;
        private static readonly int KeySize = 32;
        private static readonly int Iterations = 10000;

        public void EncryptFile(string inputFile, string password)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException($"Файл не найден: {inputFile}");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

            byte[] salt = GenerateSalt();
            byte[] key = GenerateKey(password, salt);

            string tempFile = inputFile + ".tmp";
            string outputFile = inputFile + ".enc";

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.GenerateIV();

                    using (FileStream fsIn = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 81920))
                    using (FileStream fsCrypt = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920))
                    {
                        // Записываем соль и IV
                        fsCrypt.Write(salt, 0, salt.Length);
                        fsCrypt.Write(aes.IV, 0, aes.IV.Length);

                        using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            BufferedCopy(fsIn, cs, 81920);
                        }
                    }
                }

                SafeDeleteFile(inputFile);
                File.Move(tempFile, outputFile);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                throw new EncryptionException($"Ошибка шифрования файла {inputFile}", ex);
            }
        }
        public void DecryptFile(string inputFile, string password, int bufferSize = 81920)
        {
            if (!File.Exists(inputFile))
                throw new FileNotFoundException($"Файл не найден: {inputFile}");

            if (!inputFile.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Файл должен иметь расширение .enc", nameof(inputFile));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(password));

            byte[] salt = new byte[SaltSize];
            byte[] iv = new byte[16];
            string tempFile = Path.ChangeExtension(inputFile, ".dec.tmp");
            string outputFile = Path.ChangeExtension(inputFile, null);

            try
            {
                using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
                {
                    if (fsCrypt.Read(salt, 0, salt.Length) != SaltSize)
                        throw new InvalidDataException("Неверный формат зашифрованного файла (ошибка чтения соли)");

                    if (fsCrypt.Read(iv, 0, iv.Length) != iv.Length)
                        throw new InvalidDataException("Неверный формат зашифрованного файла (ошибка чтения IV)");

                    byte[] key = GenerateKey(password, salt);

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = key;
                        aes.IV = iv;

                        using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        using (FileStream fsOut = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize))
                        {
                            BufferedCopy(cs, fsOut, bufferSize);
                        }
                    }
                }
                SafeDeleteFile(inputFile);
                File.Move(tempFile, outputFile);
            }
            catch (CryptographicException ex)
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                throw new DecryptionException("Ошибка дешифрования: возможно, неверный пароль.", ex);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                throw new DecryptionException($"Ошибка дешифрования файла {inputFile}", ex);
            }
        }

        public void EncryptFolder(string folderPath, string password)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Путь к папке не может быть пустым.", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Папка не найдена: {folderPath}");

            var stack = new Stack<string>();
            stack.Push(folderPath);

            while (stack.Count > 0)
            {
                string currentDir = stack.Pop();

                foreach (string file in Directory.GetFiles(currentDir))
                {
                    if (!file.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                        EncryptFile(file, password);
                }

                foreach (string subDir in Directory.GetDirectories(currentDir))
                    stack.Push(subDir);
            }
        }
        public void DecryptFolder(string folderPath, string password)
        {
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Путь к папке не может быть пустым.", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Папка не найдена: {folderPath}");

            var stack = new Stack<string>();
            stack.Push(folderPath);

            while (stack.Count > 0)
            {
                string currentDir = stack.Pop();

                foreach (string file in Directory.GetFiles(currentDir))
                {
                    if (file.EndsWith(".enc", StringComparison.OrdinalIgnoreCase))
                        DecryptFile(file, password);
                }

                foreach (string subDir in Directory.GetDirectories(currentDir))
                    stack.Push(subDir);
            }
        }
        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            return salt;
        }

        private byte[] GenerateKey(string password, byte[] salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                return deriveBytes.GetBytes(KeySize);
        }
        private void SafeDeleteFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return;
                FileInfo info = new FileInfo(filePath);
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                {
                    byte[] zeros = new byte[4096];
                    long remaining = info.Length;
                    while (remaining > 0)
                    {
                        int toWrite = (int)Math.Min(zeros.Length, remaining);
                        fs.Write(zeros, 0, toWrite);
                        remaining -= toWrite;
                    }
                }

                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Не удалось удалить файл: {filePath}", ex);
            }
        }

        private void BufferedCopy(Stream source, Stream destination, int bufferSize)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                destination.Write(buffer, 0, bytesRead);
        }
    }
    public class EncryptionException : Exception
    {
        public EncryptionException(string message) : base(message) { }
        public EncryptionException(string message, Exception inner) : base(message, inner) { }
    }

    public class DecryptionException : Exception
    {
        public DecryptionException(string message) : base(message) { }
        public DecryptionException(string message, Exception inner) : base(message, inner) { }
    }
}


