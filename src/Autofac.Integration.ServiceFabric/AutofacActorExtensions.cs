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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Extras.DynamicProxy;
using Autofac.Util;
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
        /// <typeparam name="TActor">The type of the actor to register.</typeparam>
        /// <exception cref="ArgumentException">Thrown when <typeparamref name="TActor"/> is not a valid actor type.</exception>
        /// <remarks>The actor will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterActor<TActor>(this ContainerBuilder builder) where TActor : ActorBase
        {
            builder.RegisterActor(typeof(TActor));
        }

        /// <summary>
        /// Registers an actor service with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="actorType">The type of the actor to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="actorType"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="actorType"/> is not a valid actor type.</exception>
        /// <remarks>The actor will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterActor(this ContainerBuilder builder, Type actorType)
        {
            if (actorType == null)
                throw new ArgumentNullException(nameof(actorType));

            if (!actorType.IsActorType())
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The type {0} is not a valid actor type", actorType.FullName), nameof(actorType));

            builder.RegisterActorWithContainer(actorType);
        }

        /// <summary>
        /// Registers all valid actor services found in the specified assemblies with the container.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="assemblies">The assemblies to search for actor services.</param>
        /// <remarks>The actor will be wrapped in a dynamic proxy and must be public and not sealed.</remarks>
        public static void RegisterActors(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            var actorTypes = assemblies
                .SelectMany(a => a.GetLoadableTypes())
                .Where(t => t.IsActorType());

            foreach (var actorType in actorTypes)
            {
                builder.RegisterActorWithContainer(actorType);
            }
        }

        /// <summary>
        /// Registers an actor service factory with Service Fabric for creating instances of <typeparamref name="TActor"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <typeparam name="TActor">The type of the actor service.</typeparam>
        public static void RegisterActorServiceFactory<TActor>(this IContainer container) where TActor : ActorBase
        {
            ActorRuntime.RegisterActorAsync<TActor>((context, actorTypeInfo) =>
            {
                return new ActorService(context, actorTypeInfo, (actorService, actorId) =>
                {
                    var lifetimeScope = container.BeginLifetimeScope();
                    var actor = lifetimeScope.Resolve<TActor>(
                        TypedParameter.From(actorService),
                        TypedParameter.From(actorId));
                    return actor;
                });
            }).GetAwaiter().GetResult();
        }

        private static bool IsActorType(this Type type)
        {
            return type.IsAssignableTo<ActorBase>()
                && type.IsClass
                && type.IsPublic
                && !type.IsSealed
                && !type.IsAbstract;
        }

        private static void RegisterActorWithContainer(this ContainerBuilder builder, Type actorType)
        {
            builder.RegisterType(actorType)
                .InstancePerLifetimeScope()
                .EnableClassInterceptors()
                .InterceptedBy(typeof(AutofacActorInterceptor));
        }
    }
}
