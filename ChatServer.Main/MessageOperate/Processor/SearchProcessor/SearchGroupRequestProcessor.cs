using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolGood.Words.Pinyin;

namespace ChatServer.Main.MessageOperate.Processor.SearchProcessor
{
    class SearchGroupRequestProcessor : IProcessor<SearchGroupRequest>
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;

        public SearchGroupRequestProcessor(IUserService userService, IUnitOfWork unitOfWork)
        {
            this.userService = userService;
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<SearchGroupRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new SearchGroupResponse { Response = new CommonResponse { State = false } });
                return;
            }

            var groupRepository = unitOfWork.GetRepository<Group>();
            var searchContentLower = message.Content.ToLower();

            // 获取所有未解散的群组进行内存过滤(包含拼音匹配)
            var allGroups = await groupRepository.GetAll()
                .Where(d => d.IsDisband == false)
                .Select(d => new { d.Id, d.Name, d.Description })
                .ToListAsync();

            var ids = allGroups.Where(d =>
            {
                // 原有的直接匹配逻辑
                if (d.Id.Equals(message.Content)) return true;
                if (d.Name.ToLower().Equals(searchContentLower)) return true;
                if (d.Name.ToLower().Contains(searchContentLower) && message.Content.Length >= 2) return true;
                if (d.Description != null && d.Description.Contains(message.Content) && message.Content.Length >= 2) return true;
                if (d.Description != null && d.Description.Equals(message.Content)) return true;

                // 拼音匹配逻辑
                if (message.Content.Length >= 2)
                {
                    var namePinyin = WordsHelper.GetPinyin(d.Name).ToLower().Replace(" ", "");
                    var nameFirstPinyin = WordsHelper.GetFirstPinyin(d.Name).ToLower();

                    // 全拼匹配
                    if (namePinyin.Contains(searchContentLower)) return true;
                    // 首字母匹配
                    if (nameFirstPinyin.Contains(searchContentLower)) return true;
                }

                return false;
            }).Select(d => d.Id).ToList();

            var response = new SearchGroupResponse { Response = new CommonResponse { State = true } };
            response.Ids.AddRange(ids);

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
