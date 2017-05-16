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
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Autofac.Integration.ServiceFabric
{
    /// <summary>
    /// Extension methods for registering actors with Autofac and Service Fabric.
    /// </summary>
    public static class AutofacActorExtensions
    {
        /// <summary>
        /// Registers an actor service with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="stateManagerFactory">A factory method to create <see cref="IActorStateManager"/>.</param>
        /// <param name="stateProvider">State provider to store the state for actor objects.</param>
        /// <param name="settings">/// Settings to configures behavior of Actor Service.</param>
        /// <typeparam name="TActor">The type of the actor to register.</typeparam>
        /// <exception cref="ArgumentException">Thrown when <typeparamref name="TActor"/> is not a valid actor type.</exception>
        /// <remarks>The actor will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterActor<TActor>(
            this ContainerBuilder builder,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null) where TActor : ActorBase
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var actorType = typeof(TActor);

            if (!actorType.CanBeProxied())
                throw new ArgumentException(actorType.GetInvalidProxyTypeErrorMessage());

            builder.RegisterServiceWithInterception<TActor, ActorInterceptor>();

            builder.RegisterBuildCallback(
                c => c.Resolve<IActorFactoryRegistration>().RegisterActorFactory<TActor>(
                    c, stateManagerFactory, stateProvider, settings));
        }
    }
}
