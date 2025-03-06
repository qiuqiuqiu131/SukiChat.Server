using File.Protobuf;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Resources.Tools
{
    internal class FileUnit
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public int CurrentIndex { get; set; }
        public int TotleSize { get; set; }
        public int TotleCount { get; set; }
        public string Time { get; set; }
        public FileStream? fileStream { get; set; }

        public FileUnit(string path,string fileName,string type)
        {
            Path = path;
            FileName = fileName;
            Type = type;
            Time = DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public FileUnit(FileHeader fileHeader)
        {
            Path = fileHeader.Path;
            FileName = fileHeader.FileName;
            Type = fileHeader.Type;
            TotleSize = fileHeader.TotleSize;
            TotleCount = fileHeader.TotleCount;
            Time = fileHeader.Time;
        }
    }
}
