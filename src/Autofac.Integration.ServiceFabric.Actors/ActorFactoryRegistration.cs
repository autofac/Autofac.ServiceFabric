// This software is part of the Autofac IoC container
// Copyright © 2017 Autofac Contributors
// https://autofac.org
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

namespace Autofac.Integration.ServiceFabric.Actors
{
    // ReSharper disable once ClassNeverInstantiated.Global
    [SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated at runtime via dependency injection")]
    internal sealed class ActorFactoryRegistration : IActorFactoryRegistration
    {
        internal Action<Exception> ConstructorExceptionCallback { get; }

        internal Action<ContainerBuilder> ConfigurationAction { get; }

        // ReSharper disable once UnusedMember.Global
        public ActorFactoryRegistration(
            Action<Exception> constructorExceptionCallback,
            Action<ContainerBuilder> configurationAction)
        {
            ConstructorExceptionCallback = constructorExceptionCallback;
            ConfigurationAction = configurationAction;
        }

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
