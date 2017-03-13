using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Autofac.Integration.ServiceFabric.Test
{
    public sealed class RegistrationExtensionsTests
    {
        [Fact]
        public void RegisterServiceFabricSupportOnlyAddsModuleOnce()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceFabricSupport();
            builder.RegisterServiceFabricSupport();

            var container = builder.Build();

            var actorInterceptors = container.Resolve<IEnumerable<AutofacActorInterceptor>>().ToArray();
            Assert.Equal(1, actorInterceptors.Length);

            var serviceInterceptors = container.Resolve<IEnumerable<AutofacServiceInterceptor>>().ToArray();
            Assert.Equal(1, serviceInterceptors.Length);
        }

        [Fact]
        public void RegisterServiceFabricSupportThrowsWhenContainerBuilderIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => RegistrationExtensions.RegisterServiceFabricSupport(null));

            Assert.Equal("builder", exception.ParamName);
        }
    }
}
