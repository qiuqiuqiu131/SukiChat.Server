using ChatServer.Common;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using ChatServer.Main.Services;
using ChatServer.Main.Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor
{
    class ResetPasswordRequestProcessor : IProcessor<ResetPasswordRequest>
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IUserService userService;
        private readonly ICipherHelper cipherHelper;

        public ResetPasswordRequestProcessor(IUnitOfWork unitOfWork,
            IUserService userService,
            ICipherHelper cipherHelper)
        {
            this.unitOfWork = unitOfWork;
            this.userService = userService;
            this.cipherHelper = cipherHelper;
        }

        public async Task Process(MessageUnit<ResetPasswordRequest> unit)
        {
            unit.Channel.TryGetTarget(out var channel);

            var message = unit.Message;

            if (!await userService.IsUserExist(message.Id))
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new ResetPasswordResponse
                    {
                        Response = new CommonResponse { State = false, Message = "非法账号" }
                    });
                return;
            }

            try
            {
                var encryptPassword = cipherHelper.Encrypt(message.OrigionalPassword);
                var userRepositry = unitOfWork.GetRepository<User>();
                var entity = await userRepositry.GetFirstOrDefaultAsync(predicate:d => d.Id.Equals(message.Id) && d.Password.Equals(encryptPassword),disableTracking:false);

                if(entity == null)
                {
                    if (channel != null)
                        await channel.WriteAndFlushProtobufAsync(new ResetPasswordResponse
                        {
                            Response = new CommonResponse { State = false, Message = "原始密码不正确" }
                        });
                    return;
                }

                entity.Password = cipherHelper.Encrypt(message.NewPassword);

                await unitOfWork.SaveChangesAsync();
            }
            catch
            {
                if (channel != null)
                    await channel.WriteAndFlushProtobufAsync(new ResetPasswordResponse
                    {
                        Response = new CommonResponse { State = false, Message = "服务器出错" }
                    });
                return;
            }

            // 密码更新成功
            if (channel != null)
                await channel.WriteAndFlushProtobufAsync(new ResetPasswordResponse
                {
                    Response = new CommonResponse { State = true }
                });
        }
    }
}
