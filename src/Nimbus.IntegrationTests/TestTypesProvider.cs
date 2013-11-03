using System;
using System.Collections.Generic;
using System.Linq;
using Nimbus.Configuration;

namespace Nimbus.IntegrationTests
{
    public class TestTypesProvider : ITypeProvider
    {
        private readonly Type[] _commandHandlerTypes;
        private readonly Type[] _eventHandlerTypes;
        private readonly Type[] _requestHandlerTypes;

        public TestTypesProvider(IEnumerable<Type> commandHandlerTypes, IEnumerable<Type> eventHandlerTypes, IEnumerable<Type> requestHandlerTypes)
        {
            _commandHandlerTypes = commandHandlerTypes.ToArray();
            _eventHandlerTypes = eventHandlerTypes.ToArray();
            _requestHandlerTypes = requestHandlerTypes.ToArray();
        }

        public IEnumerable<Type> CommandHandlerTypes
        {
            get { return _commandHandlerTypes; }
        }

        public IEnumerable<Type> EventHandlerTypes
        {
            get { return _eventHandlerTypes; }
        }

        public IEnumerable<Type> RequestHandlerTypes
        {
            get { return _requestHandlerTypes; }
        }
    }
}