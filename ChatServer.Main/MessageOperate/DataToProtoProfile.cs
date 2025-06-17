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

            CreateMap<User, UserMessage>()
                .ForMember(um => um.RegisterTime, opt => opt.MapFrom(u => u.RegisteTime.ToString()))
                .ForMember(um => um.Introduction, opt => opt.MapFrom(u => u.Introduction ?? string.Empty))
                .ForMember(um => um.Birth, opt => opt.MapFrom(u => u.Birth == null ? string.Empty : u.Birth.ToString()));

            CreateMap<UserMessage, User>()
                .ForMember(u => u.RegisteTime, opt => opt.MapFrom(um => DateTime.Parse(um.RegisterTime)))
                .ForMember(u => u.Introduction, opt => opt.MapFrom(um => string.IsNullOrEmpty(um.Introduction) ? null : um.Introduction))
                .ForMember(u => u.Birth, opt => opt.MapFrom(um => string.IsNullOrEmpty(um.Birth) ? (DateOnly?)null : DateOnly.Parse(um.Birth)));

            #endregion

            #region User + UserDetailMessage
            CreateMap<User, UserDetailMessage>()
                .ForMember(um => um.RegisterTime, opt => opt.MapFrom(u => u.RegisteTime.ToString()))
                .ForMember(um => um.Introduction, opt => opt.MapFrom(u => u.Introduction ?? string.Empty))
                .ForMember(um => um.Birth, opt => opt.MapFrom(u => u.Birth == null ? string.Empty : u.Birth.ToString()))
                .ForMember(um => um.LastDeleteFriendMessageTime, opt => opt.MapFrom(um => um.LastDeleteFriendMessageTime.ToString()))
                .ForMember(um => um.LastDeleteGroupMessageTime, opt => opt.MapFrom(um => um.LastDeleteGroupMessageTime.ToString()))
                .ForMember(um => um.LastReadFriendMessageTime, opt => opt.MapFrom(um => um.LastReadFriendMessageTime.ToString()))
                .ForMember(um => um.LastReadGroupMessageTime, opt => opt.MapFrom(um => um.LastReadGroupMessageTime.ToString()))
                .ForMember(um => um.EmailNumber, opt => opt.MapFrom(u => u.EmailNumber ?? string.Empty))
                .ForMember(um => um.PhoneNumber, opt => opt.MapFrom(u => u.PhoneNumber ?? string.Empty));

            CreateMap<UserDetailMessage, User>()
                .ForMember(u => u.RegisteTime, opt => opt.MapFrom(um => DateTime.Parse(um.RegisterTime)))
                .ForMember(u => u.Introduction, opt => opt.MapFrom(um => string.IsNullOrEmpty(um.Introduction) ? null : um.Introduction))
                .ForMember(u => u.Birth, opt => opt.MapFrom(um => string.IsNullOrEmpty(um.Birth) ? (DateOnly?)null : DateOnly.Parse(um.Birth)))
                .ForMember(u => u.LastReadFriendMessageTime, opt => opt.MapFrom(um => DateTime.Parse(um.LastReadFriendMessageTime)))
                .ForMember(u => u.LastReadGroupMessageTime, opt => opt.MapFrom(um => DateTime.Parse(um.LastReadGroupMessageTime)))
                .ForMember(u => u.LastDeleteFriendMessageTime, opt => opt.MapFrom(um => DateTime.Parse(um.LastDeleteFriendMessageTime)))
                .ForMember(u => u.LastDeleteGroupMessageTime, opt => opt.MapFrom(um => DateTime.Parse(um.LastDeleteGroupMessageTime)))
                .ForMember(u => u.EmailNumber, opt => opt.MapFrom(um => string.IsNullOrEmpty(um.EmailNumber) ? null : um.EmailNumber))
                .ForMember(u => u.PhoneNumber, opt => opt.MapFrom(um => string.IsNullOrEmpty(um.PhoneNumber) ? null : um.PhoneNumber));
            #endregion

            #region FriendRequest
            CreateMap<FriendRequestFromClient, FriendRequest>()
                .ForMember(fr => fr.RequestTime, opt => opt.MapFrom(u => DateTime.Parse(u.RequestTime)));

            CreateMap<FriendRequest, FriendRequestFromServer>()
                .ForMember(fr => fr.RequestId, opt => opt.MapFrom(u => u.Id))
                .ForMember(fr => fr.RequestTime, opt => opt.MapFrom(u => u.RequestTime.ToString()));

            CreateMap<FriendRequest,FriendRequestMessage>()
                .ForMember(frm => frm.RequestTime,opt => opt.MapFrom(fr => fr.RequestTime.ToString()))
                .ForMember(frm => frm.SolvedTime,opt => opt.MapFrom(fr => fr.SolveTime == null ? string.Empty : fr.SolveTime.ToString()))
                .ForMember(frm => frm.RequestId,opt => opt.MapFrom(fr => fr.Id));
            #endregion

            #region GroupRequest
            CreateMap<GroupRequest, GroupRequestMessage>()
                .ForMember(grm => grm.RequestTime, opt => opt.MapFrom(gr => gr.RequestTime.ToString()))
                .ForMember(grm => grm.RequestId, opt => opt.MapFrom(gr => gr.Id))
                .ForMember(grm => grm.AcceptByUserId, opt => opt.MapFrom(gm => gm.AcceptByUserId == null ? string.Empty : gm.AcceptByUserId))
                .ForMember(grm => grm.SolvedTime, opt => opt.MapFrom(gr => gr.SolveTime == null ? string.Empty: gr.SolveTime.ToString()));
            #endregion

            #region Group + GroupMessage

            CreateMap<Group, GroupMessage>()
                .ForMember(gm => gm.CreateTime, opt => opt.MapFrom(g => g.CreateTime.ToString()))
                .ForMember(gm => gm.Description, opt => opt.MapFrom(g => g.Description ?? string.Empty))
                .ForMember(gm => gm.GroupId, opt => opt.MapFrom(g => g.Id));

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
                .ForMember(gdm => gdm.Method, opt => opt.MapFrom(gd => gd.DeleteMethod))
                .ForMember(gdm => gdm.OperateId, opt => opt.MapFrom(gd => gd.OperateUserId));
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
                .ForMember(fcm => fcm.Time, opt => opt.MapFrom(cp => cp.Time.ToString()))
                .ForMember(fcm => fcm.RetractTime, opt => opt.MapFrom(cp => cp.RetractTime.ToString()));
            CreateMap<FriendChatMessage, ChatPrivate>()
                .ForMember(cp => cp.Message, opt => opt.MapFrom(fcm => ChatMessageHelper.EncruptChatMessage(fcm.Messages)))
                .ForMember(cp => cp.Time, opt => opt.MapFrom(fcm => DateTime.Parse(fcm.Time)))
                .ForMember(cp => cp.RetractTime, opt => opt.MapFrom(fcm => DateTime.Parse(fcm.RetractTime)));
            #endregion

            #region ChatGroup + GroupChatMessage
            CreateMap<GroupChatMessage, ChatGroup>()
                .ForMember(cg => cg.Message, opt => opt.MapFrom(gcm => ChatMessageHelper.EncruptChatMessage(gcm.Messages)))
                .ForMember(cg => cg.Time, opt => opt.MapFrom(gcm => DateTime.Parse(gcm.Time)))
                .ForMember(cg => cg.RetractTime, opt => opt.MapFrom(gcm => DateTime.Parse(gcm.RetractTime)));
            CreateMap<ChatGroup, GroupChatMessage>()
                .ForMember(gcm => gcm.Messages, opt => opt.MapFrom(cg => ChatMessageHelper.DecruptChatMessage(cg.Message)))
                .ForMember(gcm => gcm.Time, opt => opt.MapFrom(cg => cg.Time.ToString()))
                .ForMember(gcm => gcm.RetractTime, opt => opt.MapFrom(cg => cg.RetractTime.ToString()));
            #endregion

            #region ChatPrivateDetailMessage + ChatPrivateDetail
            CreateMap<ChatPrivateDetail, ChatPrivateDetailMessage>().ReverseMap();
            #endregion

            #region ChatGroupDetailMessage + ChatGroupDetail
            CreateMap<ChatGroupDetail, ChatGroupDetailMessage>().ReverseMap();
            #endregion

            #region GroupRelation + EnterGroupMessage
            CreateMap<GroupRelation, EnterGroupMessage>()
                .ForMember(egm => egm.JoinTime, opt => opt.MapFrom(gr => gr.JoinTime.ToString()))
                .ForMember(egm => egm.Remark, opt => opt.MapFrom(gr => gr.Remark ?? string.Empty))
                .ForMember(egm => egm.NickName, opt => opt.MapFrom(gr => gr.NickName ?? string.Empty));
            #endregion

            #region UserGroup + UserGroupMessage
            CreateMap<UserGroup, UserGroupMessage>()
                .ForMember(ugm => ugm.UserId, opt => opt.MapFrom(ug => ug.UserId))
                .ForMember(ugm => ugm.GroupName, opt => opt.MapFrom(ug => ug.GroupName))
                .ForMember(ugm => ugm.GroupType, opt => opt.MapFrom(ug => ug.GroupType));
            #endregion

            #region JoinGroupRequestFromClient + GroupRequest

            CreateMap<JoinGroupRequestFromClient, GroupRequest>()
                .ForMember(gr => gr.UserFromId, opt => opt.MapFrom(g => g.UserId))
                .ForMember(gr => gr.RequestTime, opt => opt.Ignore());

            #endregion
        }
    }
}
