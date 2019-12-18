using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Nimbus.Extensions;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.Tests.Unit.Conventions
{
    [TestFixture]
    [Category("Convention")]
    public class NimbusCode
    {
        [Test]
        public void MustAdhereToConventions()
        {
            var types = typeof(Bus).Assembly    //TODO scan all Nimbus assemblies
                                   .GetTypes();

            foreach (var type in types)
            {
                ShouldNeverUseLazy(type);
                ShouldNeverUseBlockingCollection(type);
            }
        }

        /// <summary>
        ///     Lazy doesn't prevent multiple creation of items when the Lazy object is uninitialized. Use ThreadSafeLazy instead.
        /// </summary>
        private static void ShouldNeverUseLazy(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            fields.Where(f => f.FieldType.IsClosedTypeOf(typeof(Lazy<>))).ShouldBeEmpty();
        }

        /// <summary>
        ///     BlockingCollection blocks the thread, which is a Bad Thing. Use AsyncBlockingCollection instead.
        /// </summary>
        private static void ShouldNeverUseBlockingCollection(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            fields.Where(f => f.FieldType.IsClosedTypeOf(typeof(BlockingCollection<>))).ShouldBeEmpty();
        }
    }
}