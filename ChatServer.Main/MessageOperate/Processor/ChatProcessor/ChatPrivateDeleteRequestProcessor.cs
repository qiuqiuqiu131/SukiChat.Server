using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class ChatPrivateDeleteRequestProcessor : IProcessor<ChatPrivateDeleteRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;

        public ChatPrivateDeleteRequestProcessor(IUnitOfWork unitOfWork,IUserService userService)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
        }

        public async Task Process(MessageUnit<ChatPrivateDeleteRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel); 
            var message = unit.Message;

            if(! await userService.IsUserExist(message.UserId))
            {
                if(channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatPrivateDeleteResponse
                    {
                        Response = new CommonResponse { State = false, Message = "非法账号" }
                    });
                }
                return;
            }

            try
            {
                var chatPrivateDetailRepository = unitOfWork.GetRepository<ChatPrivateDetail>();
                var chatPrivateDetail = await chatPrivateDetailRepository.GetFirstOrDefaultAsync(predicate: d => d.UserId.Equals(message.UserId) && d.ChatPrivateId.Equals(message.ChatPrivateId), disableTracking: false);
                if (chatPrivateDetail != null)
                {
                    chatPrivateDetail.Time = DateTime.Now;
                    chatPrivateDetail.IsDeleted = true;
                }
                else
                {
                    var entity = new ChatPrivateDetail
                    {
                        UserId = message.UserId,
                        ChatPrivateId = message.ChatPrivateId,
                        IsDeleted = true,
                        Time = DateTime.Now
                    };
                    await chatPrivateDetailRepository.InsertAsync(entity);
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatPrivateDeleteResponse
                    {
                        Response = new CommonResponse { State = false, Message = "服务器出错" }
                    });
                }
                return;
            }

            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new ChatPrivateDeleteResponse
                {
                    Response = new CommonResponse { State = true }
                });
            }
        }
    }
}
