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
                .ForMember(um => um.Birth,opt => opt.MapFrom(u => u.Birth.ToString()));
            CreateMap<UserMessage, User>()
                .ForMember(u => u.RegisteTime, opt => opt.MapFrom(um => DateTime.Parse(um.RegisterTime)))
                .ForMember(u => u.Birth, opt => opt.MapFrom(um => DateOnly.Parse(um.Birth)));
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

            #region NewFriendMessage + FriendRelation
            CreateMap<FriendRelation, NewFriendMessage>()
                .ForMember(cp => cp.FrinedId, opt => opt.MapFrom(nfm => nfm.User2Id))
                .ForMember(cp => cp.Group, opt => opt.MapFrom(nfm => nfm.Grouping))
                .ForMember(cp => cp.RelationTime, opt => opt.MapFrom(nfm => nfm.GroupTime.ToString()));
            #endregion

            #region ChatPrivate + FriendChatMessage
            CreateMap<ChatPrivate, FriendChatMessage>()
                .ForMember(fcm => fcm.Messages, opt => opt.MapFrom(cp => ChatMessageHelper.DecruptChatMessage(cp.Message)))
                .ForMember(fcm => fcm.Time, opt => opt.MapFrom(cp => cp.Time.ToString()));
            CreateMap<FriendChatMessage, ChatPrivate>()
                .ForMember(cp => cp.Message, opt => opt.MapFrom(fcm => ChatMessageHelper.EncruptChatMessage(fcm.Messages)))
                .ForMember(cp => cp.Time, opt => opt.MapFrom(fcm => DateTime.Parse(fcm.Time)));
            #endregion

        }
    }
}
