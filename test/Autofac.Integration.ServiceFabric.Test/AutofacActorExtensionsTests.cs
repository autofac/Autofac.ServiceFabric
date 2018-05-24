using System;
using System.Collections.Generic;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Moq;
using Test.Scenario.InternalsVisible;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Test
{
    public sealed class AutofacActorExtensionsTests
    {
        [Fact]
        public void RegisterActorRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterActor<Actor1>();
            builder.RegisterInstance(new Mock<IActorFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
        }

        [Fact]
        public void RegisterActorAppliesInterceptor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ServiceFabricModule());
            builder.RegisterInstance(new Mock<IActorFactoryRegistration>().Object);
            builder.RegisterActor<Actor1>();

            var container = builder.Build();

            var registration = container.RegistrationFor<Actor1>();
            const string metadataKey = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";
            var interceptorServices = (IEnumerable<Service>)registration.Metadata[metadataKey];
            Assert.Contains(new TypedService(typeof(ActorInterceptor)), interceptorServices);
        }

        [Fact]
        public void RegisterActorRegistersInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterActor<Actor1>();
            builder.RegisterInstance(new Mock<IActorFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertSharing<Actor1>(InstanceSharing.Shared);
            container.AssertLifetime<Actor1, CurrentScopeLifetime>();
            container.AssertOwnership<Actor1>(InstanceOwnership.OwnedByLifetimeScope);
        }

        [Fact]
        public void RegisterActorAddsFactoryCallback()
        {
            var builder = new ContainerBuilder();
            builder.RegisterActor<Actor1>();
            var factoryMock = new Mock<IActorFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            factoryMock.Verify(x => x.RegisterActorFactory<Actor1>(container, null, null, null, null), Times.Once);
        }

        [Fact]
        public void RegisterActorSupportsInternalsVisibleToDynamicProxyGenAssembly2()
        {
            var builder = new ContainerBuilder();
            builder.RegisterActor<InternalsVisibleActor>();
            builder.RegisterInstance(new Mock<IActorFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<InternalsVisibleActor>();
        }

        [Fact]
        public void RegisterActorCanBeCalledFromModuleLoad()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ActorModule());
            var factoryMock = new Mock<IActorFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
            factoryMock.Verify(x => x.RegisterActorFactory<Actor1>(container, null, null, null, null), Times.Once);
        }

        [Fact]
        public void RegisterActorCanBeCalledWithStateManagerFactory()
        {
            var builder = new ContainerBuilder();

            // ReSharper disable once ConvertToLocalFunction
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = (actor, provider) => null;
            builder.RegisterActor<Actor1>(stateManagerFactory: stateManagerFactory);
            var factoryMock = new Mock<IActorFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
            factoryMock.Verify(x => x.RegisterActorFactory<Actor1>(container, stateManagerFactory, null, null, null), Times.Once);
        }

        [Fact]
        public void RegisterActorCanBeCalledWithStateProvider()
        {
            var builder = new ContainerBuilder();
            var stateProvider = new Mock<IActorStateProvider>().Object;
            builder.RegisterActor<Actor1>(stateProvider: stateProvider);
            var factoryMock = new Mock<IActorFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
            factoryMock.Verify(x => x.RegisterActorFactory<Actor1>(container, null, stateProvider, null, null), Times.Once);
        }

        [Fact]
        public void RegisterActorCanBeCalledWithSettings()
        {
            var builder = new ContainerBuilder();
            var settings = new ActorServiceSettings();
            builder.RegisterActor<Actor1>(settings: settings);
            var factoryMock = new Mock<IActorFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
            factoryMock.Verify(x => x.RegisterActorFactory<Actor1>(container, null, null, settings, null), Times.Once);
        }

        [Fact]
        public void RegisterActorThrowsIfProvidedBuilderIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => AutofacActorExtensions.RegisterActor<Actor1>(null));

            Assert.Equal("builder", exception.ParamName);
        }

        [Fact]
        public void RegisterActorThrowsIfProvidedTypeIsSealed()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterActor<SealedActor>());

            Assert.Equal(typeof(SealedActor).GetInvalidProxyTypeErrorMessage(), exception.Message);
        }

        [Fact]
        public void RegisterActorThrowsIfProvidedTypeIsNotPublic()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterActor<InternalActor>());

            Assert.Equal(typeof(InternalActor).GetInvalidProxyTypeErrorMessage(), exception.Message);
        }

        [Fact]
        public void ContainerBuildThrowsIfRegisterActorLifetimeScopeChangedToInstancePerDependency()
        {
            var builder = new ContainerBuilder();
            var factoryMock = new Mock<IActorFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);
            builder.RegisterActor<Actor1>().InstancePerDependency();

            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

            Assert.Equal(typeof(Actor1).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(), exception.Message);
        }

        [Fact]
        public void ContainerBuildThrowsIfRegisterActorLifetimeScopeChangedToSingleInstance()
        {
            var builder = new ContainerBuilder();
            var factoryMock = new Mock<IActorFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);
            builder.RegisterActor<Actor1>().SingleInstance();

            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

            Assert.Equal(typeof(Actor1).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(), exception.Message);
        }

        [Fact]
        public void ContainerBuildThrowsIfRegisterActorLifetimeScopeChangedToExternallyOwned()
        {
            var builder = new ContainerBuilder();
            var factoryMock = new Mock<IActorFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);
            builder.RegisterActor<Actor1>().ExternallyOwned();

            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

            Assert.Equal(typeof(Actor1).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(), exception.Message);
        }

        [Fact]
        public void RegisterActorThrowsIfProvidedActorServiceIsNotInheritsFromActorService()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterActor<Actor1>(actorServiceType: typeof(WrongActorService)));

            Assert.Equal(typeof(WrongActorService).GetInvalidActorServiceTypeErrorMessage(), exception.Message);
        }
    }

    public class WrongActorService
    {
    }

    public class ActorModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterActor<Actor1>();
        }
    }

    public class Actor1 : Actor
    {
        public Actor1(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }
    }

    public sealed class SealedActor : Actor
    {
        public SealedActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }
    }

    internal class InternalActor : Actor
    {
        public InternalActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }
    }
}
