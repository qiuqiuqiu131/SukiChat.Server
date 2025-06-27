using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor
{
    class PasswordAuthenticateRequestProcessor : IProcessor<PasswordAuthenticateRequest>
    {
        private readonly IUnitOfWork unitOfWork;

        public PasswordAuthenticateRequestProcessor(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<PasswordAuthenticateRequest> unit)
        {
            var message = unit.Message;
            unit.Channel.TryGetTarget(out var channel);

            try
            {
                var userRepository = unitOfWork.GetRepository<User>();
                var user = await userRepository.GetFirstOrDefaultAsync(
                    predicate: d => d.EmailNumber == message.Email && d.PhoneNumber == message.Phone);
                if(user != null)
                {
                    var result = new PasswordAuthenticateResponse
                    {
                        Response = new CommonResponse { State = true },
                        PassKey = user.Password,
                        UserId = user.Id
                    };
                    if (channel != null)
                    {
                        await channel.WriteAndFlushProtobufAsync(result);
                    }
                }
                else
                {
                    if (channel != null)
                        await channel.WriteAndFlushProtobufAsync(new PasswordAuthenticateResponse
                        {
                            Response = new CommonResponse { State = false, Message = "用户身份验证失败" }
                        });
                }
            }
            catch
            {
                if (channel != null)
                {
                    await channel.WriteAndFlushProtobufAsync(new PasswordAuthenticateResponse
                    {
                        Response = new CommonResponse { State = false, Message = "服务器错误" }
                    });
                }
            }
        }
    }
}
