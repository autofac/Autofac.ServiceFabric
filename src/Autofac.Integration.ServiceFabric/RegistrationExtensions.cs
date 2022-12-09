// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Microsoft.ServiceFabric.Actors.Runtime;

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
        /// <param name="constructorExceptionCallback">Callback will be invoked if an exception is thrown during resolving.</param>
        /// <param name="configurationAction">Callback will be invoked while configuring the lifetime scope for a service.</param>
        public static void RegisterServiceFabricSupport(
            this ContainerBuilder builder,
            Action<Exception>? constructorExceptionCallback = null,
            Action<ContainerBuilder>? configurationAction = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.Properties.ContainsKey(MetadataKey))
            {
                return;
            }

            builder.AddInternalRegistrations(constructorExceptionCallback, configurationAction);

            builder.Properties.Add(MetadataKey, true);
        }

        internal static IRegistrationBuilder<TService, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterServiceWithInterception<TService, TInterceptor>(
                this ContainerBuilder builder,
                object? lifetimeScopeTag = null)
            where TService : class
            where TInterceptor : IInterceptor
        {
            return builder.RegisterType<TService>()
                .InstancePerMatchingLifetimeScope(lifetimeScopeTag ?? Constants.DefaultLifetimeScopeTag)
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
                    registration.Ownership == InstanceOwnership.OwnedByLifetimeScope)
                {
                    return;
                }

                var message = typeof(TService).GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage();
                throw new InvalidOperationException(message);
            });
        }

        private static void AddInternalRegistrations(
            this ContainerBuilder builder,
            Action<Exception>? constructorExceptionCallback = null,
            Action<ContainerBuilder>? configurationAction = null)
        {
            var exceptionCallback = constructorExceptionCallback ?? (ex => { });
            var configurationCallback = configurationAction ?? (_ => { });

            builder.RegisterType<ActorInterceptor>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ServiceInterceptor>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ActorFactoryRegistration>()
                .As<IActorFactoryRegistration>()
                .WithParameter(TypedParameter.From(exceptionCallback))
                .WithParameter(TypedParameter.From(configurationCallback))
                .SingleInstance();

            builder.RegisterType<StatelessServiceFactoryRegistration>()
                .As<IStatelessServiceFactoryRegistration>()
                .WithParameter(TypedParameter.From(exceptionCallback))
                .WithParameter(TypedParameter.From(configurationCallback))
                .SingleInstance();

            builder.RegisterType<StatefulServiceFactoryRegistration>()
                .As<IStatefulServiceFactoryRegistration>()
                .WithParameter(TypedParameter.From(exceptionCallback))
                .WithParameter(TypedParameter.From(configurationCallback))
                .SingleInstance();

            builder.RegisterType<ActorService>()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}
