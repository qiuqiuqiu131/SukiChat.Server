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
    class SearchUserRequestProcessor : IProcessor<SearchUserRequest>
    {
        private readonly IUserService userService;
        private readonly IUnitOfWork unitOfWork;

        public SearchUserRequestProcessor(IUserService userService, IUnitOfWork unitOfWork)
        {
            this.userService = userService;
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<SearchUserRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new SearchUserResponse { Response = new CommonResponse { State = false } });
                return;
            }

            var userRepository = unitOfWork.GetRepository<User>();
            var searchContentLower = message.Content.ToLower();

            // 获取所有用户进行内存过滤(包含拼音匹配)
            var allUsers = await userRepository.GetAll()
                .Select(d => new { d.Id, d.Name, d.Introduction })
                .ToListAsync();

            var ids = allUsers.Where(d =>
            {
                // 原有的直接匹配逻辑
                if (d.Id.Equals(message.Content)) return true;
                if (d.Name.ToLower().Equals(searchContentLower)) return true;
                if (d.Name.ToLower().Contains(searchContentLower) && message.Content.Length >= 2) return true;
                if (d.Introduction != null && d.Introduction.Contains(message.Content) && message.Content.Length >= 2) return true;
                if (d.Introduction != null && d.Introduction.Equals(message.Content)) return true;

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

            var response = new SearchUserResponse { Response = new CommonResponse { State = true } };
            response.Ids.AddRange(ids);

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(response);
        }
    }
}
