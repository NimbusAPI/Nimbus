using System;

namespace Nimbus.Configuration.Settings
{
    //FIXME there should be a nicer way to specify these. Perhaps use the ITypeProvider
    // combined with some class-level attributes?
    public class GlobalInboundInterceptorTypesSetting : Setting<Type[]>
    {
        public override Type[] Default
        {
            get { return new Type[0]; }
        }
    }
}