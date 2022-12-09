// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.ServiceFabric.Services.Runtime;

namespace Autofac.Integration.ServiceFabric
{
    internal interface IStatefulServiceFactoryRegistration
    {
        void RegisterStatefulServiceFactory<TService>(
            ILifetimeScope lifetimeScope, string serviceTypeName, object lifetimeScopeTag = null)
            where TService : StatefulServiceBase;
    }
}
