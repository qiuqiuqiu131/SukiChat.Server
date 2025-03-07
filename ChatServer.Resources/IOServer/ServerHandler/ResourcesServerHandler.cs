using ChatServer.Common;
using ChatServer.Resources.Tools;
using DotNetty.Transport.Channels;
using File.Protobuf;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Resources.IOServer.ServerHandler
{
    internal class ResourcesServerHandler : SimpleChannelInboundHandler<IMessage>
    {
        private readonly FileOperator _fileOperator;
        private readonly WeakReference<IChannel?> channel;

        public ResourcesServerHandler(FileOperator fileOperator)
        {
            _fileOperator = fileOperator;
            fileOperator.OnFileAllReceived += OnFileAllReceived;
            channel = new WeakReference<IChannel?>(null);
        }

        override public void ChannelActive(IChannelHandlerContext context)
        {
            channel.SetTarget(context.Channel);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            _fileOperator.Clear();
            base.ExceptionCaught(context, exception);
        }

        protected override async void ChannelRead0(IChannelHandlerContext context, IMessage message)
        {
            if(message.GetType() == typeof(FileHeader))
            {
                var mess = (FileHeader) message;
                await _fileOperator.ReceiveFileHeader(mess);
                FilePackResponse response = new FilePackResponse
                {
                    Success = true,
                    FileName = mess.FileName,
                    PackIndex = 0,
                    Time = mess.Time,
                    PackSize = 0
                };
                if(channel.TryGetTarget(out var cn))
                    await cn.WriteAndFlushProtobufAsync(response);
            }
            else if(message.GetType() == typeof(FilePack))
            {
                var response = _fileOperator.ReceiveFilePack((FilePack)message);
                if (channel.TryGetTarget(out var cn) && response != null)
                    await cn.WriteAndFlushProtobufAsync(response);
            }
            else if(message.GetType() == typeof(FilePackResponse))
            {
                var response = await _fileOperator.ReceiveFilePackResponse((FilePackResponse)message);
                if (response != null && channel.TryGetTarget(out var cn) && response != null)
                    await cn.WriteAndFlushProtobufAsync(response);
            }
            else if(message.GetType() == typeof(FileRequest))
            {
                var response = _fileOperator.ReceiveFileReqeust((FileRequest)message);
                if (response != null && channel.TryGetTarget(out var cn) && response != null)
                    await cn.WriteAndFlushProtobufAsync(response);
            }
        }

        private async void OnFileAllReceived((bool,string) result)
        {
            FileResponse response = new FileResponse
            {
                Success = result.Item1,
                FileName = result.Item2
            };
            
            if(channel.TryGetTarget(out IChannel? cn) && cn != null)
            {
                // Console.WriteLine($"FileAllReceived:{result.Item1},{result.Item2}");
                await cn.WriteAndFlushProtobufAsync(response);
            }
        }
    }
}
