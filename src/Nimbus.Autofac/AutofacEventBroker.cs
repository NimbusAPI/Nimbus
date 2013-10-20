using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Autofac
{
    public class AutofacEventBroker : IEventBroker
    {
        public void Publish<TBusEvent>(TBusEvent busEvent)
        {
            throw new NotImplementedException();
        }
    }
}
