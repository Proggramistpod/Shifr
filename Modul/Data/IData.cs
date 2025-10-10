using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shifr.WorkFile.Data
{
    internal class IData
    {
        public string Path { get; set; }
        public string File { get; set; }
        public DateTime Date { get; set; }
        public bool IsEncryption { get; set; }

        public IData(string path, string file, DateTime date)
        {
            Path = path;
            File = file;
            Date = date;
            IsEncryption = true;
        }
    }
}
