using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.Services
{
    public interface IUserService
    {
        public Task<object> GetOutlineMessage(string userId);
        public Task<bool> IsUserExist(string userId);
        public Task<User> GetUser(string userId);
    }

    public class UserService : BaseService, IUserService
    {
        private readonly IUnitOfWork unitOfWork;

        public UserService(IServiceProvider serviceProvider):base(serviceProvider)
        {
            unitOfWork = _scopedProvider.ServiceProvider.GetRequiredService<IUnitOfWork>();
        }

        /// <summary>
        /// 获取用户离线时接受到的消息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<object> GetOutlineMessage(string userId)
        {
            // 获取用户最近一次离线的时间
            var onlineRepository = unitOfWork.GetRepository<UserOnline>();
            var onlineUser = await onlineRepository.GetFirstOrDefaultAsync(
                predicate: x => x.UserId.Equals(userId),
                orderBy: x => x.OrderByDescending(d => d.LogoutTime));
            var lastLogoutTime = onlineUser?.LogoutTime ?? DateTime.MinValue;


            // 获取离线时的好友请求
            var friendRepository = unitOfWork.GetRepository<FriendRequest>();
            var friendRequests = await friendRepository.GetAllAsync(predicate: x => x.UserTargetId.Equals(userId) && x.RequestTime > lastLogoutTime);

            // 获取离线消息
            var chatPrivateRepository = unitOfWork.GetRepository<ChatPrivate>();
            var chats = await chatPrivateRepository.GetAllAsync(predicate: x => x.UserTargetId.Equals(userId) && x.Time > lastLogoutTime);

            return new MessagePackage
            {
                Chats = chats.ToList(),
                FriendRequests = friendRequests.ToList()
            };
        }

        public Task<User> GetUser(string userId)
        {
            var userRepository = unitOfWork.GetRepository<User>();  
            return userRepository.GetFirstOrDefaultAsync(predicate: d => d.Id.Equals(userId));
        }

        public Task<bool> IsUserExist(string userId)
        {
            var userRepository = unitOfWork.GetRepository<User>();
            return userRepository.ExistsAsync(d => d.Id.Equals(userId));
        }
    }

    public class MessagePackage
    {
        public List<ChatPrivate> Chats { get; set; }
        public List<FriendRequest> FriendRequests { get; set; }
    }
}
