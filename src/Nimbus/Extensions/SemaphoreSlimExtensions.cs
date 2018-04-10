using System;
using System.Reflection;
using System.Threading;

namespace Nimbus.Extensions
{
    internal static class SemaphoreSlimExtensions
    {
        private static readonly FieldInfo _lockObjFieldInfo = typeof(SemaphoreSlim).GetField("m_lockObj", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool IsDisposed(this SemaphoreSlim semaphoreSlim)
        {
            if (semaphoreSlim == null)
            {
                throw new ArgumentNullException(nameof(semaphoreSlim));
            }

            return _lockObjFieldInfo.GetValue(semaphoreSlim) == null;
        }
    }
}