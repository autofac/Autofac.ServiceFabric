using System;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Actors.Test
{
    public sealed class ServiceFabricModuleTests
    {
        [Theory]
        [InlineData(typeof(ActorInterceptor))]
        [InlineData(typeof(IActorFactoryRegistration))]
        public void RegistersRequiredSupportTypes(Type interceptorType)
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceFabricSupport();
            var container = builder.Build();

            Assert.True(container.IsRegistered(interceptorType));
        }

        [Fact]
        public void ActorFactoryRegistrationReceivesProvidedConstructorExceptionCallbackParameter()
        {
            var builder = new ContainerBuilder();
            Exception capturedException = null;
            void ConstructorExceptionCallback(Exception ex) => capturedException = ex;
            builder.RegisterServiceFabricSupport(ConstructorExceptionCallback);
            var container = builder.Build();

            var factoryRegistration = (ActorFactoryRegistration)container.Resolve<IActorFactoryRegistration>();
            var thrownException = new Exception("Failed to construct instance");
            factoryRegistration.ConstructorExceptionCallback(thrownException);

            Assert.Same(thrownException, capturedException);
        }

        [Fact]
        public void ActorFactoryRegistrationReceivesDefaultConstructorExceptionCallbackParameter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceFabricSupport();
            var container = builder.Build();

            var factoryRegistration = (ActorFactoryRegistration)container.Resolve<IActorFactoryRegistration>();
            Assert.NotNull(factoryRegistration.ConstructorExceptionCallback);
        }

        [Fact]
        public void ActorFactoryRegistrationReceivesProvidedConfigurationActionParameter()
        {
            var builder = new ContainerBuilder();
            var invoked = false;
            builder.RegisterServiceFabricSupport(configurationAction: _ => invoked = true);
            var container = builder.Build();

            var factoryRegistration = (ActorFactoryRegistration)container.Resolve<IActorFactoryRegistration>();
            factoryRegistration.ConfigurationAction(new ContainerBuilder());

            Assert.True(invoked);
        }

        [Fact]
        public void ActorFactoryRegistrationReceivesDefaultConfigurationActionParameter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceFabricSupport();
            var container = builder.Build();

            var factoryRegistration = (ActorFactoryRegistration)container.Resolve<IActorFactoryRegistration>();
            Assert.NotNull(factoryRegistration.ConfigurationAction);
        }
    }
}
