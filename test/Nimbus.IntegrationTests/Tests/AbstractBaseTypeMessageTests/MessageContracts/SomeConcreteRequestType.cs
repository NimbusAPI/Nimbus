using Nimbus.MessageContracts;

namespace Nimbus.IntegrationTests.Tests.AbstractBaseTypeMessageTests.MessageContracts
{
    public class SomeConcreteRequestType : SomeAbstractRequestType<SomeConcreteRequestType, SomeConcreteResponseType>,
                                           IBusMulticastRequest<SomeConcreteRequestType, SomeConcreteResponseType>
    {
    }
}