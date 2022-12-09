// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        [Fact]
        public void StatefulServiceFactoryRegistrationReceivesProvidedConstructorExceptionCallbackParameter()
        {
            var builder = new ContainerBuilder();
            Exception capturedException = null;
            void ConstructorExceptionCallback(Exception ex) => capturedException = ex;
            builder.RegisterServiceFabricSupport(ConstructorExceptionCallback);
            var container = builder.Build();

            var factoryRegistration = (StatefulServiceFactoryRegistration)container.Resolve<IStatefulServiceFactoryRegistration>();
            var thrownException = new Exception("Failed to construct instance");
            factoryRegistration.ConstructorExceptionCallback(thrownException);

            Assert.Same(thrownException, capturedException);
        }

        [Fact]
        public void StatefulServiceFactoryRegistrationReceivesDefaultConstructorExceptionCallbackParameter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceFabricSupport();
            var container = builder.Build();

            var factoryRegistration = (StatefulServiceFactoryRegistration)container.Resolve<IStatefulServiceFactoryRegistration>();
            Assert.NotNull(factoryRegistration.ConstructorExceptionCallback);
        }

        [Fact]
        public void StatefulServiceFactoryRegistrationReceivesProvidedConfigurationActionParameter()
        {
            var builder = new ContainerBuilder();
            var invoked = false;
            builder.RegisterServiceFabricSupport(configurationAction: _ => invoked = true);
            var container = builder.Build();

            var factoryRegistration = (StatefulServiceFactoryRegistration)container.Resolve<IStatefulServiceFactoryRegistration>();
            factoryRegistration.ConfigurationAction(new ContainerBuilder());

            Assert.True(invoked);
        }

        [Fact]
        public void StatefulServiceFactoryRegistrationReceivesDefaultConfigurationActionParameter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceFabricSupport();
            var container = builder.Build();

            var factoryRegistration = (StatefulServiceFactoryRegistration)container.Resolve<IStatefulServiceFactoryRegistration>();
            Assert.NotNull(factoryRegistration.ConfigurationAction);
        }

        [Fact]
        public void StatelessServiceFactoryRegistrationReceivesProvidedConstructorExceptionCallbackParameter()
        {
            var builder = new ContainerBuilder();
            Exception capturedException = null;
            void ConstructorExceptionCallback(Exception ex) => capturedException = ex;
            builder.RegisterServiceFabricSupport(ConstructorExceptionCallback);
            var container = builder.Build();

            var factoryRegistration = (StatelessServiceFactoryRegistration)container.Resolve<IStatelessServiceFactoryRegistration>();
            var thrownException = new Exception("Failed to construct instance");
            factoryRegistration.ConstructorExceptionCallback(thrownException);

            Assert.Same(thrownException, capturedException);
        }

        [Fact]
        public void StatelessServiceFactoryRegistrationReceivesDefaultConstructorExceptionCallbackParameter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceFabricSupport();
            var container = builder.Build();

            var factoryRegistration = (StatelessServiceFactoryRegistration)container.Resolve<IStatelessServiceFactoryRegistration>();
            Assert.NotNull(factoryRegistration.ConstructorExceptionCallback);
        }

        [Fact]
        public void StatelessServiceFactoryRegistrationReceivesProvidedConfigurationActionParameter()
        {
            var builder = new ContainerBuilder();
            var invoked = false;
            builder.RegisterServiceFabricSupport(configurationAction: _ => invoked = true);
            var container = builder.Build();

            var factoryRegistration = (StatelessServiceFactoryRegistration)container.Resolve<IStatelessServiceFactoryRegistration>();
            factoryRegistration.ConfigurationAction(new ContainerBuilder());

            Assert.True(invoked);
        }

        [Fact]
        public void StatelessServiceFactoryRegistrationReceivesDefaultConfigurationActionParameter()
        {
            var builder = new ContainerBuilder();
            builder.RegisterServiceFabricSupport();
            var container = builder.Build();

            var factoryRegistration = (StatelessServiceFactoryRegistration)container.Resolve<IStatelessServiceFactoryRegistration>();
            Assert.NotNull(factoryRegistration.ConfigurationAction);
        }
    }
}
