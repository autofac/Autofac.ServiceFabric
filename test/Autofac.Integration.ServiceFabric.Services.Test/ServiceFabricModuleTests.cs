﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Services.Test
{
    public sealed class ServiceFabricModuleTests
    {
        [Theory]
        [InlineData(typeof(ServiceInterceptor))]
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
