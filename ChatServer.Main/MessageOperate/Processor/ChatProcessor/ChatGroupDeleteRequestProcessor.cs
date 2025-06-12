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
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.ChatProcessor
{
    class ChatGroupDeleteRequestProcessor(IUnitOfWork unitOfWork, IUserService userService) : IProcessor<ChatGroupDeleteRequest>
    {
        public async Task Process(MessageUnit<ChatGroupDeleteRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if(!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatGroupDeleteResponse
                    {
                        Response = new CommonResponse { State = false, Message = "非法账号" }
                    });
                }
                return;
            }

            try
            {
                var chatGroupDetailRepository = unitOfWork.GetRepository<ChatGroupDetail>();
                var chatGroupDetail = await chatGroupDetailRepository.GetFirstOrDefaultAsync(predicate:d => d.UserId.Equals(message.UserId) && d.ChatGroupId.Equals(message.ChatGroupId),disableTracking:false);
                if (chatGroupDetail != null)
                {
                    chatGroupDetail.Time = DateTime.Now;
                    chatGroupDetail.IsDeleted = true;
                }
                else
                {
                    var entity = new ChatGroupDetail
                    {
                        UserId = message.UserId,
                        ChatGroupId = message.ChatGroupId,
                        IsDeleted = true,
                        Time = DateTime.Now
                    };
                    await chatGroupDetailRepository.InsertAsync(entity);
                }

                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if(channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ChatGroupDeleteResponse
                    {
                        Response = new CommonResponse { State = false, Message = "服务器出错" }
                    });
                }
                return;
            }

            if (channel != null)
            {
                await channel.WriteAndFlushProtobufAsync(new ChatGroupDeleteResponse
                {
                    Response = new CommonResponse { State = true }
                });
            }
        }
    }
}
