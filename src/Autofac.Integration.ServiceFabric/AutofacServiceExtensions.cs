// This software is part of the Autofac IoC container
// Copyright © 2017 Autofac Contributors
// http://autofac.org
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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Extras.DynamicProxy;
using Autofac.Util;
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
        /// <typeparam name="TService">The type of the stateful service to register.</typeparam>
        /// <exception cref="ArgumentException">Thrown when <typeparamref name="TService"/> is not a valid service type.</exception>
        /// <remarks>The service will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterStatefulService<TService>(this ContainerBuilder builder)
            where TService : StatefulServiceBase
        {
            builder.RegisterStatefulService(typeof(TService));
        }

        /// <summary>
        /// Registers a stateful service with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="serviceType">The type of the stateful service to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceType"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is not a valid service type.</exception>
        /// <remarks>The service will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterStatefulService(this ContainerBuilder builder, Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (!serviceType.IsServiceType<StatefulServiceBase>())
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The type {0} is not a valid stateful service type", serviceType.FullName), nameof(serviceType));

            builder.RegisterServiceWithContainer(serviceType);
        }

        /// <summary>
        /// Registers all valid stateful services found in the specified assemblies with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="assemblies">The assemblies to search for stateful services.</param>
        /// <remarks>The service will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterStatefulServices(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterServices<StatefulServiceBase>(assemblies);
        }

        /// <summary>
        /// Registers a stateless service with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <typeparam name="TService">The type of the stateless service to register.</typeparam>
        /// <exception cref="ArgumentException">Thrown when <typeparamref name="TService"/> is not a valid service type.</exception>
        /// <remarks>The service will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterStatelessService<TService>(this ContainerBuilder builder)
            where TService : StatelessService
        {
            builder.RegisterStatelessService(typeof(TService));
        }

        /// <summary>
        /// Registers a stateless service with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="serviceType">The type of the stateless service to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceType"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> is not a valid service type.</exception>
        /// <remarks>The service will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterStatelessService(this ContainerBuilder builder, Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));

            if (!serviceType.IsServiceType<StatelessService>())
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The type {0} is not a valid stateless service type", serviceType.FullName), nameof(serviceType));

            builder.RegisterServiceWithContainer(serviceType);
        }

        /// <summary>
        /// Registers all valid stateless services found in the specified assemblies with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="assemblies">The assemblies to search for stateless services.</param>
        /// <remarks>The service will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterStatelessServices(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            builder.RegisterServices<StatelessService>(assemblies);
        }

        /// <summary>
        /// Registers a stateful service factory with Service Fabric for creating instances of <typeparamref name="TService"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="serviceTypeName">ServiceTypeName as provied in service manifest.</param>
        /// <typeparam name="TService">The type of the stateful service.</typeparam>
        public static void RegisterStatefulServiceFactory<TService>(this IContainer container, string serviceTypeName)
            where TService : StatefulServiceBase
        {
            ServiceRuntime.RegisterServiceAsync(serviceTypeName, context =>
            {
                var lifetimeScope = container.BeginLifetimeScope();
                var service = lifetimeScope.Resolve<TService>(TypedParameter.From(context));
                return service;
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Registers a stateless service factory with Service Fabric for creating instances of <typeparamref name="TService"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="serviceTypeName">ServiceTypeName as provied in service manifest.</param>
        /// <typeparam name="TService">The type of the stateless service.</typeparam>
        public static void RegisterStatelessServiceFactory<TService>(this IContainer container, string serviceTypeName)
            where TService : StatelessService
        {
            ServiceRuntime.RegisterServiceAsync(serviceTypeName, context =>
            {
                var lifetimeScope = container.BeginLifetimeScope();
                var service = lifetimeScope.Resolve<TService>(TypedParameter.From(context));
                return service;
            }).GetAwaiter().GetResult();
        }

        private static bool IsServiceType<TServiceBase>(this Type type)
        {
            return type.IsAssignableTo<TServiceBase>()
                && type.IsClass
                && type.IsPublic
                && !type.IsSealed
                && !type.IsAbstract;
        }

        private static void RegisterServiceWithContainer(this ContainerBuilder builder, Type serviceType)
        {
            builder.RegisterType(serviceType)
                .InstancePerLifetimeScope()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(AutofacServiceInterceptor));
        }

        private static void RegisterServices<TServiceBase>(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            var serviceTypes = assemblies
                .SelectMany(a => a.GetLoadableTypes())
                .Where(t => t.IsServiceType<TServiceBase>());

            foreach (var serviceType in serviceTypes)
            {
                builder.RegisterServiceWithContainer(serviceType);
            }
        }
    }
}
