using System;
using System.Collections.Generic;
using System.Fabric;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using Moq;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Test
{
    public sealed class AutofacServiceExtensionsTests
    {
        [Fact]
        public void GenericRegisterStatefulServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("ServiceType");
            builder.RegisterInstance(new Mock<IStatefulServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<StatefulService1>();
        }

        [Fact]
        public void GenericRegisterStatelessServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("ServiceType");
            builder.RegisterInstance(new Mock<IStatelessServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<StatelessService1>();
        }

        [Fact]
        public void GenericRegisterStatefulServiceAppliesInterceptor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("ServiceType");
            builder.RegisterModule(new ServiceFabricModule());
            builder.RegisterInstance(new Mock<IStatefulServiceFactoryRegistration>().Object);

            var container = builder.Build();

            var registration = container.RegistrationFor<StatefulService1>();
            const string metadataKey = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";
            var interceptorServices = (IEnumerable<Service>)registration.Metadata[metadataKey];
            Assert.Contains(new TypedService(typeof(ServiceInterceptor)), interceptorServices);
        }

        [Fact]
        public void GenericRegisterStatelessServiceAppliesInterceptor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("ServiceType");
            builder.RegisterModule(new ServiceFabricModule());
            builder.RegisterInstance(new Mock<IStatelessServiceFactoryRegistration>().Object);

            var container = builder.Build();

            var registration = container.RegistrationFor<StatelessService1>();
            const string metadataKey = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";
            var interceptorServices = (IEnumerable<Service>)registration.Metadata[metadataKey];
            Assert.Contains(new TypedService(typeof(ServiceInterceptor)), interceptorServices);
        }

        [Fact]
        public void GenericRegisterStatefulServiceRegistersInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("ServiceType");
            builder.RegisterInstance(new Mock<IStatefulServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertSharing<StatefulService1>(InstanceSharing.Shared);
            container.AssertLifetime<StatefulService1, CurrentScopeLifetime>();
            container.AssertOwnership<StatefulService1>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void GenericRegisterStatelessServiceRegistersInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("ServiceType");
            builder.RegisterInstance(new Mock<IStatelessServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertSharing<StatelessService1>(InstanceSharing.Shared);
            container.AssertLifetime<StatelessService1, CurrentScopeLifetime>();
            container.AssertOwnership<StatelessService1>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void RegisterStatefulServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService(typeof(StatefulService1), "ServiceType");
            builder.RegisterInstance(new Mock<IStatefulServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<StatefulService1>();
        }

        [Fact]
        public void RegisterStatelessServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService(typeof(StatelessService1), "ServiceType");
            builder.RegisterInstance(new Mock<IStatelessServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<StatelessService1>();
        }

        [Fact]
        public void RegisterStatefulServiceAddsFactoryCallback()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService(typeof(StatefulService1), "ServiceType");
            var factoryMock = new Mock<IStatefulServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            factoryMock.Verify(x => x.RegisterStatefulServiceFactory(container, typeof(StatefulService1), "ServiceType"), Times.Once);
        }

        [Fact]
        public void RegisterStatelessServiceAddsFactoryCallback()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService(typeof(StatelessService1), "ServiceType");
            var factoryMock = new Mock<IStatelessServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            factoryMock.Verify(x => x.RegisterStatelessServiceFactory(container, typeof(StatelessService1), "ServiceType"), Times.Once);
        }

        [Theory]
        [InlineData(typeof(NonDerivedService))]
        [InlineData(typeof(SealedStatefulService))]
        [InlineData(typeof(InternalStatefulService))]
        [InlineData(typeof(IInterfaceOnlyService))]
        public void RegisterStatefulServiceThrowsIfProvidedTypeIsNotStatefulService(Type serviceType)
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterStatefulService(serviceType, "ServiceType"));

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

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterStatelessService(serviceType, "ServiceType"));

            Assert.Equal("serviceType", exception.ParamName);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedBuilderIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => AutofacServiceExtensions.RegisterStatefulService(null, typeof(StatefulService1), "ServiceType"));

            Assert.Equal("builder", exception.ParamName);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedBuilderIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => AutofacServiceExtensions.RegisterStatelessService(null, typeof(StatelessService1), "ServiceType"));

            Assert.Equal("builder", exception.ParamName);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedTypeIsNull()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterStatefulService(null, "ServiceType"));

            Assert.Equal("serviceType", exception.ParamName);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedTypeIsNull()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterStatelessService(null, "ServiceType"));

            Assert.Equal("serviceType", exception.ParamName);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedServiceTypeNameIsNull()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterStatefulService(typeof(StatefulService1), null));

            Assert.Equal("serviceTypeName", exception.ParamName);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedServiceTypeNameIsNull()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterStatelessService(typeof(StatelessService1), null));

            Assert.Equal("serviceTypeName", exception.ParamName);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedServiceTypeNameIsEmpty()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterStatefulService(typeof(StatefulService1), string.Empty));

            Assert.Equal("serviceTypeName", exception.ParamName);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedServiceTypeNameIsEmpty()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterStatelessService(typeof(StatelessService1), string.Empty));

            Assert.Equal("serviceTypeName", exception.ParamName);
        }
    }

    public class StatefulService1 : StatefulServiceBase
    {
        public StatefulService1(StatefulServiceContext serviceContext, IStateProviderReplica stateProviderReplica) : base(serviceContext, stateProviderReplica)
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
