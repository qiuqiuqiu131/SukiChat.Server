using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.Main.Manager
{
    /// <summary>
    /// ID生成器服务接口
    /// </summary>
    public interface IIdGeneratorManager
    {
        /// <summary>
        /// 生成用户ID
        /// </summary>
        /// <returns>新的用户ID</returns>
        string GenerateUserId();

        /// <summary>
        /// 生成群组ID
        /// </summary>
        /// <returns>新的群组ID</returns>
        string GenerateGroupId();
    }

    /// <summary>
    /// ID生成器服务实现
    /// </summary>
    public class IdGeneratorManager : IIdGeneratorManager
    {
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly HashSet<string> _usedUserIds = new HashSet<string>();
        private readonly HashSet<string> _usedGroupIds = new HashSet<string>();
        
        private readonly object _userIdLock = new object();
        private readonly object _groupIdLock = new object();
        private readonly Random _random = new Random();

        private const int UserIdLength = 10;
        private const int GroupIdLength = 10;
        private const string UserIdPrefix = "";
        private const string GroupIdPrefix = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitOfWork">工作单元</param>
        public IdGeneratorManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            InitializeFromDatabaseAsync().Wait();
        }

        /// <summary>
        /// 生成用户ID
        /// </summary>
        /// <returns>新的用户ID</returns>
        public string GenerateUserId()
        {
            lock (_userIdLock)
            {
                string userId;
                do
                {
                    string numericPart = GenerateRandomNumericString(UserIdLength - UserIdPrefix.Length);
                    userId = $"{UserIdPrefix}{numericPart}";
                } while (_usedUserIds.Contains(userId));

                _usedUserIds.Add(userId);
                return userId;
            }
        }

        /// <summary>
        /// 生成群组ID
        /// </summary>
        /// <returns>新的群组ID</returns>
        public string GenerateGroupId()
        {
            lock (_groupIdLock)
            {
                string groupId;
                do
                {
                    string numericPart = GenerateRandomNumericString(GroupIdLength - GroupIdPrefix.Length);
                    groupId = $"{GroupIdPrefix}{numericPart}";
                } while (_usedGroupIds.Contains(groupId));

                _usedGroupIds.Add(groupId);
                return groupId;
            }
        }

        /// <summary>
        /// 从数据库初始化已使用的ID集合
        /// </summary>
        private async Task InitializeFromDatabaseAsync()
        {
            // 加载所有用户ID
            var userRepository = _unitOfWork.GetRepository<User>();
            var userIds = await userRepository.GetAll()
                .Select(u => u.Id)
                .ToListAsync();
            foreach (var userId in userIds)
            {
                _usedUserIds.Add(userId);
            }

            // 加载所有群组ID
            var groupRepository = _unitOfWork.GetRepository<Group>();
            var groupIds = await groupRepository.GetAll()
                .Select(g => g.Id)
                .ToListAsync();
            foreach (var groupId in groupIds)
            {
                _usedGroupIds.Add(groupId);
            }
        }

        /// <summary>
        /// 生成指定长度的随机数字字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns>随机数字字符串</returns>
        private string GenerateRandomNumericString(int length)
        {
            var buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = (char)('0' + _random.Next(10));
            }
            return new string(buffer);
        }
    }
}
