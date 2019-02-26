using System;
using System.Collections.Generic;
using System.Fabric;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using Moq;
using Test.Scenario.InternalsVisible;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Test
{
    public sealed class AutofacServiceExtensionsTests
    {
        [Fact]
        public void RegisterStatefulServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("ServiceType");
            builder.RegisterInstance(new Mock<IStatefulServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<StatefulService1>();
        }

        [Fact]
        public void RegisterStatelessServiceRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("ServiceType");
            builder.RegisterInstance(new Mock<IStatelessServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<StatelessService1>();
        }

        [Fact]
        public void RegisterStatefulServiceAppliesInterceptor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("ServiceType");
            builder.RegisterServiceFabricSupport();
            builder.RegisterInstance(new Mock<IStatefulServiceFactoryRegistration>().Object);

            var container = builder.Build();

            var registration = container.RegistrationFor<StatefulService1>();
            const string metadataKey = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";
            var interceptorServices = (IEnumerable<Service>)registration.Metadata[metadataKey];
            Assert.Contains(new TypedService(typeof(ServiceInterceptor)), interceptorServices);
        }

        [Fact]
        public void RegisterStatelessServiceAppliesInterceptor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("ServiceType");
            builder.RegisterServiceFabricSupport();
            builder.RegisterInstance(new Mock<IStatelessServiceFactoryRegistration>().Object);

            var container = builder.Build();

            var registration = container.RegistrationFor<StatelessService1>();
            const string metadataKey = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";
            var interceptorServices = (IEnumerable<Service>)registration.Metadata[metadataKey];
            Assert.Contains(new TypedService(typeof(ServiceInterceptor)), interceptorServices);
        }

        [Fact]
        public void RegisterStatefulServiceWithoutTagRegistersInstancePerMatchingLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("ServiceType");
            builder.RegisterInstance(new Mock<IStatefulServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertSharing<StatefulService1>(InstanceSharing.Shared);
            container.AssertLifetime<StatefulService1, MatchingScopeLifetime>();
            container.AssertOwnership<StatefulService1>(InstanceOwnership.OwnedByLifetimeScope);

            var lifetime = (MatchingScopeLifetime)container.RegistrationFor<StatefulService1>().Lifetime;
            Assert.Contains(Constants.DefaultLifetimeScopeTag, lifetime.TagsToMatch);
        }

        [Fact]
        public void RegisterStatelessServiceWithoutTagRegistersInstancePerMatchingLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("ServiceType");
            builder.RegisterInstance(new Mock<IStatelessServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertSharing<StatelessService1>(InstanceSharing.Shared);
            container.AssertLifetime<StatelessService1, MatchingScopeLifetime>();
            container.AssertOwnership<StatelessService1>(InstanceOwnership.OwnedByLifetimeScope);

            var lifetime = (MatchingScopeLifetime)container.RegistrationFor<StatelessService1>().Lifetime;
            Assert.Contains(Constants.DefaultLifetimeScopeTag, lifetime.TagsToMatch);
        }

        [Fact]
        public void RegisterStatefulServiceWithTagRegistersInstancePerMatchingLifetimeScope()
        {
            var builder = new ContainerBuilder();
            const string lifetimeScopeTag = "Tag";
            builder.RegisterStatefulService<StatefulService1>("ServiceType", lifetimeScopeTag);
            builder.RegisterInstance(new Mock<IStatefulServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertSharing<StatefulService1>(InstanceSharing.Shared);
            container.AssertLifetime<StatefulService1, MatchingScopeLifetime>();
            container.AssertOwnership<StatefulService1>(InstanceOwnership.OwnedByLifetimeScope);

            var lifetime = (MatchingScopeLifetime)container.RegistrationFor<StatefulService1>().Lifetime;
            Assert.Contains(lifetimeScopeTag, lifetime.TagsToMatch);
        }

        [Fact]
        public void RegisterStatelessServiceWithTagRegistersInstancePerMatchingLifetimeScope()
        {
            var builder = new ContainerBuilder();
            const string lifetimeScopeTag = "Tag";
            builder.RegisterStatelessService<StatelessService1>("ServiceType", lifetimeScopeTag);
            builder.RegisterInstance(new Mock<IStatelessServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertSharing<StatelessService1>(InstanceSharing.Shared);
            container.AssertLifetime<StatelessService1, MatchingScopeLifetime>();
            container.AssertOwnership<StatelessService1>(InstanceOwnership.OwnedByLifetimeScope);

            var lifetime = (MatchingScopeLifetime)container.RegistrationFor<StatelessService1>().Lifetime;
            Assert.Contains(lifetimeScopeTag, lifetime.TagsToMatch);
        }

        [Fact]
        public void RegisterStatefulServiceAddsFactoryCallback()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("ServiceType");
            var factoryMock = new Mock<IStatefulServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            factoryMock.Verify(x => x.RegisterStatefulServiceFactory<StatefulService1>(container, "ServiceType", null), Times.Once);
        }

        [Fact]
        public void RegisterStatelessServiceAddsFactoryCallback()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("ServiceType");
            var factoryMock = new Mock<IStatelessServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            factoryMock.Verify(x => x.RegisterStatelessServiceFactory<StatelessService1>(container, "ServiceType", null), Times.Once);
        }

        [Fact]
        public void RegisterStatefulServiceCanBeCalledWithLifetimeScopeTag()
        {
            var builder = new ContainerBuilder();
            const string lifetimeScopeTag = "CustomTag";
            builder.RegisterStatefulService<StatefulService1>("ServiceType", lifetimeScopeTag);
            var factoryMock = new Mock<IStatefulServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            container.AssertRegistered<StatefulService1>();
            factoryMock.Verify(x => x.RegisterStatefulServiceFactory<StatefulService1>(container, "ServiceType", lifetimeScopeTag), Times.Once);
        }

        [Fact]
        public void RegisterStatelessServiceCanBeCalledWithLifetimeScopeTag()
        {
            var builder = new ContainerBuilder();
            const string lifetimeScopeTag = "CustomTag";
            builder.RegisterStatelessService<StatelessService1>("ServiceType", lifetimeScopeTag);
            var factoryMock = new Mock<IStatelessServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            container.AssertRegistered<StatelessService1>();
            factoryMock.Verify(x => x.RegisterStatelessServiceFactory<StatelessService1>(container, "ServiceType", lifetimeScopeTag), Times.Once);
        }

        [Fact]
        public void RegisterStatefulServiceSupportsInternalsVisibleToDynamicProxyGenAssembly2()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<InternalsVisibleStatefulService>("ServiceType");
            builder.RegisterInstance(new Mock<IStatefulServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<InternalsVisibleStatefulService>();
        }

        [Fact]
        public void RegisterStatelessServiceSupportsInternalsVisibleToDynamicProxyGenAssembly2()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<InternalsVisibleStatelessService>("ServiceType");
            builder.RegisterInstance(new Mock<IStatelessServiceFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<InternalsVisibleStatelessService>();
        }

        [Fact]
        public void RegisterStatefulServiceCanBeCalledFromModuleLoad()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new StatefulServiceModule());
            var factoryMock = new Mock<IStatefulServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            container.AssertRegistered<StatefulService1>();
            factoryMock.Verify(x => x.RegisterStatefulServiceFactory<StatefulService1>(container, "serviceTypeName", null), Times.Once);
        }

        [Fact]
        public void RegisterStatelessServiceCanBeCalledFromModuleLoad()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new StatelessServiceModule());
            var factoryMock = new Mock<IStatelessServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            container.AssertRegistered<StatelessService1>();
            factoryMock.Verify(x => x.RegisterStatelessServiceFactory<StatelessService1>(container, "serviceTypeName", null), Times.Once);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedTypeIsNotPublic()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterStatefulService<InternalStatefulService>("ServiceType"));

            Assert.Equal(typeof(InternalStatefulService).GetInvalidProxyTypeErrorMessage(), exception.Message);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedTypeIsNotPublic()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterStatelessService<InternalStatelessService>("ServiceType"));

            Assert.Equal(typeof(InternalStatelessService).GetInvalidProxyTypeErrorMessage(), exception.Message);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedTypeIsSealed()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterStatefulService<SealedStatefulService>("ServiceType"));

            Assert.Equal(typeof(SealedStatefulService).GetInvalidProxyTypeErrorMessage(), exception.Message);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedTypeIsSealed()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterStatelessService<SealedStatelessService>("ServiceType"));

            Assert.Equal(typeof(SealedStatelessService).GetInvalidProxyTypeErrorMessage(), exception.Message);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedBuilderIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                    () => AutofacServiceExtensions.RegisterStatefulService<StatefulService1>(null, "ServiceType"));

            Assert.Equal("builder", exception.ParamName);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedBuilderIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => AutofacServiceExtensions.RegisterStatelessService<StatelessService1>(null, "ServiceType"));

            Assert.Equal("builder", exception.ParamName);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedServiceTypeNameIsNull()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterStatefulService<StatefulService1>(null));

            Assert.Equal("serviceTypeName", exception.ParamName);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedServiceTypeNameIsNull()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterStatelessService<StatelessService1>(null));

            Assert.Equal("serviceTypeName", exception.ParamName);
        }

        [Fact]
        public void RegisterStatefulServiceThrowsIfProvidedServiceTypeNameIsEmpty()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterStatefulService<StatefulService1>(string.Empty));

            Assert.Equal("serviceTypeName", exception.ParamName);
        }

        [Fact]
        public void RegisterStatelessServiceThrowsIfProvidedServiceTypeNameIsEmpty()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterStatelessService<StatelessService1>(string.Empty));

            Assert.Equal("serviceTypeName", exception.ParamName);
        }

        [Fact]
        public void ContainerBuildThrowsIfRegisterStatefulServiceLifetimeScopeChangedToInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("serviceTypeName").InstancePerDependency();
            var factoryMock = new Mock<IStatefulServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

            Assert.Equal(typeof(StatefulService1).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(), exception.Message);
        }

        [Fact]
        public void ContainerBuildThrowsIfRegisterStatelessServiceLifetimeScopeChangedToInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("serviceTypeName").InstancePerDependency();
            var factoryMock = new Mock<IStatelessServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

            Assert.Equal(typeof(StatelessService1).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(), exception.Message);
        }

        [Fact]
        public void ContainerBuildThrowsIfRegisterStatefulServiceLifetimeScopeChangedToSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("serviceTypeName").SingleInstance();
            var factoryMock = new Mock<IStatefulServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

            Assert.Equal(typeof(StatefulService1).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(), exception.Message);
        }

        [Fact]
        public void ContainerBuildThrowsIfRegisterStatelessServiceLifetimeScopeChangedToSingleInstance()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("serviceTypeName").SingleInstance();
            var factoryMock = new Mock<IStatelessServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

            Assert.Equal(typeof(StatelessService1).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(), exception.Message);
        }

        [Fact]
        public void ContainerBuildThrowsIfRegisterStatefulServiceLifetimeScopeChangedToExternallyOwned()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatefulService<StatefulService1>("serviceTypeName").ExternallyOwned();
            var factoryMock = new Mock<IStatefulServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

            Assert.Equal(typeof(StatefulService1).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(), exception.Message);
        }

        [Fact]
        public void ContainerBuildThrowsIfRegisterStatelessServiceLifetimeScopeChangedToExternallyOwned()
        {
            var builder = new ContainerBuilder();
            builder.RegisterStatelessService<StatelessService1>("serviceTypeName").ExternallyOwned();
            var factoryMock = new Mock<IStatelessServiceFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

            Assert.Equal(typeof(StatelessService1).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(), exception.Message);
        }
    }

    public class StatefulServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterStatefulService<StatefulService1>("serviceTypeName");
        }
    }

    public class StatelessServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterStatelessService<StatelessService1>("serviceTypeName");
        }
    }

    public class StatefulService1 : StatefulServiceBase
    {
        public StatefulService1(StatefulServiceContext serviceContext, IStateProviderReplica2 stateProviderReplica)
            : base(serviceContext, stateProviderReplica)
        {
        }
    }

    public sealed class SealedStatefulService : StatefulServiceBase
    {
        public SealedStatefulService(StatefulServiceContext serviceContext, IStateProviderReplica2 stateProviderReplica)
            : base(serviceContext, stateProviderReplica)
        {
        }
    }

    internal class InternalStatefulService : StatefulServiceBase
    {
        public InternalStatefulService(StatefulServiceContext serviceContext, IStateProviderReplica2 stateProviderReplica)
            : base(serviceContext, stateProviderReplica)
        {
        }
    }

    public class StatelessService1 : StatelessService
    {
        public StatelessService1(StatelessServiceContext serviceContext)
            : base(serviceContext)
        {
        }
    }

    public sealed class SealedStatelessService : StatelessService
    {
        public SealedStatelessService(StatelessServiceContext serviceContext)
            : base(serviceContext)
        {
        }
    }

    internal class InternalStatelessService : StatelessService
    {
        public InternalStatelessService(StatelessServiceContext serviceContext)
            : base(serviceContext)
        {
        }
    }
}
