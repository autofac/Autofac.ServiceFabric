// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Autofac.Integration.ServiceFabric
{
    /// <summary>
    /// Extension methods for registering services (stateful and stateless) with Autofac and Service Fabric.
    /// </summary>
    public static class AutofacServiceExtensions
    {
        /// <summary>
        /// Registers a stateful service with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="serviceTypeName">ServiceTypeName as provided in service manifest.</param>
        /// <param name="lifetimeScopeTag">The tag applied to the <see cref="ILifetimeScope"/> in which the stateful service is hosted.</param>
        /// <typeparam name="TService">The type of the stateful service to register.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="ArgumentException">Thrown when <typeparamref name="TService"/> is not a valid service type.</exception>
        /// <remarks>The service will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterStatefulService<TService>(
                this ContainerBuilder builder,
                string serviceTypeName,
                object lifetimeScopeTag = null)
            where TService : StatefulServiceBase
        {
            var registration = RegisterServiceWithContainer<TService>(builder, serviceTypeName, lifetimeScopeTag);

            builder.RegisterBuildCallback(l =>
                l.Resolve<IStatefulServiceFactoryRegistration>()
                    .RegisterStatefulServiceFactory<TService>(l, serviceTypeName, lifetimeScopeTag));

            return registration;
        }

        /// <summary>
        /// Registers a stateless service with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="serviceTypeName">ServiceTypeName as provided in service manifest.</param>
        /// <param name="lifetimeScopeTag">The tag applied to the <see cref="ILifetimeScope"/> in which the stateless service is hosted.</param>
        /// <typeparam name="TService">The type of the stateless service to register.</typeparam>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="ArgumentException">Thrown when <typeparamref name="TService"/> is not a valid service type.</exception>
        /// <remarks>The service will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterStatelessService<TService>(
                this ContainerBuilder builder,
                string serviceTypeName,
                object lifetimeScopeTag = null)
            where TService : StatelessService
        {
            var registration = RegisterServiceWithContainer<TService>(builder, serviceTypeName, lifetimeScopeTag);

            builder.RegisterBuildCallback(l =>
                l.Resolve<IStatelessServiceFactoryRegistration>()
                    .RegisterStatelessServiceFactory<TService>(l, serviceTypeName, lifetimeScopeTag));

            return registration;
        }

        private static IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterServiceWithContainer<TService>(
                ContainerBuilder builder,
                string serviceTypeName,
                object lifetimeScopeTag = null)
            where TService : class
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (string.IsNullOrEmpty(serviceTypeName))
                throw new ArgumentException(AutofacServiceExtensionsResources.MissingServiceTypeNameErrorMessage, nameof(serviceTypeName));

            var serviceType = typeof(TService);

            if (!serviceType.CanBeProxied())
                throw new ArgumentException(serviceType.GetInvalidProxyTypeErrorMessage());

            var registration = builder.RegisterServiceWithInterception<TService, ServiceInterceptor>(lifetimeScopeTag);

            registration.EnsureRegistrationIsInstancePerLifetimeScope();

            return registration;
        }
    }
}
