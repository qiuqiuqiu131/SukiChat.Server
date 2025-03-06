using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File.Protobuf;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;

namespace ChatServer.Resources.Tools
{
    // 用于处理文件操作，存放读取的数据流
    internal class FileOperator
    {
        private readonly IConfigurationRoot configuration;

        private Dictionary<string, FileUnit> FileUnitDicts = new();

        public event Action<(bool, string)> OnFileAllReceived;

        const int CHUNK_SIZE = ushort.MaxValue; // 64KB per chunk

        public FileOperator(IConfigurationRoot configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// 接受到文件头
        /// </summary>
        /// <param name="fileHeader"></param>
        public void ReceiveFileHeader(FileHeader fileHeader)
        {
            if (FileUnitDicts.ContainsKey(fileHeader.Time)) return;

            var fileUnit = new FileUnit(fileHeader);
            string path = Path.Combine(configuration["ResourcesPath"]!, fileHeader.Path, fileHeader.FileName);
            FileInfo fileInfo = new FileInfo(path);
            if(!fileInfo.Exists)
                Directory.CreateDirectory(fileInfo.DirectoryName!);
            fileUnit.fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

            FileUnitDicts.Add(fileHeader.Time, fileUnit);
        }

        /// <summary>
        /// 接受到文件包
        /// </summary>
        /// <param name="filePack"></param>
        public FilePackResponse? ReceiveFilePack(FilePack filePack)
        {
            if (!FileUnitDicts.ContainsKey(filePack.Time))
            {
                FilePackResponse response = new FilePackResponse
                {
                    FileName = filePack.FileName,
                    PackIndex = filePack.PackIndex,
                    PackSize = filePack.PackSize,
                    Time = filePack.Time,
                    Success = false
                };
                return response;
            }

            var fileUnit = FileUnitDicts[filePack.Time];
            bool result = true;
            try
            {
                // 验证文件接收顺序完整性
                if (filePack.PackIndex != fileUnit.CurrentIndex + 1)
                    result = false;
                else
                {
                    if(fileUnit.fileStream == null) result = false;
                    else
                    {
                        fileUnit.fileStream.Write(filePack.Data.ToByteArray(), 0, filePack.PackSize);
                        fileUnit.fileStream.Flush();
                        fileUnit.CurrentIndex = filePack.PackIndex;
                    }
                }
            }
            catch
            {
                result = false;
            }

            if(result && fileUnit.CurrentIndex == fileUnit.TotleCount)
            {
                string path = Path.Combine(configuration["ResourcesPath"]!, fileUnit.Path, fileUnit.FileName);
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Length == fileUnit.TotleSize)
                {
                    OnFileAllReceived?.Invoke((true, fileUnit.FileName));

                    fileUnit.fileStream?.Dispose();
                    fileUnit.fileStream = null;
                }
                else
                {
                    OnFileAllReceived?.Invoke((false, fileUnit.FileName));
                    
                    fileUnit.fileStream?.Dispose();
                    fileUnit.fileStream = null;
                    System.IO.File.Delete(path);
                }

                return null;
            }
            else
            {
                if(!result)
                {
                    string path = Path.Combine(configuration["ResourcesPath"]!, fileUnit.Path, fileUnit.FileName);

                    fileUnit.fileStream?.Dispose();
                    fileUnit.fileStream = null;
                    System.IO.File.Delete(path);
                }

                // 拼接响应
                FilePackResponse response = new FilePackResponse
                {
                    FileName = filePack.FileName,
                    PackIndex = filePack.PackIndex,
                    PackSize = filePack.PackSize,
                    Time = filePack.Time,
                    Success = result
                };
                return response;
            }
        }

        /// <summary>
        /// 接收到文件请求
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <returns></returns>
        public FileHeader? ReceiveFileReqeust(FileRequest fileRequest)
        {
            string path = Path.Combine(configuration["ResourcesPath"]!, fileRequest.Path, fileRequest.FileName);
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                return new FileHeader { Exist = false };

            FileUnit fileUnit = new FileUnit(fileRequest.Path, fileRequest.FileName, fileRequest.Type);
            fileUnit.TotleSize = (int)fileInfo.Length;
            fileUnit.TotleCount = (int)Math.Ceiling((double)fileInfo.Length / CHUNK_SIZE);
            fileUnit.fileStream = new FileStream(path,FileMode.Open,FileAccess.Read);
            fileUnit.Time = DateTime.Now.ToString("yyyyMMddHHmmss");
            fileUnit.CurrentIndex = 0;

            FileUnitDicts.Add(fileUnit.Time, fileUnit);

            FileHeader fileHeader = new FileHeader
            {
                FileName = fileRequest.FileName,
                Exist = true,
                Path = fileRequest.Path,
                Time = fileUnit.Time,
                Type = fileUnit.Type,
                TotleCount = fileUnit.TotleCount,
                TotleSize = fileUnit.TotleSize
            };

            return fileHeader;
        }
    
        /// <summary>
        /// 接收到文件分片响应
        /// </summary>
        /// <param name="filePack"></param>
        public async Task<FilePack?> ReceiveFilePackResponse(FilePackResponse filePackResponse)
        {
            if(!FileUnitDicts.TryGetValue(filePackResponse.Time, out FileUnit fileUnit))
                return null;
            
            if(!filePackResponse.Success || fileUnit.CurrentIndex != filePackResponse.PackIndex || fileUnit.fileStream == null)
            {
                fileUnit.fileStream?.Dispose();
                fileUnit.fileStream = null;
                FileUnitDicts.Remove(fileUnit.Time);
                return null;
            }

            byte[] buffer = new byte[CHUNK_SIZE];
            int bytesRead = await fileUnit.fileStream.ReadAsync(buffer, 0, CHUNK_SIZE);

            FilePack filePack = new FilePack
            {
                PackIndex = ++fileUnit.CurrentIndex,
                FileName = filePackResponse.FileName,
                PackSize = bytesRead,
                Data = ByteString.CopyFrom(buffer,0,bytesRead),
                Time = filePackResponse.Time
            };

            Array.Clear(buffer);

            if(filePack.PackIndex == fileUnit.TotleCount)
            {
                fileUnit.fileStream?.Dispose();
                fileUnit.fileStream = null;
                FileUnitDicts.Remove(fileUnit.Time);
            }

            return filePack;
        }

        public void Clear()
        {
            foreach(var unit in FileUnitDicts.Values)
            {
                if(unit.fileStream != null)
                    unit.fileStream.Dispose();
                unit.fileStream = null;
            }
            FileUnitDicts.Clear();
        }
    }
}
