// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.ServiceFabric.Services.Runtime;

namespace Autofac.Integration.ServiceFabric
{
    /// <summary>
    /// Defines a registration for a factory that can create a stateful service.
    /// </summary>
    internal interface IStatefulServiceFactoryRegistration
    {
        /// <summary>
        /// Registers a factory for creating a stateful service.
        /// </summary>
        /// <param name="lifetimeScope">
        /// The root lifetime scope / container serving as the parent from which
        /// the service lifetime will be created.
        /// </param>
        /// <param name="serviceTypeName">ServiceTypeName as provided in service manifest.</param>
        /// <param name="lifetimeScopeTag">The tag applied to the <see cref="ILifetimeScope"/> in which the stateful service is hosted.</param>
        /// <typeparam name="TService">The type of the stateful service to register.</typeparam>
        void RegisterStatefulServiceFactory<TService>(
            ILifetimeScope lifetimeScope, string serviceTypeName, object? lifetimeScopeTag = null)
            where TService : StatefulServiceBase;
    }
}
