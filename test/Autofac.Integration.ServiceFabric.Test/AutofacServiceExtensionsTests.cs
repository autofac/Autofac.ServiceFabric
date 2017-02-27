using System;
using System.Collections.Generic;
using System.Fabric;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Test
{
    public sealed class AutofacServiceExtensionsTests
    {
        [Fact]
        public void GenericRegisterStatefulServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>();

            var container = builder.Build();

            container.AssertRegistered<StatefulService1>();
        }

        [Fact]
        public void GenericRegisterStatelessServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>();

            var container = builder.Build();

            container.AssertRegistered<StatelessService1>();
        }

        [Fact]
        public void GenericRegisterStatefulServiceAppliesInterceptor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacServiceFabricModule());
            builder.RegisterStatefulService<StatefulService1>();

            var container = builder.Build();

            var registration = container.RegistrationFor<StatefulService1>();
            const string metadataKey = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";
            var interceptorServices = (IEnumerable<Service>)registration.Metadata[metadataKey];
            Assert.Contains(new TypedService(typeof(AutofacServiceInterceptor)), interceptorServices);
        }

        [Fact]
        public void GenericRegisterStatelessServiceAppliesInterceptor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacServiceFabricModule());
            builder.RegisterStatelessService<StatelessService1>();

            var container = builder.Build();

            var registration = container.RegistrationFor<StatelessService1>();
            const string metadataKey = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";
            var interceptorServices = (IEnumerable<Service>)registration.Metadata[metadataKey];
            Assert.Contains(new TypedService(typeof(AutofacServiceInterceptor)), interceptorServices);
        }

        [Fact]
        public void GenericRegisterStatefulServiceRegistersInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>();

            var container = builder.Build();

            container.AssertSharing<StatefulService1>(InstanceSharing.Shared);
            container.AssertLifetime<StatefulService1, CurrentScopeLifetime>();
            container.AssertOwnership<StatefulService1>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void GenericRegisterStatelessServiceRegistersInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>();

            var container = builder.Build();

            container.AssertSharing<StatelessService1>(InstanceSharing.Shared);
            container.AssertLifetime<StatelessService1, CurrentScopeLifetime>();
            container.AssertOwnership<StatelessService1>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void RegisterStatefulServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService(typeof(StatefulService1));

            var container = builder.Build();

            container.AssertRegistered<StatefulService1>();
        }

        [Fact]
        public void RegisterStatelessServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService(typeof(StatelessService1));

            var container = builder.Build();

            container.AssertRegistered<StatelessService1>();
        }

        [Theory]
        [InlineData(typeof(NonDerivedService))]
        [InlineData(typeof(SealedStatefulService))]
        [InlineData(typeof(InternalStatefulService))]
        [InlineData(typeof(IInterfaceOnlyService))]
        public void RegisterStatefulServiceThrowsIfProvidedTypeIsNotStatefulService(Type serviceType)
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterStatefulService(serviceType));

            Assert.Equal("serviceType", exception.ParamName);
        }

        [Theory]
        [InlineData(typeof(NonDerivedService))]
        [InlineData(typeof(SealedStatelessService))]
        [InlineData(typeof(InternalStatelessService))]
        [InlineData(typeof(IInterfaceOnlyService))]
        public void RegisterStatelessServiceThrowsIfProvidedTypeIsNotStatelessService(Type serviceType)
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterStatelessService(serviceType));

            Assert.Equal("serviceType", exception.ParamName);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedTypeIsNull()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterStatefulService(null));

            Assert.Equal("serviceType", exception.ParamName);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedTypeIsNull()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterStatelessService(null));

            Assert.Equal("serviceType", exception.ParamName);
        }

        [Fact]
        public void RegisterStatefulServicesRegistersAllStatefulServiceTypesInAssembly()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulServices(Assembly.GetExecutingAssembly());

            var container = builder.Build();

            container.AssertRegistered<StatefulService1>();
            container.AssertRegistered<StatefulService2>();
        }

        [Fact]
        public void RegisterStatelessServicesRegistersAllStatelessServiceTypesInAssembly()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessServices(Assembly.GetExecutingAssembly());

            var container = builder.Build();

            container.AssertRegistered<StatelessService1>();
            container.AssertRegistered<StatelessService2>();
        }
    }

    public class StatefulService1 : StatefulServiceBase
    {
        public StatefulService1(StatefulServiceContext serviceContext, IStateProviderReplica stateProviderReplica) : base(serviceContext, stateProviderReplica)
        {
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class StatefulService2 : StatefulServiceBase
    {
        public StatefulService2(StatefulServiceContext serviceContext, IStateProviderReplica stateProviderReplica) : base(serviceContext, stateProviderReplica)
        {
        }
    }

    public sealed class SealedStatefulService : StatefulServiceBase
    {
        public SealedStatefulService(StatefulServiceContext serviceContext, IStateProviderReplica stateProviderReplica) : base(serviceContext, stateProviderReplica)
        {
        }
    }

    internal class InternalStatefulService : StatefulServiceBase
    {
        public InternalStatefulService(StatefulServiceContext serviceContext, IStateProviderReplica stateProviderReplica) : base(serviceContext, stateProviderReplica)
        {
        }
    }

    public class StatelessService1 : StatelessService
    {
        public StatelessService1(StatelessServiceContext serviceContext) : base(serviceContext)
        {
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class StatelessService2 : StatelessService
    {
        public StatelessService2(StatelessServiceContext serviceContext) : base(serviceContext)
        {
        }
    }

    public sealed class SealedStatelessService : StatelessService
    {
        public SealedStatelessService(StatelessServiceContext serviceContext) : base(serviceContext)
        {
        }
    }

    internal class InternalStatelessService : StatelessService
    {
        public InternalStatelessService(StatelessServiceContext serviceContext) : base(serviceContext)
        {
        }
    }

    public class NonDerivedService
    {
    }

    public interface IInterfaceOnlyService
    {
    }
}
