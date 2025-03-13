using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace ChatServer.Main.Services;

public interface IGroupService
{
    /// <summary>
    /// 获取群组成员ID列表
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <returns>成员ID列表</returns>
    Task<List<string>> GetGroupMembers(string groupId);

    Task<List<string>> GetGroupManagers(string groupId); 

    /// <summary>
    /// 判断用户是否为群组成员
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="groupId">群组ID</param>
    /// <returns>是否为成员</returns>
    Task<bool> IsGroupMember(string userId, string groupId);

    /// <summary>
    /// 检查群组是否存在
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <returns>是否存在</returns>
    Task<bool> IsGroupExist(string groupId);

    /// <summary>
    /// 判断用户是否为群组管理员
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="groupId">群组ID</param>
    /// <returns>是否为成员</returns>
    Task<bool> IsGroupManager(string userId, string groupId);

    /// <summary>
    /// 获取用户在群组中最后一次发送消息的时间
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    Task<DateTime> MemberLastSpeakTime(string userId, string groupId);

    /// <summary>
    /// 获取某个用户加入的所有群聊ID
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<string>> GetGroupsOfUser(string userId);

    /// <summary>
    /// 获取某个用户加入的所有为管理员或者群主的群聊ID
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<string>> GetGroupsOfManager(string userId);
}

public class GroupService : BaseService,IGroupService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger logger;

    public GroupService(IServiceProvider serviceProvider, ILogger logger) : base(serviceProvider)
    {
        unitOfWork = _scopedProvider.ServiceProvider.GetRequiredService<IUnitOfWork>();
        this.logger = logger;
    }

    /// <summary>
    /// 获取群组成员ID列表
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <returns>成员ID列表</returns>
    public async Task<List<string>> GetGroupMembers(string groupId)
    {
        try
        {
            var repository = unitOfWork.GetRepository<GroupRelation>();
            var members = await repository.GetAll()
                .Where(gu => gu.GroupId == groupId)
                .Select(gu => gu.UserId)
                .ToListAsync();

            return members;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "获取群组成员列表失败: {GroupId}", groupId);
            return new List<string>();
        }
    }

    /// <summary>
    /// 获取群组管理员和群主ID
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<List<string>> GetGroupManagers(string groupId)
    {
        try
        {
            var repository = unitOfWork.GetRepository<GroupRelation>();
            var members = await repository.GetAll()
                .Where(gu => gu.GroupId == groupId && (gu.Status == 0 || gu.Status == 1))
                .Select(gu => gu.UserId)
                .ToListAsync();

            return members;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "获取群组成员列表失败: {GroupId}", groupId);
            return new List<string>();
        }
    }

    /// <summary>
    /// 判断用户是否为群组成员
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="groupId">群组ID</param>
    /// <returns>是否为成员</returns>
    public async Task<bool> IsGroupMember(string userId, string groupId)
    {
        try
        {
            var repository = unitOfWork.GetRepository<GroupRelation>();
            return await repository.ExistsAsync(d => d.UserId.Equals(userId) && d.GroupId.Equals(groupId));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "检查用户是否为群组成员失败: {UserId}, {GroupId}", userId, groupId);
            return false;
        }
    }

    /// <summary>
    /// 检查群组是否存在
    /// </summary>
    /// <param name="groupId">群组ID</param>
    /// <returns>是否存在</returns>
    public async Task<bool> IsGroupExist(string groupId)
    {
        try
        {
            var repository = unitOfWork.GetRepository<Group>();
            return await repository.ExistsAsync(d => d.Id.Equals(groupId));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "检查群组是否存在失败: {GroupId}", groupId);
            return false;
        }
    }

    /// <summary>
    /// 判断用户是否为群组管理员
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="groupId">群组ID</param>
    /// <returns>是否为成员</returns>
    public async Task<bool> IsGroupManager(string userId, string groupId)
    {
        try
        {
            var repository = unitOfWork.GetRepository<GroupRelation>();
            return await repository.ExistsAsync(d => d.UserId.Equals(userId) && d.GroupId.Equals(groupId) && (d.Status == 0 || d.Status == 1));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "检查用户是否为群组成员失败: {UserId}, {GroupId}", userId, groupId);
            return false;
        }
    }

    /// <summary>
    /// 获取用户在群组中最后一次发送消息的时间
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public async Task<DateTime> MemberLastSpeakTime(string userId, string groupId)
    {
        try
        {
            var repository = unitOfWork.GetRepository<ChatGroup>();
            var time = await repository.GetFirstOrDefaultAsync(
                predicate:d => d.GroupId.Equals(groupId) && d.UserFromId.Equals(groupId),
                orderBy: o => o.OrderByDescending(d => d.Time));
            if(time != null) 
                return time.Time;
            else 
                return DateTime.MinValue;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// 获取某个用户加入的所有群聊ID
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<string>> GetGroupsOfUser(string userId)
    {
        try
        {
            var repository = unitOfWork.GetRepository<GroupRelation>();
            var result = await repository.GetAll(predicate: d => d.UserId.Equals(userId))
                .Select(d => d.GroupId).ToListAsync();
            return result;
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// 获取某个用户加入的所有为管理员或者群主的群聊ID
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<string>> GetGroupsOfManager(string userId)
    {
        try
        {
            var repository = unitOfWork.GetRepository<GroupRelation>();
            var result = await repository.GetAll(
                predicate: d => d.UserId.Equals(userId) && (d.Status == 0 || d.Status == 1))
                .Select(d => d.GroupId).ToListAsync();
            return result;
        }
        catch
        {
            return new List<string>();
        }
    }
}
