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

namespace ChatServer.Main.Services
{
    public interface ILoginService
    {
        Task<bool> Registe(string Name, string Password);
        Task<string?> Login(string Id, string Password);
        Task UserOutline(ClientChannel client);
    }

    public class LoginService : ILoginService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICipherHelper cipherHelper;

        public LoginService(IUnitOfWork unitOfWork, ICipherHelper cipherHelper)
        {
            this.unitOfWork = unitOfWork;
            this.cipherHelper = cipherHelper;
        }

        /// <summary>
        /// 生成用户ID
        /// </summary>
        /// <returns></returns>
        private string GenerateID()
        {
            var userRepository = unitOfWork.GetRepository<User>();

            int count = userRepository.Count() + 2024000001;
            string Id = count.ToString("D10");

            return Id;
        }

        public async Task<bool> Registe(string Name, string Password)
        {
            if (Password.Length < 6 || Password.Length > 18 || string.IsNullOrEmpty(Name))
                return false;

            string encryptPassword = cipherHelper.Encrypt(Password);
            int count = encryptPassword.Length;

            User user = new User { Id = GenerateID(),
                Name = Name,
                HeadCount = 0,
                HeadIndex = 0,
                Password = encryptPassword,
                RegisteTime = DateTime.Now };

            var userRepository = unitOfWork.GetRepository<User>();

            try
            {
                var result = await userRepository.InsertAsync(user);
                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                return await Registe(Name, Password);
            }
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
