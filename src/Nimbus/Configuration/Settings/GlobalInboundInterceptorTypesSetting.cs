using System;

namespace Nimbus.Configuration.Settings
{
    public class GlobalInboundInterceptorTypesSetting : Setting<Type[]>
    {
        public override Type[] Default
        {
            get { return new Type[0]; }
        }
    }
}