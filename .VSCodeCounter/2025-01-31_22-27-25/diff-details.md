# Diff Details

Date : 2025-01-31 22:27:25

Directory d:\\LanguageLearnig\\C#\\ChatServer

Total : 51 files,  -338 codes, -20 comments, -271 blanks, all -629 lines

[Summary](results.md) / [Details](details.md) / [Diff Summary](diff.md) / Diff Details

## Files
| filename | language | code | comment | blank | total |
| :--- | :--- | ---: | ---: | ---: | ---: |
| [.vs/ChatServer/v17/DocumentLayout.json](/.vs/ChatServer/v17/DocumentLayout.json) | JSON | -206 | 0 | 0 | -206 |
| [ChatServer.Common/ChannelExtensions.cs](/ChatServer.Common/ChannelExtensions.cs) | C# | 2 | 0 | 0 | 2 |
| [ChatServer.Common/Protobuf/ChatRelationProtocol.cs](/ChatServer.Common/Protobuf/ChatRelationProtocol.cs) | C# | 261 | 6 | 22 | 289 |
| [ChatServer.Common/Protobuf/ChatUserProtocol.cs](/ChatServer.Common/Protobuf/ChatUserProtocol.cs) | C# | 590 | 16 | 61 | 667 |
| [ChatServer.Common/obj/ChatServer.Common.csproj.nuget.dgspec.json](/ChatServer.Common/obj/ChatServer.Common.csproj.nuget.dgspec.json) | JSON | 5 | 0 | 0 | 5 |
| [ChatServer.Common/obj/project.assets.json](/ChatServer.Common/obj/project.assets.json) | JSON | 5 | 0 | 0 | 5 |
| [ChatServer.DataBase/DataBase/ChatServerDbContext.cs](/ChatServer.DataBase/DataBase/ChatServerDbContext.cs) | C# | 0 | 0 | 1 | 1 |
| [ChatServer.DataBase/DataBase/DataEntity/FriendRelation.cs](/ChatServer.DataBase/DataBase/DataEntity/FriendRelation.cs) | C# | 2 | 0 | 1 | 3 |
| [ChatServer.DataBase/DataBase/DataEntity/FriendRequest.cs](/ChatServer.DataBase/DataBase/DataEntity/FriendRequest.cs) | C# | -5 | 0 | 0 | -5 |
| [ChatServer.DataBase/DataToProtoProfile.cs](/ChatServer.DataBase/DataToProtoProfile.cs) | C# | 5 | 0 | 3 | 8 |
| [ChatServer.DataBase/Migrations/20250123151515\_init.Designer.cs](/ChatServer.DataBase/Migrations/20250123151515_init.Designer.cs) | C# | -117 | -2 | -37 | -156 |
| [ChatServer.DataBase/Migrations/20250123151515\_init.cs](/ChatServer.DataBase/Migrations/20250123151515_init.cs) | C# | -110 | -3 | -12 | -125 |
| [ChatServer.DataBase/Migrations/20250126040237\_sacurityQuestion.Designer.cs](/ChatServer.DataBase/Migrations/20250126040237_sacurityQuestion.Designer.cs) | C# | -152 | -2 | -49 | -203 |
| [ChatServer.DataBase/Migrations/20250126040237\_sacurityQuestion.cs](/ChatServer.DataBase/Migrations/20250126040237_sacurityQuestion.cs) | C# | -64 | -3 | -9 | -76 |
| [ChatServer.DataBase/Migrations/20250126044330\_FriendRelation.Designer.cs](/ChatServer.DataBase/Migrations/20250126044330_FriendRelation.Designer.cs) | C# | -185 | -2 | -60 | -247 |
| [ChatServer.DataBase/Migrations/20250126044330\_FriendRelation.cs](/ChatServer.DataBase/Migrations/20250126044330_FriendRelation.cs) | C# | -50 | -3 | -5 | -58 |
| [ChatServer.DataBase/Migrations/20250126073001\_FriendRequest.Designer.cs](/ChatServer.DataBase/Migrations/20250126073001_FriendRequest.Designer.cs) | C# | -185 | -2 | -60 | -247 |
| [ChatServer.DataBase/Migrations/20250126073001\_FriendRequest.cs](/ChatServer.DataBase/Migrations/20250126073001_FriendRequest.cs) | C# | -14 | -3 | -6 | -23 |
| [ChatServer.DataBase/Migrations/20250126073248\_FriendRequest\_1.Designer.cs](/ChatServer.DataBase/Migrations/20250126073248_FriendRequest_1.Designer.cs) | C# | -228 | -2 | -76 | -306 |
| [ChatServer.DataBase/Migrations/20250126073248\_FriendRequest\_1.cs](/ChatServer.DataBase/Migrations/20250126073248_FriendRequest_1.cs) | C# | -59 | -3 | -6 | -68 |
| [ChatServer.DataBase/Migrations/20250126150837\_chatPrivate\_1.Designer.cs](/ChatServer.DataBase/Migrations/20250126150837_chatPrivate_1.Designer.cs) | C# | -228 | -2 | -76 | -306 |
| [ChatServer.DataBase/Migrations/20250126150837\_chatPrivate\_1.cs](/ChatServer.DataBase/Migrations/20250126150837_chatPrivate_1.cs) | C# | -50 | -3 | -10 | -63 |
| [ChatServer.DataBase/Migrations/20250127064706\_FriendRequest\_2.Designer.cs](/ChatServer.DataBase/Migrations/20250127064706_FriendRequest_2.Designer.cs) | C# | -230 | -2 | -77 | -309 |
| [ChatServer.DataBase/Migrations/20250127064706\_FriendRequest\_2.cs](/ChatServer.DataBase/Migrations/20250127064706_FriendRequest_2.cs) | C# | -23 | -3 | -4 | -30 |
| [ChatServer.DataBase/Migrations/20250131110141\_init\_1.Designer.cs](/ChatServer.DataBase/Migrations/20250131110141_init_1.Designer.cs) | C# | 237 | 2 | 80 | 319 |
| [ChatServer.DataBase/Migrations/20250131110141\_init\_1.cs](/ChatServer.DataBase/Migrations/20250131110141_init_1.cs) | C# | 227 | 3 | 23 | 253 |
| [ChatServer.DataBase/Migrations/ChatServerDbContextModelSnapshot.cs](/ChatServer.DataBase/Migrations/ChatServerDbContextModelSnapshot.cs) | C# | 7 | 0 | 3 | 10 |
| [ChatServer.DataBase/obj/ChatServer.DataBase.csproj.nuget.dgspec.json](/ChatServer.DataBase/obj/ChatServer.DataBase.csproj.nuget.dgspec.json) | JSON | 10 | 0 | 0 | 10 |
| [ChatServer.DataBase/obj/project.assets.json](/ChatServer.DataBase/obj/project.assets.json) | JSON | 5 | 0 | 0 | 5 |
| [ChatServer.Main/App.cs](/ChatServer.Main/App.cs) | C# | 1 | 0 | 0 | 1 |
| [ChatServer.Main/IOServer/Manager/ClientChannelManager.cs](/ChatServer.Main/IOServer/Manager/ClientChannelManager.cs) | C# | 5 | 0 | 0 | 5 |
| [ChatServer.Main/IOServer/ServerHandler/ClientConnectHandler.cs](/ChatServer.Main/IOServer/ServerHandler/ClientConnectHandler.cs) | C# | -2 | 0 | 0 | -2 |
| [ChatServer.Main/IOServer/ServerHandler/EchoServerHandler.cs](/ChatServer.Main/IOServer/ServerHandler/EchoServerHandler.cs) | C# | -2 | -4 | -1 | -7 |
| [ChatServer.Main/MessageOperate/BusinessServer.cs](/ChatServer.Main/MessageOperate/BusinessServer.cs) | C# | 1 | 0 | 0 | 1 |
| [ChatServer.Main/MessageOperate/Processor/FriendRequestProcessor.cs](/ChatServer.Main/MessageOperate/Processor/FriendRequestProcessor.cs) | C# | 20 | -7 | 1 | 14 |
| [ChatServer.Main/MessageOperate/Processor/FriendResponseProcessor.cs](/ChatServer.Main/MessageOperate/Processor/FriendResponseProcessor.cs) | C# | 28 | 3 | 2 | 33 |
| [ChatServer.Main/MessageOperate/Processor/GetUserRequestProcessor.cs](/ChatServer.Main/MessageOperate/Processor/GetUserRequestProcessor.cs) | C# | 3 | 0 | 1 | 4 |
| [ChatServer.Main/MessageOperate/Processor/LoginRequestProcessor.cs](/ChatServer.Main/MessageOperate/Processor/LoginRequestProcessor.cs) | C# | -1 | -2 | 0 | -3 |
| [ChatServer.Main/MessageOperate/Processor/LogoutRequestProcessor.cs](/ChatServer.Main/MessageOperate/Processor/LogoutRequestProcessor.cs) | C# | 3 | 0 | 1 | 4 |
| [ChatServer.Main/MessageOperate/Processor/OutlineMessageRequestProcessor.cs](/ChatServer.Main/MessageOperate/Processor/OutlineMessageRequestProcessor.cs) | C# | 74 | 2 | 12 | 88 |
| [ChatServer.Main/MessageOperate/Processor/RegisteRequestProcessor.cs](/ChatServer.Main/MessageOperate/Processor/RegisteRequestProcessor.cs) | C# | 3 | 0 | 1 | 4 |
| [ChatServer.Main/MessageOperate/ProtobufDispatcher.cs](/ChatServer.Main/MessageOperate/ProtobufDispatcher.cs) | C# | -14 | -6 | -1 | -21 |
| [ChatServer.Main/MessageOperate/Server/LoginServer.cs](/ChatServer.Main/MessageOperate/Server/LoginServer.cs) | C# | 1 | 0 | 0 | 1 |
| [ChatServer.Main/Services/FriendService.cs](/ChatServer.Main/Services/FriendService.cs) | C# | 33 | 1 | 5 | 39 |
| [ChatServer.Main/obj/ChatServer.Main.csproj.nuget.dgspec.json](/ChatServer.Main/obj/ChatServer.Main.csproj.nuget.dgspec.json) | JSON | 20 | 0 | 0 | 20 |
| [ChatServer.Main/obj/project.assets.json](/ChatServer.Main/obj/project.assets.json) | JSON | 5 | 0 | 0 | 5 |
| [ChatServer.Test/obj/ChatServer.Test.csproj.nuget.dgspec.json](/ChatServer.Test/obj/ChatServer.Test.csproj.nuget.dgspec.json) | JSON | 15 | 0 | 0 | 15 |
| [ChatServer.Test/obj/project.assets.json](/ChatServer.Test/obj/project.assets.json) | JSON | 5 | 0 | 0 | 5 |
| [SocketServer/Server/SocketServer.cs](/SocketServer/Server/SocketServer.cs) | C# | 4 | 1 | 1 | 6 |
| [SocketServer/obj/SocketServer.csproj.nuget.dgspec.json](/SocketServer/obj/SocketServer.csproj.nuget.dgspec.json) | JSON | 5 | 0 | 0 | 5 |
| [SocketServer/obj/project.assets.json](/SocketServer/obj/project.assets.json) | JSON | 5 | 0 | 0 | 5 |

[Summary](results.md) / [Details](details.md) / [Diff Summary](diff.md) / Diff Details