using ChatServer.DataBase.DataBase.DataEntity;
using Microsoft.EntityFrameworkCore;

namespace ChatServer.DataBase.DataBase
{
    public class ChatServerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserOnline> UserOnlines { get; set; }
        public DbSet<ChatPrivate> ChatPrivates { get; set; }
        public DbSet<ChatGroup> ChatGroups { get; set; }
        public DbSet<SacurityQuestion> SacurityQuestions { get; set; }
        public DbSet<FriendRelation> FriendRelations { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupRelation> GroupRelations { get; set; }
        public DbSet<GroupRequest> GroupRequests { get; set; }

        public ChatServerDbContext(DbContextOptions<ChatServerDbContext> options) : base(options) {

        }
    }
}
