using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Shifr.WorkFile
{

    internal class FileEncryptor
    {
        private const string EN = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string RU = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";

        public string ProcessText(string text, CipherType type, bool isEncrypt, string keyOrShift)
        {
            switch (type)
            {
                case CipherType.Caesar:
                    if (!int.TryParse(keyOrShift, out int shift))
                        throw new ArgumentException("Для шифра Цезаря параметр должен быть числом.");
                    return isEncrypt ? CaesarEncrypt(text, shift) : CaesarDecrypt(text, shift);
                case CipherType.Vigenere:
                    if (string.IsNullOrWhiteSpace(keyOrShift))
                        throw new ArgumentException("Для шифра Виженера нужен ключ (буквы).");
                    return isEncrypt ? VigenereEncrypt(text, keyOrShift) : VigenereDecrypt(text, keyOrShift);
                case CipherType.Playfair:
                    if (string.IsNullOrWhiteSpace(keyOrShift))
                        throw new ArgumentException("Для шифра Плейфера нужен ключ (буквы).");
                    return isEncrypt ? PlayfairEncrypt(text, keyOrShift) : PlayfairDecrypt(text, keyOrShift);

                default:
                    throw new NotSupportedException("Неизвестный тип шифра.");
            }
        }

        // === ЦЕЗАРЬ ===
        private string CaesarEncrypt(string text, int shift)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    bool isLower = char.IsLower(c);
                    char upperC = char.ToUpper(c);
                    string alphabet = GetAlphabet(upperC);
                    if (alphabet == null)
                    {
                        result.Append(c);
                        continue;
                    }

                    int index = alphabet.IndexOf(upperC);
                    int newIndex = (index + shift) % alphabet.Length;
                    if (newIndex < 0) newIndex += alphabet.Length;
                    char newChar = alphabet[newIndex];
                    result.Append(isLower ? char.ToLower(newChar) : newChar);
                }
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        private string CaesarDecrypt(string text, int shift) =>
            CaesarEncrypt(text, -shift);

        // === ВИЖЕНЕР ===
        private string VigenereEncrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            int keyIndex = 0;
            key = key.ToUpper();

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    bool isLower = char.IsLower(c);
                    char upperC = char.ToUpper(c);
                    char upperK = char.ToUpper(key[keyIndex % key.Length]);
                    string alphabet = GetAlphabet(upperC);

                    if (alphabet == null || GetAlphabet(upperK) != alphabet)
                    {
                        result.Append(c);
                        continue;
                    }

                    int shift = alphabet.IndexOf(upperK);
                    int index = alphabet.IndexOf(upperC);
                    int newIndex = (index + shift) % alphabet.Length;
                    char newChar = alphabet[newIndex];

                    result.Append(isLower ? char.ToLower(newChar) : newChar);
                    keyIndex++;
                }
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        private string VigenereDecrypt(string text, string key)
        {
            StringBuilder result = new StringBuilder();
            int keyIndex = 0;
            key = key.ToUpper();

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    bool isLower = char.IsLower(c);
                    char upperC = char.ToUpper(c);
                    char upperK = char.ToUpper(key[keyIndex % key.Length]);
                    string alphabet = GetAlphabet(upperC);

                    if (alphabet == null || GetAlphabet(upperK) != alphabet)
                    {
                        result.Append(c);
                        continue;
                    }

                    int shift = alphabet.IndexOf(upperK);
                    int index = alphabet.IndexOf(upperC);
                    int newIndex = (index - shift + alphabet.Length) % alphabet.Length;
                    char newChar = alphabet[newIndex];

                    result.Append(isLower ? char.ToLower(newChar) : newChar);
                    keyIndex++;
                }
                else
                    result.Append(c);
            }

            return result.ToString();
        }

        // === ПЛЕЙФЕР ===
        private string PlayfairEncrypt(string text, string key)
        {
            text = text.ToUpper().Replace('Ё', 'Е');
            key = key.ToUpper().Replace('Ё', 'Е');

            string alphabet = ContainsRussian(text) ? RU.Replace("Ё", "") : EN.Replace("J", "I");
            char[,] table = GeneratePlayfairTable(key, alphabet);
            text = PreparePlayfairText(text, alphabet);

            StringBuilder result = new StringBuilder();

            for (int i = 0; i < text.Length; i += 2)
            {
                char a = text[i];
                char b = text[i + 1];

                (int ra, int ca) = FindPosition(table, a);
                (int rb, int cb) = FindPosition(table, b);

                if (ra == rb)
                {
                    result.Append(table[ra, (ca + 1) % 5]);
                    result.Append(table[rb, (cb + 1) % 5]);
                }
                else if (ca == cb)
                {
                    result.Append(table[(ra + 1) % 5, ca]);
                    result.Append(table[(rb + 1) % 5, cb]);
                }
                else
                {
                    result.Append(table[ra, cb]);
                    result.Append(table[rb, ca]);
                }
            }

            return result.ToString();
        }

        private string PlayfairDecrypt(string text, string key)
        {
            text = text.ToUpper().Replace('Ё', 'Е');
            key = key.ToUpper().Replace('Ё', 'Е');

            string alphabet = ContainsRussian(text) ? RU.Replace("Ё", "") : EN.Replace("J", "I");
            char[,] table = GeneratePlayfairTable(key, alphabet);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < text.Length; i += 2)
            {
                char a = text[i];
                char b = text[i + 1];

                (int ra, int ca) = FindPosition(table, a);
                (int rb, int cb) = FindPosition(table, b);

                if (ra == rb)
                {
                    result.Append(table[ra, (ca + 4) % 5]);
                    result.Append(table[rb, (cb + 4) % 5]);
                }
                else if (ca == cb)
                {
                    result.Append(table[(ra + 4) % 5, ca]);
                    result.Append(table[(rb + 4) % 5, cb]);
                }
                else
                {
                    result.Append(table[ra, cb]);
                    result.Append(table[rb, ca]);
                }
            }

            return result.ToString();
        }

        // === ВСПОМОГАТЕЛЬНЫЕ ===
        private string GetAlphabet(char c)
        {
            if (EN.Contains(c)) return EN;
            if (RU.Contains(c)) return RU;
            return null;
        }

        private bool ContainsRussian(string text) =>
            text.IndexOfAny(RU.ToCharArray()) >= 0;

        private string PreparePlayfairText(string text, string alphabet)
        {
            StringBuilder result = new StringBuilder();
            foreach (char c in text)
                if (alphabet.Contains(c)) result.Append(c);

            for (int i = 0; i < result.Length - 1; i += 2)
                if (result[i] == result[i + 1])
                    result.Insert(i + 1, alphabet[0]);

            if (result.Length % 2 != 0)
                result.Append(alphabet[0]);

            return result.ToString();
        }

        private char[,] GeneratePlayfairTable(string key, string alphabet)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in key)
                if (alphabet.Contains(c) && !sb.ToString().Contains(c))
                    sb.Append(c);

            foreach (char c in alphabet)
                if (!sb.ToString().Contains(c))
                    sb.Append(c);

            char[,] table = new char[5, 5];
            for (int i = 0; i < 25; i++)
                table[i / 5, i % 5] = sb[i];

            return table;
        }

        private (int, int) FindPosition(char[,] table, char c)
        {
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 5; j++)
                    if (table[i, j] == c)
                        return (i, j);
            return (-1, -1);
        }
    }
}


