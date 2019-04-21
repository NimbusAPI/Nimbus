using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using Nimbus.Extensions;
using NullGuard;

namespace Nimbus.Infrastructure.Serialization
{
    internal class NimbusDataContractResolver : DataContractResolver
    {
        private const string _xmlNamespace = "http://dynamicdatacontracttypes.nimbusapi.github.io/";
        private readonly Dictionary<string, Assembly> _typeToAssemblyLookup;

        public NimbusDataContractResolver(ITypeProvider typeProvider)
        {
            _typeToAssemblyLookup = typeProvider
                .AllSerializableTypes()
                .ToDictionary(t => t.FullName, t => t.Assembly);
        }

        public override bool TryResolveType(Type type,
                                            Type declaredType,
                                            DataContractResolver knownTypeResolver,
                                            out XmlDictionaryString typeName,
                                            out XmlDictionaryString typeNamespace)
        {
            var fullName = type.FullName;

            if (_typeToAssemblyLookup.ContainsKey(fullName))
            {
                var dictionary = new XmlDictionary();
                typeName = dictionary.Add(fullName);
                typeNamespace = dictionary.Add(_xmlNamespace);
                return true;
            }

            return knownTypeResolver.TryResolveType(type, declaredType, null, out typeName, out typeNamespace);
        }

        public override Type ResolveName(string typeName, string typeNamespace, [AllowNull]Type declaredType, DataContractResolver knownTypeResolver)
        {
            if (string.Compare(typeNamespace, _xmlNamespace, StringComparison.OrdinalIgnoreCase) == 0)
            {
                Assembly containingAssembly;
                if (_typeToAssemblyLookup.TryGetValue(typeName, out containingAssembly))
                {
                    var type = containingAssembly.GetType(typeName);
                    if (type != null) return type;
                }
            }

            return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
        }
    }
}