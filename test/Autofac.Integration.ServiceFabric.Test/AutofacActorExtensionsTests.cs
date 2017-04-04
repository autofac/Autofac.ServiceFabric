using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Moq;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Test
{
    public sealed class AutofacActorExtensionsTests
    {
        [Fact]
        public void GenericRegisterActorRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterActor<Actor1>();
            builder.RegisterInstance(new Mock<IActorFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
        }

        [Fact]
        public void GenericRegisterActorAppliesInterceptor()
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
        public void GenericRegisterActorRegistersInstancePerLifetimeScope()
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
        public void RegisterActorRegistersProvidedType()
        {
            var builder = new ContainerBuilder();
            builder.RegisterActor(typeof(Actor1));
            builder.RegisterInstance(new Mock<IActorFactoryRegistration>().Object);

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
        }

        [Fact]
        public void RegisterActorAddsFactoryCallback()
        {
            var builder = new ContainerBuilder();
            builder.RegisterActor(typeof(Actor1));
            var factoryMock = new Mock<IActorFactoryRegistration>();
            builder.RegisterInstance(factoryMock.Object);

            var container = builder.Build();

            factoryMock.Verify(x => x.RegisterActorFactory(typeof(Actor1), container), Times.Once);
        }

        [Fact]
        public void RegisterActorThrowsIfProvidedBuilderIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => AutofacActorExtensions.RegisterActor(null, typeof(Actor1)));

            Assert.Equal("builder", exception.ParamName);
        }

        [Fact]
        public void RegisterActorThrowsIfProvidedTypeIsNull()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterActor(null));

            Assert.Equal("actorType", exception.ParamName);
        }

        [Theory]
        [InlineData(typeof(NonDerivedActor))]
        [InlineData(typeof(SealedActor))]
        [InlineData(typeof(InternalActor))]
        [InlineData(typeof(IInterfaceOnlyActor))]
        public void RegisterActorThrowsIfProvidedTypeIsNotActor(Type actorType)
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(() => builder.RegisterActor(actorType));

            Assert.Equal("actorType", exception.ParamName);
        }
    }

    public class Actor1 : Actor
    {
        public Actor1(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }
    }

    public sealed class SealedActor : Actor
    {
        public SealedActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }
    }

    internal class InternalActor : Actor
    {
        public InternalActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }
    }

    public class NonDerivedActor
    {
    }

    public interface IInterfaceOnlyActor
    {
    }
}
