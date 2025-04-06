using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.GroupProcessor
{
    class ResetHeadImageRequestProcessor : IProcessor<ResetHeadImageRequest>
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IGroupService groupService;
        private readonly IClientChannelManager clientChannelManager;

        public ResetHeadImageRequestProcessor(IUserService userService,IUnitOfWork unitOfWork,IGroupService groupService,
            IClientChannelManager clientChannelManager)
        {
            this.userService = userService;
            this.unitOfWork = unitOfWork;
            this.groupService = groupService;
            this.clientChannelManager = clientChannelManager;
        }

        public async Task Process(MessageUnit<ResetHeadImageRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if(!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new ResetHeadImageResponse
                    {
                        Response = new CommonResponse { State = false, Message = "非法账号" }
                    });
                return;
            }

            try
            {
                var groupRepository = unitOfWork.GetRepository<Group>();
                var entity = await groupRepository.GetFirstOrDefaultAsync(predicate: d => d.Id.Equals(message.GroupId), disableTracking: false);
                if (entity == null)
                    throw new Exception();

                entity.IsCustomHead = true;
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if(channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new ResetHeadImageResponse
                    {
                        Response = new CommonResponse { State = false, Message = "服务器出错" }
                    });
                }
                return;
            }

            if(channel != null)
                await channel.WriteAndFlushProtobufAsync(new ResetHeadImageResponse
                {
                    Response = new CommonResponse { State = true }
                });

            var groupMember = await groupService.GetGroupMembers(message.GroupId);
            foreach (var member in groupMember)
            {
                var client = clientChannelManager.GetClient(member);
                if (client != null)
                    _ = client.WriteAndFlushProtobufAsync(new UpdateGroupMessage
                    {
                        GroupId = message.GroupId,
                        Response = new CommonResponse { State = true }
                    });
            }
        }
    }
}
