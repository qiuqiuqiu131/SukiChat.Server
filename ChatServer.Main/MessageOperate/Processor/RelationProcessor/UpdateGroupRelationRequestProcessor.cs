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

namespace ChatServer.Main.MessageOperate.Processor.RelationProcessor
{
    internal class UpdateGroupRelationRequestProcessor : IProcessor<UpdateGroupRelationRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IGroupService groupService;
        private readonly IClientChannelManager clientChannelManager;

        public UpdateGroupRelationRequestProcessor(IUnitOfWork unitOfWork,
            IGroupService groupService,
            IClientChannelManager clientChannelManager)
        {
            this.unitOfWork = unitOfWork;
            this.groupService = groupService;
            this.clientChannelManager = clientChannelManager;
        }

        public async Task Process(MessageUnit<UpdateGroupRelationRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            var groupRelationRepository = unitOfWork.GetRepository<GroupRelation>();
            var groupRelation = await groupRelationRepository.GetFirstOrDefaultAsync(
                predicate: d => d.UserId.Equals(message.UserId) && d.GroupId.Equals(message.GroupId), disableTracking: false);

            if (groupRelation == null)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateGroupRelation
                    {
                        Response = new CommonResponse { State = false }
                    });
                return;
            }

            groupRelation.Remark = string.IsNullOrEmpty(message.Remark) ? null : message.Remark;
            groupRelation.NickName = string.IsNullOrEmpty(message.NickName) ? null : message.NickName;
            groupRelation.CantDisturb = message.CantDisturb;
            groupRelation.IsTop = message.IsTop;
            groupRelation.Grouping = message.Grouping;
            groupRelation.IsChatting = message.IsChatting;

            try
            {
                await unitOfWork.SaveChangesAsync();
            }
            catch
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateGroupRelation
                    {
                        Response = new CommonResponse { State = false }
                    });
                return;
            }

            var response = new UpdateGroupRelation
            {
                Response = new CommonResponse { State = true },
                UserId = message.UserId,
                GroupId = message.GroupId
            };
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(response);

            var memberIds = await groupService.GetGroupMembers(message.GroupId);
            foreach (var memberId in memberIds)
            {
                if (memberId == message.UserId)
                    continue;
                var memberChannel = clientChannelManager.GetClient(memberId);
                if (memberChannel != null)
                    await memberChannel.WriteAndFlushProtobufAsync(response);
            }
        }
    }
}
