using ChatServer.Common;
using ChatServer.Main.Entity;
using File.Protobuf;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class FileRequestProcessor : IProcessor<FileRequest>
    {
        private readonly IConfigurationRoot configuration;

        public FileRequestProcessor(IConfigurationRoot configuration)
        {
            this.configuration = configuration;
        }

        public async Task Process(MessageUnit<FileRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            string path = Path.Combine(configuration["ResourcesPath"]!, message.Path, message.FileName);
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new FileHeader
                    {
                        Exist = false,
                        FileName = message.FileName,
                        Path = message.Path
                    });
                }
            }
            else
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new FileHeader
                    {
                        Exist = true,
                        FileName = message.FileName,
                        Path = message.Path,
                        TotleSize = (int)fileInfo.Length
                    });
                }
            }
        }
    }
}
