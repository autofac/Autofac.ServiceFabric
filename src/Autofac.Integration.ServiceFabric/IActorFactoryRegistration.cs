// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.ServiceFabric.Actors.Runtime;

namespace Autofac.Integration.ServiceFabric
{
    internal interface IActorFactoryRegistration
    {
        void RegisterActorFactory<TActor>(
            ILifetimeScope lifetimeScope,
            Type actorServiceType,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null,
            object lifetimeScopeTag = null)
            where TActor : ActorBase;
    }
}
