using ChatServer.Common.Protobuf;
using ChatServer.DataBase.DataBase.UnitOfWork;
using ChatServer.Main.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Main.MessageOperate.Processor.RelationProcessor
{
    class RemoveMemberRequestProcessor : IProcessor<RemoveMemberRequest>
    {
        private readonly IUnitOfWork unitOfWork;

        public RemoveMemberRequestProcessor(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task Process(MessageUnit<RemoveMemberRequest> unit)
        {
            
        }
    }
}
