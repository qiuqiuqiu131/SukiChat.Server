using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.DataBase.DataBase.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ChatServer.Main.Services
{
    public interface IFriendService
    {
        public Task<bool> IsFriend(string Id1, string Id2);
    }

    public class FriendService : IFriendService
    {
        private readonly IUnitOfWork unitOfWork;

        public FriendService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        // 判断是否已经为好友
        public async Task<bool> IsFriend(string Id1, string Id2)
        {
            var friendRespository = unitOfWork.GetRepository<FriendRelation>();
            var friend = await friendRespository.GetFirstOrDefaultAsync(predicate: x => x.User1Id.Equals(Id1) && x.User2Id.Equals(Id2));
            if(friend == null)
                return false;
            else
                return true;
        }
    }
}
