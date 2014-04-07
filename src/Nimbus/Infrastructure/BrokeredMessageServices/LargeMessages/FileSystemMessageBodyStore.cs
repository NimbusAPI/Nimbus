using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Infrastructure.BrokeredMessageServices.LargeMessages
{
    class FileSystemMessageBodyStore: IMessageBodyStore
    {
        public Task Store(string id, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> Retrieve(string id)
        {
            throw new NotImplementedException();
        }

        public Task Delete(string id)
        {
            throw new NotImplementedException();
        }
    }
}
