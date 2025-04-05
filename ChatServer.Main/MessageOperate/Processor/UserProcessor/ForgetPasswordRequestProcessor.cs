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

            // 参数验证
            if (!IsValidRequest(message))
            {
                await SendErrorResponse(channel, "输入不正确");
                return;
            }

            try
            {
                // 查找并更新用户
                var user = await FindUserByCredentials(message, userRepository);
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

        private async Task<User?> FindUserByCredentials(ForgetPasswordRequest message, IRepository<User> userRepository)
        {
            // 通过ID和电话号码查找用户
            if (!string.IsNullOrWhiteSpace(message.Id) && !string.IsNullOrWhiteSpace(message.PhoneNumber))
            {
                return await userRepository.GetFirstOrDefaultAsync(
                    predicate: d => d.Id.Equals(message.Id) &&
                                    d.PhoneNumber != null &&
                                    d.PhoneNumber.Equals(message.PhoneNumber),
                    disableTracking: false);
            }

            // 通过ID和邮箱查找用户
            if (!string.IsNullOrWhiteSpace(message.Id) && !string.IsNullOrWhiteSpace(message.EmailNumber))
            {
                return await userRepository.GetFirstOrDefaultAsync(
                    predicate: d => d.Id.Equals(message.Id) &&
                                    d.EmailNumber != null &&
                                    d.EmailNumber.Equals(message.EmailNumber),
                    disableTracking: false);
            }

            // 通过电话和邮箱组合查找用户（不依赖ID）
            if (!string.IsNullOrWhiteSpace(message.PhoneNumber) && !string.IsNullOrWhiteSpace(message.EmailNumber))
            {
                return await userRepository.GetFirstOrDefaultAsync(
                    predicate: d => d.PhoneNumber != null &&
                                    d.PhoneNumber.Equals(message.PhoneNumber) &&
                                    d.EmailNumber != null &&
                                    d.EmailNumber.Equals(message.EmailNumber),
                    disableTracking: false);
            }

            return null;
        }

        private bool IsValidRequest(ForgetPasswordRequest message)
        {
            // 密码必须存在
            if (string.IsNullOrWhiteSpace(message.Password))
            {
                return false;
            }

            // 必须满足以下验证条件之一：
            // 1. ID + 电话号码
            // 2. ID + 邮箱
            // 3. 电话号码 + 邮箱
            return (!string.IsNullOrWhiteSpace(message.Id) && (!string.IsNullOrWhiteSpace(message.PhoneNumber) || !string.IsNullOrWhiteSpace(message.EmailNumber))) ||
                   (!string.IsNullOrWhiteSpace(message.PhoneNumber) && !string.IsNullOrWhiteSpace(message.EmailNumber));
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
