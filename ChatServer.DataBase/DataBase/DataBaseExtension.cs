using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.Repository;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.DataBase.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatServer.DataBase.DataBase;

public static class DataBaseExtension
{
    public static void AddDataBaseServices(this IServiceCollection services)
    {
        // 添加数据库上下文
        services.AddDbContext<ChatServerDbContext>(options =>
        {
            var connectionString = DbSettings.Configuration.GetConnectionString("ChatDbConnection");
            var serverVersion = ServerVersion.AutoDetect(connectionString);
            options.UseMySql(connectionString, serverVersion);
        });

        // 添加工作单元
        services.AddUnitOfWork<ChatServerDbContext>();

        // 添加自定义仓储
        services.AddCustomRepository<User, UserRepository>();
        services.AddCustomRepository<UserOnline, UserOnlineRepository>();
        services.AddCustomRepository<SacurityQuestion, SacurityQuestionRepository>();
        services.AddCustomRepository<UserGroup, UserGroupRepository>();

        services.AddCustomRepository<ChatPrivate, ChatPrivateRepository>();
        services.AddCustomRepository<ChatPrivateDetail, ChatPrivateDetailRepository>();
        services.AddCustomRepository<FriendRelation, FriendRelationRepository>();
        services.AddCustomRepository<FriendRequest, FriendRequestRepository>();
        services.AddCustomRepository<FriendDelete, FriendDeleteRepository>();

        services.AddCustomRepository<ChatGroup, ChatGroupRepository>();
        services.AddCustomRepository<ChatGroupDetail, ChatGroupDetailRepository>();
        services.AddCustomRepository<GroupRelation, GroupRelationRepository>();
        services.AddCustomRepository<GroupRequest, GroupRequestRepository>();
        services.AddCustomRepository<Group, GroupRepository>();
        services.AddCustomRepository<GroupDelete, GroupDeleteRepository>();
    }
}
