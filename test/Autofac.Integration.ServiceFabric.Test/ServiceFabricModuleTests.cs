using System;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Test
{
    public sealed class ServiceFabricModuleTests
    {
        [Theory]
        [InlineData(typeof(ActorInterceptor))]
        [InlineData(typeof(ServiceInterceptor))]
        [InlineData(typeof(IActorFactoryRegistration))]
        [InlineData(typeof(IStatefulServiceFactoryRegistration))]
        [InlineData(typeof(IStatelessServiceFactoryRegistration))]
        public void RegistersRequiredSupportTypes(Type interceptorType)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ServiceFabricModule());
            var container = builder.Build();

            Assert.True(container.IsRegistered(interceptorType));
        }
    }
}
