// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Fabric;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Autofac.Integration.ServiceFabric
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated at runtime via dependency injection")]
    internal sealed class StatefulServiceFactoryRegistration : IStatefulServiceFactoryRegistration
    {
        internal Action<Exception> ConstructorExceptionCallback { get; }

        internal Action<ContainerBuilder> ConfigurationAction { get; }

        // ReSharper disable once UnusedMember.Global
        public StatefulServiceFactoryRegistration(
            Action<Exception> constructorExceptionCallback,
            Action<ContainerBuilder> configurationAction)
        {
            ConstructorExceptionCallback = constructorExceptionCallback;
            ConfigurationAction = configurationAction;
        }

        public void RegisterStatefulServiceFactory<TService>(
            ILifetimeScope container, string serviceTypeName, object? lifetimeScopeTag = null)
            where TService : StatefulServiceBase
        {
            ServiceRuntime.RegisterServiceAsync(serviceTypeName, context =>
            {
                var tag = lifetimeScopeTag ?? Constants.DefaultLifetimeScopeTag;
                var lifetimeScope = container.BeginLifetimeScope(tag, builder =>
                {
                    builder.RegisterInstance(context)
                        .As<StatefulServiceContext>()
                        .As<ServiceContext>();

                    ConfigurationAction(builder);
                });

                try
                {
                    var service = lifetimeScope.Resolve<TService>();
                    return service;
                }
                catch (Exception ex)
                {
                    // Proactively dispose lifetime scope as interceptor will not be called.
                    lifetimeScope.Dispose();

                    ConstructorExceptionCallback(ex);
                    throw;
                }
            }).GetAwaiter().GetResult();
        }
    }
}
