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
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;

namespace Autofac.Integration.ServiceFabric
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    public static class RegistrationExtensions
    {
        private const string MetadataKey = "__ServiceFabricRegistered";

        /// <summary>
        /// Adds the core services required by the Service Fabric integration.
        /// </summary>
        /// <param name="builder">The container builder to register the services with.</param>
        /// <param name="constructorExceptionCallback">Callback will be invoked if there are an exception thrown during resolving.</param>
        public static void RegisterServiceFabricSupport(this ContainerBuilder builder, Action<Exception> constructorExceptionCallback = null)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            if (builder.Properties.ContainsKey(MetadataKey)) return;

            builder.RegisterModule(new ServiceFabricModule(constructorExceptionCallback));

            builder.Properties.Add(MetadataKey, true);
        }

        internal static IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterServiceWithInterception<TService, TInterceptor>(
                this ContainerBuilder builder,
                object tag)
            where TService : class
            where TInterceptor : IInterceptor
        {
            return builder.RegisterType<TService>()
                .InstancePerMatchingLifetimeScope(tag ?? "ServiceFabric")
                .EnableClassInterceptors()
                .InterceptedBy(typeof(TInterceptor));
        }

        internal static IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            EnsureRegistrationIsInstancePerLifetimeScope<TService>(
                this IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle> builder)
            where TService : class
        {
            return builder.OnRegistered(args =>
            {
                var registration = args.ComponentRegistration;

                if (registration.Lifetime.GetType() == typeof(MatchingScopeLifetime) &&
                    registration.Sharing == InstanceSharing.Shared &&
                    registration.Ownership == InstanceOwnership.OwnedByLifetimeScope) return;

                var message = typeof(TService).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage();
                throw new InvalidOperationException(message);
            });
        }
    }
}
