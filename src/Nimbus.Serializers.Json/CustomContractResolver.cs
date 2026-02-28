using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nimbus.Serializers.Json
{
    public class CustomContractResolver : DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var members = objectType
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Cast<MemberInfo>()
                .ToList();

            return members;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);
            if (jsonProperty.Writable) return jsonProperty;

            var propertyInfo = member as PropertyInfo;
            if (propertyInfo == null) return jsonProperty;

            jsonProperty.Writable = propertyInfo.CanWrite;
            return jsonProperty;
        }
    }
}