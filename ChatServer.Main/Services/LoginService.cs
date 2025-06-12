using ChatServer.Common.Protobuf;
using ChatServer.DataBase;
using ChatServer.DataBase.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.IOServer.Manager;
using ChatServer.Main.Services.Helper;
using ChatServer.Main.Entity;
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;
using ChatServer.Main.Manager;
using Microsoft.Extensions.Configuration;

namespace ChatServer.Main.Services
{
    public interface ILoginService
    {
        Task<(bool,string?)> Registe(string Name, string Password);
        Task<string?> Login(string Id, string Password);
        Task UserOutline(ClientChannel client);
    }

    public class LoginService : BaseService,ILoginService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICipherHelper cipherHelper;

        public LoginService(IServiceProvider serviceProvider, ICipherHelper cipherHelper):base(serviceProvider)
        {
            unitOfWork = _scopedProvider.ServiceProvider.GetRequiredService<IUnitOfWork>();
            this.cipherHelper = cipherHelper;
        }

        /// <summary>
        /// 自动添加机器人
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public async Task<(bool, string?)> Registe(string Name, string Password)
        {
            if (Password.Length < 6 || Password.Length > 18 || string.IsNullOrEmpty(Name))
                return (false, null);

            string encryptPassword = cipherHelper.Encrypt(Password);
            int count = encryptPassword.Length;

            var idGeneratorManager = _scopedProvider.ServiceProvider.GetRequiredService<IIdGeneratorManager>();

            User user = new User
            {
                Id = idGeneratorManager.GenerateUserId(),
                Name = Name,
                HeadCount = 0,
                HeadIndex = -1,
                Password = encryptPassword,
                RegisteTime = DateTime.Now
            };

            var userRepository = unitOfWork.GetRepository<User>();

            try
            {
                var result = await userRepository.InsertAsync(user);
                await unitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return await Registe(Name, Password);
            }

            // -- 自动添加机器人 --//
            var configuration = _scopedProvider.ServiceProvider.GetRequiredService<IConfigurationRoot>();
            var robotId = configuration.GetValue("Robot:Id","1310000001");
            var robot = await userRepository.GetFirstOrDefaultAsync(predicate: d => d.Id.Equals(robotId));

            // 创建机器人
            if (robot != null)
            {
                try
                {
                    var friendRelationRepository = unitOfWork.GetRepository<FriendRelation>();
                    var friendRelation1 = new FriendRelation
                    {
                        User1Id = user.Id,
                        User2Id = robotId,
                        Grouping = "默认分组",
                        GroupTime = DateTime.Now
                    };
                    var friendRelation2 = new FriendRelation
                    {
                        User1Id = robotId,
                        User2Id = user.Id,
                        Grouping = "默认分组",
                        GroupTime = DateTime.Now
                    };
                    await friendRelationRepository.InsertAsync(friendRelation1);
                    await friendRelationRepository.InsertAsync(friendRelation2);
                    await unitOfWork.SaveChangesAsync();
                }
                catch { }
            }

            return (true, user.Id);
        }

        /// <summary>
        /// 登录成功，返回用户名
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public async Task<string?> Login(string Id, string Password)
        {
            var userRepository = unitOfWork.GetRepository<User>();

            var encryptPassword = cipherHelper.Encrypt(Password);

            var user = await userRepository.GetFirstOrDefaultAsync(predicate: d => d.Id.Equals(Id) && d.Password.Equals(encryptPassword));

            // 用户不存在，登录失败
            if (user == null)
                return null;

            return user.Name;
        }

        public async Task UserOutline(ClientChannel client)
        {
            if(client == null || !client.isLogined)
                return;

            UserOnline userOnline = new UserOnline()
            {
                UserId = client.userId!,
                LoginTime = client.loginTime!.Value,
                LogoutTime = DateTime.Now
            };

            var repository = unitOfWork.GetRepository<UserOnline>();
            await repository.InsertAsync(userOnline);
            await unitOfWork.SaveChangesAsync();
        }
    }
}
