using System;

namespace Shifr.WorkFile.Data
{
    public class DataRecord
    {
        public string Path { get; set; }
        public string File { get; set; }
        public DateTime Date { get; set; }
        public Shifr.WorkFile.CipherType CipherType { get; set; }
        public string Key { get; set; }
        public bool IsEncryption { get; set; }
    }
}
