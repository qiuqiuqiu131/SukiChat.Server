using AutoMapper;
using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.DataEntity;
using ChatServer.Main.Services.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate
{
    public class DataToProtoProfile:Profile
    {
        public DataToProtoProfile()
        {
            #region User + UserMessage
            CreateMap<User,UserMessage>()
                .ForMember(um => um.RegisterTime,opt => opt.MapFrom(u => u.RegisteTime.ToString()))
                .ForMember(um => um.Introduction,opt => opt.MapFrom(u => u.Introduction ?? string.Empty))
                .ForMember(um => um.Birth,opt => opt.MapFrom(u => u.Birth == null ? string.Empty : u.Birth.ToString()));
            CreateMap<UserMessage, User>()
                .ForMember(u => u.RegisteTime, opt => opt.MapFrom(um => DateTime.Parse(um.RegisterTime)))
                .ForMember(u => u.Introduction,opt => opt.MapFrom(um => string.IsNullOrEmpty(um.Introduction)?null:um.Introduction))
                .ForMember(u => u.Birth, opt => opt.MapFrom(um => string.IsNullOrEmpty(um.Birth) ? (DateOnly?)null: DateOnly.Parse(um.Birth)));
            #endregion

            #region FriendRequest
            CreateMap<FriendRequestFromClient, FriendRequest>()
                .ForMember(fr => fr.RequestTime, opt => opt.MapFrom(u => DateTime.Parse(u.RequestTime)));

            CreateMap<FriendRequest, FriendRequestFromServer>()
                .ForMember(fr => fr.RequestId, opt => opt.MapFrom(u => u.Id))
                .ForMember(fr => fr.RequestTime, opt => opt.MapFrom(u => u.RequestTime.ToString()));

            CreateMap<FriendRequest,FriendRequestMessage>()
                .ForMember(frm => frm.RequestTime,opt => opt.MapFrom(fr => fr.RequestTime.ToString()))
                .ForMember(frm => frm.SolvedTime,opt => opt.MapFrom(fr => fr.SolveTime.ToString()))
                .ForMember(frm => frm.RequestId,opt => opt.MapFrom(fr => fr.Id));
            #endregion

            #region GroupRequest
            CreateMap<GroupRequest, GroupRequestMessage>()
                .ForMember(grm => grm.RequestTime, opt => opt.MapFrom(gr => gr.RequestTime.ToString()))
                .ForMember(grm => grm.RequestId, opt => opt.MapFrom(gr => gr.Id))
                .ForMember(grm => grm.AcceptByUserId, opt => opt.MapFrom(gm => gm.AcceptByUserId == null ? string.Empty : gm.AcceptByUserId))
                .ForMember(grm => grm.SolvedTime, opt => opt.MapFrom(gr => gr.SolveTime == null ? string.Empty: gr.SolveTime.ToString()));
            #endregion

            #region FriendDelete + FriendDeleteMessage
            CreateMap<FriendDelete, FriendDeleteMessage>()
                .ForMember(fdm => fdm.Time, opt => opt.MapFrom(fd => fd.Time.ToString()))
                .ForMember(fdm => fdm.DeleteId, opt => opt.MapFrom(fd => fd.Id))
                .ForMember(fdm => fdm.UserId, opt => opt.MapFrom(fd => fd.UserId1))
                .ForMember(fdm => fdm.FriendId, opt => opt.MapFrom(fd => fd.UserId2));
            #endregion

            #region GroupDelete + GroupDeleteMessage
            CreateMap<GroupDelete, GroupDeleteMessage>()
                .ForMember(gdm => gdm.DeleteId, opt => opt.MapFrom(gd => gd.Id))
                .ForMember(gdm => gdm.Time, opt => opt.MapFrom(gd => gd.Time.ToString()))
                .ForMember(gdm => gdm.Method, opt => opt.MapFrom(gd => gd.DeleteMethod));
            #endregion

            #region NewFriendMessage + FriendRelation
            CreateMap<FriendRelation, NewFriendMessage>()
                .ForMember(cp => cp.UserId, opt => opt.MapFrom(nfm => nfm.User1Id))
                .ForMember(cp => cp.FrinedId, opt => opt.MapFrom(nfm => nfm.User2Id))
                .ForMember(cp => cp.RelationTime, opt => opt.MapFrom(nfm => nfm.GroupTime.ToString()))
                .ForMember(cp => cp.Remark, opt => opt.MapFrom(nfm => nfm.Remark ?? string.Empty));
            #endregion

            #region ChatPrivate + FriendChatMessage
            CreateMap<ChatPrivate, FriendChatMessage>()
                .ForMember(fcm => fcm.Messages, opt => opt.MapFrom(cp => ChatMessageHelper.DecruptChatMessage(cp.Message)))
                .ForMember(fcm => fcm.Time, opt => opt.MapFrom(cp => cp.Time.ToString()));
            CreateMap<FriendChatMessage, ChatPrivate>()
                .ForMember(cp => cp.Message, opt => opt.MapFrom(fcm => ChatMessageHelper.EncruptChatMessage(fcm.Messages)))
                .ForMember(cp => cp.Time, opt => opt.MapFrom(fcm => DateTime.Parse(fcm.Time)));
            #endregion

            #region ChatGroup + GroupChatMessage
            CreateMap<GroupChatMessage, ChatGroup>()
                .ForMember(cg => cg.Message, opt => opt.MapFrom(gcm => ChatMessageHelper.EncruptChatMessage(gcm.Messages)))
                .ForMember(cg => cg.Time,opt => opt.MapFrom(gcm =>  DateTime.Parse(gcm.Time)));
            CreateMap<ChatGroup, GroupChatMessage>()
                .ForMember(gcm => gcm.Messages, opt => opt.MapFrom(cg => ChatMessageHelper.DecruptChatMessage(cg.Message)))
                .ForMember(gcm => gcm.Time, opt => opt.MapFrom(cg => cg.Time.ToString()));
            #endregion

            #region GroupRelation + EnterGroupMessage
            CreateMap<GroupRelation, EnterGroupMessage>()
                .ForMember(egm => egm.JoinTime, opt => opt.MapFrom(gr => gr.JoinTime.ToString()))
                .ForMember(egm => egm.Remark, opt => opt.MapFrom(gr => gr.Remark ?? string.Empty))
                .ForMember(egm => egm.NickName, opt => opt.MapFrom(gr => gr.NickName ?? string.Empty));
            #endregion
        }
    }
}
