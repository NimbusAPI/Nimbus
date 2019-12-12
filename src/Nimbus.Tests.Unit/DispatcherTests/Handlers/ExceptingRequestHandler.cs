using System;
using System.Threading.Tasks;
using Nimbus.InfrastructureContracts.Handlers;
using Nimbus.Tests.Unit.DispatcherTests.MessageContracts;

namespace Nimbus.Tests.Unit.DispatcherTests.Handlers
{
    public class ExceptingRequestHandler : IHandleRequest<ExceptingRequest, ExceptingResponse>
    {
        public Task<ExceptingResponse> Handle(ExceptingRequest request)
        {
            throw new Exception("Ruh roh");
        }
    }
}