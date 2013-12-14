using System;
using System.Collections;
using System.Collections.Generic;
using Nimbus;
using System.Threading.Tasks;
using Nimbus.MessageContracts;

namespace PingPong.Windsor
{
    public class Ping : BusRequest<Ping, Pong>
    {
        public string Message { get; set; }
    }
}