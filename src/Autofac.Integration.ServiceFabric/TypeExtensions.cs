// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using Castle.DynamicProxy;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Autofac.Integration.ServiceFabric;

/// <summary>
/// Extension methods for working with <see cref="Type"/>.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// Determines if a given type can be wrapped in a proxy.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to check.</param>
    /// <returns>
    /// <see langword="true"/> if the type can be wrapped in a proxy; otherwise <see langword="false"/>.
    /// </returns>
    internal static bool CanBeProxied(this Type type)
    {
        var open = type.IsClass && !type.IsSealed && !type.IsAbstract;
        var visible = type.IsPublic || ProxyUtil.IsAccessible(type);
        return open && visible;
    }

    /// <summary>
    /// Creates an error message for when a given type can't be proxied.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to include in the message.</param>
    /// <returns>A localized message for an exception.</returns>
    internal static string GetInvalidProxyTypeErrorMessage(this Type type)
    {
        return string.Format(CultureInfo.CurrentCulture, TypeExtensionsResources.InvalidProxyTypeErrorMessage, type.FullName);
    }

    /// <summary>
    /// Creates an error message for when a particular service type isn't registered instance per lifetime scope.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to include in the message.</param>
    /// <returns>A localized message for an exception.</returns>
    internal static string GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(this Type type)
    {
        return string.Format(CultureInfo.CurrentCulture, TypeExtensionsResources.ServiceNotRegisteredAsInstancePerLifetimeScope, type.FullName);
    }

    /// <summary>
    /// Creates an error message for when a given type isn't an <see cref="ActorService"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to include in the message.</param>
    /// <returns>A localized message for an exception.</returns>
    internal static string GetInvalidActorServiceTypeErrorMessage(this Type type)
    {
        return string.Format(CultureInfo.CurrentCulture, TypeExtensionsResources.InvalidActorServiceTypeErrorMessage, type.FullName);
    }
}
