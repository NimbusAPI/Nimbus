using Nimbus.MessageContracts;

namespace Nimbus.Tests.Integration.Tests.AbstractBaseTypeMessageTests.MessageContracts
{
    public class SomeConcreteRequestType : SomeAbstractRequestType<SomeConcreteRequestType, SomeConcreteResponseType>,
                                           IBusMulticastRequest<SomeConcreteRequestType, SomeConcreteResponseType>
    {
    }
}