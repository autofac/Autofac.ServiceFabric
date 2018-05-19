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
using System.Diagnostics.CodeAnalysis;
using System.Fabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Autofac.Integration.ServiceFabric
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated at runtime via dependency injection")]
    internal sealed class ActorFactoryRegistration : IActorFactoryRegistration
    {
        internal Action<Exception> ConstructorExceptionCallback { get; }

        // ReSharper disable once UnusedMember.Global
        public ActorFactoryRegistration(Action<Exception> constructorExceptionCallback)
        {
            ConstructorExceptionCallback = constructorExceptionCallback;
        }

        public void RegisterActorFactory<TActor>(
            ILifetimeScope container,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null)
            where TActor : ActorBase
        {
            RegisterActorFactory<TActor>(container, stateManagerFactory, stateProvider, settings, null);
        }

        public void RegisterActorFactory<TActor>(
            ILifetimeScope container,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null,
            object tag = null)
            where TActor : ActorBase
        {
            ActorRuntime.RegisterActorAsync<TActor>((context, actorTypeInfo) =>
            {
                return new ActorService(
                    context,
                    actorTypeInfo,
                    (actorService, actorId) =>
                    {
                        var lifetimeScope = container.BeginLifetimeScope(tag ?? "ServiceFabric", builder =>
                        {
                            builder.RegisterInstance(context)
                                .As<StatefulServiceContext>()
                                .As<ServiceContext>();
                            builder.RegisterInstance(actorService)
                                .As<ActorService>();
                            builder.RegisterInstance(actorId)
                                .As<ActorId>();
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
                    },
                    stateManagerFactory,
                    stateProvider,
                    settings);
            }).GetAwaiter().GetResult();
        }
    }
}
