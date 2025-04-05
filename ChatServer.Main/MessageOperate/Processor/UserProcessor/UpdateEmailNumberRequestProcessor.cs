using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services.Helper;
using ChatServer.Main.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatServer.Common;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor
{
    class UpdateEmailNumberRequestProcessor : IProcessor<UpdateEmailNumberRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICipherHelper cipherHelper;
        private readonly IUserService userService;

        public UpdateEmailNumberRequestProcessor(IUnitOfWork unitOfWork,
            ICipherHelper cipherHelper,
            IUserService userService)
        {
            this.unitOfWork = unitOfWork;
            this.cipherHelper = cipherHelper;
            this.userService = userService;
        }

        public async Task Process(MessageUnit<UpdateEmailNumberRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if (!await userService.IsUserExist(message.UserId))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateEmailNumberResponse
                    {
                        Response = new CommonResponse { State = false, Message = "非法账号" }
                    });
                return;
            }

            try
            {
                var encrptPassword = cipherHelper.Encrypt(message.Password);
                var userRepository = unitOfWork.GetRepository<User>();

                // 验证密码是否正确
                var entity = await userRepository.GetFirstOrDefaultAsync(
                    predicate: d => d.Id.Equals(message.UserId) && d.Password.Equals(encrptPassword), disableTracking: false);
                if (entity == null)
                {
                    if (channel != null)
                        await channel.WriteAndFlushProtobufAsync(new UpdateEmailNumberResponse
                        {
                            Response = new CommonResponse { State = false, Message = "密码错误" }
                        });
                    return;
                }

                if (!string.IsNullOrWhiteSpace(message.EmailNumber))
                {
                    // 查看是否存在相同的号码
                    var entityPhone = await userRepository.GetFirstOrDefaultAsync(predicate: d => d.EmailNumber != null && d.EmailNumber.Equals(message.EmailNumber));
                    if (entityPhone != null)
                    {
                        if (channel != null)
                            await channel.WriteAndFlushProtobufAsync(new UpdateEmailNumberResponse
                            {
                                Response = new CommonResponse { State = false, Message = "手机号已被注册" }
                            });
                        return;
                    }
                }

                entity.EmailNumber = string.IsNullOrWhiteSpace(message.EmailNumber) ? null : message.EmailNumber;

                await unitOfWork.SaveChangesAsync();
            }
            catch
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new UpdateEmailNumberResponse
                    {
                        Response = new CommonResponse
                        {
                            State = false,
                            Message = "服务器出错"
                        }
                    });
                return;
            }

            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new UpdateEmailNumberResponse
                {
                    Response = new CommonResponse { State = true },
                    UserId = message.UserId
                });
        }
    }
}
