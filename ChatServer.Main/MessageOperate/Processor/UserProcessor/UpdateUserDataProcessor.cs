using AutoMapper;
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

namespace ChatServer.Main.MessageOperate.Processor.UserProcessor;

/// <summary>
/// 处理目标：
/// UpdateUserData 更新用户信息请求
/// 
/// 数据库操作：
/// 1、User 更新用户信息
/// </summary>
public class UpdateUserDataProcessor : IProcessor<UpdateUserData>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly ICipherHelper cipherHelper;

    public UpdateUserDataProcessor(IUnitOfWork unitOfWork, IMapper mapper, ICipherHelper cipherHelper)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.cipherHelper = cipherHelper;
    }

    public async Task Process(MessageUnit<UpdateUserData> unit)
    {
        var repository = unitOfWork.GetRepository<User>();
        UserMessage userMess = unit.Message.User;
        User user = mapper.Map<User>(userMess);
        user.Password = cipherHelper.Encrypt(userMess.Password);
        repository.Update(user);
        await unitOfWork.SaveChangesAsync();
        unitOfWork.Dispose();
    }
}
