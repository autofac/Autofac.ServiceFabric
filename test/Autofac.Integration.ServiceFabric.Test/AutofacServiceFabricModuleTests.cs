using System;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Test
{
    public sealed class AutofacServiceFabricModuleTests
    {
        [Theory]
        [InlineData(typeof(AutofacActorInterceptor))]
        [InlineData(typeof(AutofacServiceInterceptor))]
        public void RegistersInterceptors(Type interceptorType)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacServiceFabricModule());
            var container = builder.Build();

            Assert.True(container.IsRegistered(interceptorType));
        }
    }
}
