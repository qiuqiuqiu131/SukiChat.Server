using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using ChatServer.Main.Services.Helper;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor
{
    class ForgetPasswordRequestProcessor : IProcessor<ForgetPasswordRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly ICipherHelper cipherHelper;

        public ForgetPasswordRequestProcessor(IUnitOfWork unitOfWork,IUserService userService,ICipherHelper cipherHelper)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.cipherHelper = cipherHelper;
        }

        public async Task Process(MessageUnit<ForgetPasswordRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);
            if (channel == null)
            {
                return; // 无法获取通信通道，无法响应客户端
            }

            var message = unit.Message;
            var userRepository = unitOfWork.GetRepository<User>();

            try
            {
                var user = await userRepository.GetFirstOrDefaultAsync(predicate: d => d.Id == message.UserId && d.Password == message.PassKey,disableTracking:false);

                if (user == null)
                {
                    await SendErrorResponse(channel, "无法匹配账号");
                    return;
                }

                // 更新密码
                var encryptedPassword = cipherHelper.Encrypt(message.Password);
                user.Password = encryptedPassword;
                await unitOfWork.SaveChangesAsync();

                // 发送成功响应
                await channel.WriteAndFlushProtobufAsync(new ForgetPasswordResponse
                {
                    Response = new CommonResponse { State = true },
                    UserId = user.Id
                });
            }
            catch (Exception ex)
            {
                await SendErrorResponse(channel, "服务器错误");
            }
        }

        private async Task SendErrorResponse(IChannel channel, string errorMessage)
        {
            await channel.WriteAndFlushProtobufAsync(new ForgetPasswordResponse
            {
                Response = new CommonResponse
                {
                    State = false,
                    Message = errorMessage
                }
            });
        }
    }
}
