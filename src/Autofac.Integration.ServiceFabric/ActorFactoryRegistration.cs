// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Fabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Autofac.Integration.ServiceFabric
{
    /// <summary>
    /// Default implementation of <see cref="IActorFactoryRegistration"/>.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated at runtime via dependency injection")]
    internal sealed class ActorFactoryRegistration : IActorFactoryRegistration
    {
        /// <summary>
        /// Gets a callback that will be invoked if an exception is thrown during resolving.
        /// </summary>
        internal Action<Exception> ConstructorExceptionCallback { get; }

        /// <summary>
        /// Gets a callback that will be invoked while configuring the lifetime scope for a service.
        /// </summary>
        internal Action<ContainerBuilder> ConfigurationAction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActorFactoryRegistration"/> class.
        /// </summary>
        /// <param name="constructorExceptionCallback">Callback will be invoked if an exception is thrown during resolving.</param>
        /// <param name="configurationAction">Callback will be invoked while configuring the lifetime scope for a service.</param>
        // ReSharper disable once UnusedMember.Global
        public ActorFactoryRegistration(
            Action<Exception> constructorExceptionCallback,
            Action<ContainerBuilder> configurationAction)
        {
            ConstructorExceptionCallback = constructorExceptionCallback;
            ConfigurationAction = configurationAction;
        }

        /// <inheritdoc />
        public void RegisterActorFactory<TActor>(
            ILifetimeScope container,
            Type actorServiceType,
            Func<ActorBase, IActorStateProvider, IActorStateManager>? stateManagerFactory = null,
            IActorStateProvider? stateProvider = null,
            ActorServiceSettings? settings = null,
            object? lifetimeScopeTag = null)
            where TActor : ActorBase
        {
            ActorRuntime.RegisterActorAsync<TActor>((context, actorTypeInfo) =>
            {
                ActorBase ActorFactory(ActorService actorService, ActorId actorId)
                {
                    var tag = lifetimeScopeTag ?? Constants.DefaultLifetimeScopeTag;
                    var lifetimeScope = container.BeginLifetimeScope(tag, builder =>
                    {
                        builder.RegisterInstance(context)
                            .As<StatefulServiceContext>()
                            .As<ServiceContext>();
                        builder.RegisterInstance(actorService)
                            .As<ActorService>();
                        builder.RegisterInstance(actorId)
                            .As<ActorId>();

                        ConfigurationAction(builder);
                    });

                    try
                    {
                        var actor = lifetimeScope.Resolve<TActor>();
                        return actor;
                    }
                    catch (Exception ex)
                    {
                        // Proactively dispose lifetime scope as interceptor will not be called.
                        lifetimeScope.Dispose();

                        ConstructorExceptionCallback(ex);
                        throw;
                    }
                }

                return (ActorService)container.Resolve(
                    actorServiceType,
                    new TypedParameter(typeof(StatefulServiceContext), context),
                    new TypedParameter(typeof(ActorTypeInformation), actorTypeInfo),
                    new TypedParameter(typeof(Func<ActorService, ActorId, ActorBase>), (Func<ActorService, ActorId, ActorBase>)ActorFactory),
                    new TypedParameter(typeof(Func<ActorBase, IActorStateProvider, IActorStateManager>), stateManagerFactory),
                    new TypedParameter(typeof(IStateProvider), stateProvider),
                    new TypedParameter(typeof(ActorServiceSettings), settings));
            }).GetAwaiter().GetResult();
        }
    }
}
