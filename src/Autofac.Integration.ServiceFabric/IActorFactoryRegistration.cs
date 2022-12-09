// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.ServiceFabric.Actors.Runtime;

namespace Autofac.Integration.ServiceFabric
{
    /// <summary>
    /// Defines a registration for a factory that can create an actor service.
    /// </summary>
    internal interface IActorFactoryRegistration
    {
        /// <summary>
        /// Registers a factory for creating an actor service.
        /// </summary>
        /// <param name="lifetimeScope">
        /// The root lifetime scope / container serving as the parent from which
        /// the service lifetime will be created.
        /// </param>
        /// <param name="actorServiceType"><see cref="Type"/> of the actor service to create (defaults to <see cref="ActorService"/>).</param>
        /// <param name="stateManagerFactory">A factory method to create <see cref="IActorStateManager"/>.</param>
        /// <param name="stateProvider">State provider to store the state for actor objects.</param>
        /// <param name="settings">Settings to configures behavior of Actor Service.</param>
        /// <param name="lifetimeScopeTag">The tag applied to the <see cref="ILifetimeScope"/> in which the actor service is hosted.</param>
        /// <typeparam name="TActor">The type of the actor to register.</typeparam>
        void RegisterActorFactory<TActor>(
            ILifetimeScope lifetimeScope,
            Type actorServiceType,
            Func<ActorBase, IActorStateProvider, IActorStateManager>? stateManagerFactory = null,
            IActorStateProvider? stateProvider = null,
            ActorServiceSettings? settings = null,
            object? lifetimeScopeTag = null)
            where TActor : ActorBase;
    }
}
