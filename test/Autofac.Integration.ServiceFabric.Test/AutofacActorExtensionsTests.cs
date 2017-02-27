using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
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

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
        }

        [Fact]
        public void GenericRegisterActorAppliesInterceptor()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacServiceFabricModule());
            builder.RegisterActor<Actor1>();

            var container = builder.Build();

            var registration = container.RegistrationFor<Actor1>();
            const string metadataKey = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";
            var interceptorServices = (IEnumerable<Service>)registration.Metadata[metadataKey];
            Assert.Contains(new TypedService(typeof(AutofacActorInterceptor)), interceptorServices);
        }

        [Fact]
        public void GenericRegisterActorRegistersInstancePerLifetimeScope()
        {
            var builder = new ContainerBuilder();
            builder.RegisterActor<Actor1>();

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

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
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

        [Fact]
        public void RegisterActorsRegistersAllActorTypesInAssembly()
        {
            var builder = new ContainerBuilder();
            builder.RegisterActors(Assembly.GetExecutingAssembly());

            var container = builder.Build();

            container.AssertRegistered<Actor1>();
            container.AssertRegistered<Actor2>();
        }
    }

    public class Actor1 : Actor
    {
        public Actor1(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Actor2 : Actor
    {
        public Actor2(ActorService actorService, ActorId actorId) : base(actorService, actorId)
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
