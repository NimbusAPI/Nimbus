using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Nimbus.Configuration.PoorMansIocContainer;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.MessageContracts.Exceptions;

namespace Nimbus.Configuration
{
    internal static class ComponentRegistrationExtensions
    {
        public static void RegisterPropertiesFromConfigurationObject(this PoorMansIoC container, INimbusConfiguration configuration)
        {
            configuration.RegisterWith(container);

            configuration
                .GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof (ISetting).IsAssignableFrom(p.PropertyType))
                .Select(p =>
                        {
                            var v = p.GetValue(configuration);
                            if (v == null) Debugger.Break();
                            return v;
                        })
                .Do(container.Register)
                .Done();

            configuration
                .GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof (INimbusConfiguration).IsAssignableFrom(p.PropertyType))
                .Select(p => (INimbusConfiguration) p.GetValue(configuration))
                .Do(container.RegisterPropertiesFromConfigurationObject)
                .Done();
        }

        public static IEnumerable<string> ValidationErrors(this IValidatableConfigurationSetting o)
        {
            foreach (var validationError in o.Validate()) yield return validationError;

            var settingType = o.GetType();
            var properties = settingType
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(p => typeof (IValidatableConfigurationSetting).IsAssignableFrom(p.PropertyType))
                .ToArray();

            foreach (var prop in properties)
            {
                var value = (IValidatableConfigurationSetting) prop.GetValue(o);
                if (value == null)
                {
                    yield return "Property {0} of {1} has not been provided.".FormatWith(prop.Name, settingType.Name);
                    continue;
                }

                foreach (var childValidationMessage in value.ValidationErrors()) yield return childValidationMessage;
            }
        }

        public static void AssertConfigurationIsValid(this IValidatableConfigurationSetting configuration)
        {
            var validationErrors = configuration.ValidationErrors().ToArray();
            if (validationErrors.None()) return;

            var message = string.Join(Environment.NewLine, new[] {"Bus configuration is invalid:"}.Concat(validationErrors));
            throw new BusException(message);
        }
    }
}