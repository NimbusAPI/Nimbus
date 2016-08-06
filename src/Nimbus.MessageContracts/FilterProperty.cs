using System;

namespace Nimbus.MessageContracts
{
    /// <summary>
    ///     Indicates that this property should be included in the property bag of the bus message.
    ///     Use this on properties on which subscriptions will be filtered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class FilterProperty : Attribute
    {
    }
}