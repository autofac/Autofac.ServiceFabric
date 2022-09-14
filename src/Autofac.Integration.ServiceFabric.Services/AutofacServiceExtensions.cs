// This software is part of the Autofac IoC container
// Copyright © 2017 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Autofac.Builder;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Autofac.Integration.ServiceFabric.Services
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
                object? lifetimeScopeTag = null)
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
                object? lifetimeScopeTag = null)
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
                object? lifetimeScopeTag = null)
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
