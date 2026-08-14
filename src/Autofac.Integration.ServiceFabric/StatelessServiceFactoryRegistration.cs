// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Fabric;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Autofac.Integration.ServiceFabric;

/// <summary>
/// Default implementation of <see cref="IStatelessServiceFactoryRegistration"/>.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
[SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated at runtime via dependency injection")]
internal sealed class StatelessServiceFactoryRegistration : IStatelessServiceFactoryRegistration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatelessServiceFactoryRegistration"/> class.
    /// </summary>
    /// <param name="constructorExceptionCallback">
    /// Callback will be invoked if an exception is thrown during resolving. The
    /// lifetime scope created for the service is passed to the callback and is
    /// disposed once the callback completes.
    /// </param>
    /// <param name="configurationAction">Callback will be invoked while configuring the lifetime scope for a service.</param>
    // ReSharper disable once UnusedMember.Global
    public StatelessServiceFactoryRegistration(
        Action<ILifetimeScope, Exception> constructorExceptionCallback,
        Action<ContainerBuilder> configurationAction)
    {
        ConstructorExceptionCallback = constructorExceptionCallback;
        ConfigurationAction = configurationAction;
    }

    /// <summary>
    /// Gets a callback that will be invoked if an exception is thrown during resolving.
    /// </summary>
    internal Action<ILifetimeScope, Exception> ConstructorExceptionCallback
    {
        get;
    }

    /// <summary>
    /// Gets a callback that will be invoked while configuring the lifetime scope for a service.
    /// </summary>
    internal Action<ContainerBuilder> ConfigurationAction
    {
        get;
    }

    /// <inheritdoc />
    public void RegisterStatelessServiceFactory<TService>(
        ILifetimeScope lifetimeScope, string serviceTypeName, object? lifetimeScopeTag = null)
        where TService : StatelessService
    {
        ServiceRuntime.RegisterServiceAsync(serviceTypeName, context =>
        {
            var tag = lifetimeScopeTag ?? Constants.DefaultLifetimeScopeTag;
            var serviceScope = lifetimeScope.BeginLifetimeScope(tag, builder =>
            {
                builder.RegisterInstance(context)
                    .As<StatelessServiceContext>()
                    .As<ServiceContext>();

                ConfigurationAction(builder);
            });

            return ServiceScopeResolver.ResolveOrDispose<TService>(serviceScope, ConstructorExceptionCallback);
        }).GetAwaiter().GetResult();
    }
}
