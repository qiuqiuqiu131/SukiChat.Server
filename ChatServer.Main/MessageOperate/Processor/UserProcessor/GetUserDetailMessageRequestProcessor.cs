using AutoMapper;
using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor
{
    class GetUserDetailMessageRequestProcessor : IProcessor<GetUserDetailMessageRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICipherHelper cipherHelper;
        private readonly IMapper mapper;

        public GetUserDetailMessageRequestProcessor(IUnitOfWork unitOfWork,
            ICipherHelper cipherHelper,
            IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.cipherHelper = cipherHelper;
            this.mapper = mapper;
        }

        public async Task Process(MessageUnit<GetUserDetailMessageRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            var message = unit.Message;

            UserDetailMessage? messageDetail = null;

            try
            {
                var userRepository = unitOfWork.GetRepository<User>();

                // 解析密码
                var encryptPassword = cipherHelper.Encrypt(message.Password);
                var user = await userRepository.GetFirstOrDefaultAsync(predicate: d => d.Id.Equals(message.Id) && d.Password.Equals(encryptPassword));

                if (user == null)
                {
                    if (channel != null)
                        await channel.WriteAndFlushProtobufAsync(new GetUserDetailMessageResponse
                        {
                            Response = new CommonResponse { State = false }
                        });
                    return;
                }

                messageDetail = mapper.Map<UserDetailMessage>(user);
                messageDetail.Password = message.Password;

                var loginRepository = unitOfWork.GetRepository<UserOnline>();
                var online = await loginRepository.GetFirstOrDefaultAsync(predicate:d => d.UserId.Equals(user.Id));
                if (online == null)
                    messageDetail.IsFirstLogin = true;
            }
            catch
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetUserDetailMessageResponse
                    {
                        Response = new CommonResponse { State = false }
                    });
                return;
            }

            if (messageDetail == null)
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetUserDetailMessageResponse
                    {
                        Response = new CommonResponse { State = false }
                    });
            }
            else
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new GetUserDetailMessageResponse
                    {
                        Response = new CommonResponse { State = true },
                        User = messageDetail
                    });
            }
        }
    }
}
