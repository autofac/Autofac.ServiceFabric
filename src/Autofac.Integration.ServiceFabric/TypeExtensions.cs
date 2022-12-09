// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
using Castle.DynamicProxy;

namespace Autofac.Integration.ServiceFabric
{
    internal static class TypeExtensions
    {
        internal static bool CanBeProxied(this Type type)
        {
            var open = type.IsClass && !type.IsSealed && !type.IsAbstract;
            var visible = type.IsPublic || ProxyUtil.IsAccessible(type);
            return open && visible;
        }

        internal static string GetInvalidProxyTypeErrorMessage(this Type type)
        {
            return string.Format(CultureInfo.CurrentCulture, TypeExtensionsResources.InvalidProxyTypeErrorMessage, type.FullName);
        }

        internal static string GetServiceNotRegisteredAsInstancePerLifetimeScopeMessage(this Type type)
        {
            return string.Format(CultureInfo.CurrentCulture, TypeExtensionsResources.ServiceNotRegisteredAsIntancePerLifetimeScope, type.FullName);
        }

        internal static string GetInvalidActorServiceTypeErrorMessage(this Type type)
        {
            return string.Format(CultureInfo.CurrentCulture, TypeExtensionsResources.InvalidActorServiceTypeErrorMessage, type.FullName);
        }
    }
}
