// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.ServiceFabric;

/// <summary>
/// Resolves service and actor instances from the lifetime scope created for them.
/// </summary>
internal static class ServiceScopeResolver
{
    /// <summary>
    /// Resolves an instance from the lifetime scope created for it, disposing that
    /// scope if the resolve operation fails.
    /// </summary>
    /// <param name="serviceScope">The lifetime scope created for the instance.</param>
    /// <param name="constructorExceptionCallback">
    /// Callback invoked if an exception is thrown during resolving.
    /// </param>
    /// <typeparam name="TService">The type to resolve.</typeparam>
    /// <returns>The resolved instance.</returns>
    /// <remarks>
    /// On success the scope is left alone; the interceptor disposes it when Service
    /// Fabric closes or aborts the instance. On failure there is nothing for the
    /// interceptor to intercept, so the scope is disposed here instead. The callback
    /// runs before disposal so it can resolve its own dependencies - a logger, for
    /// example - from the scope, and disposal happens even if the callback throws.
    /// </remarks>
    internal static TService ResolveOrDispose<TService>(
        ILifetimeScope serviceScope,
        Action<ILifetimeScope, Exception> constructorExceptionCallback)
        where TService : class
    {
        try
        {
            return serviceScope.Resolve<TService>();
        }
        catch (Exception ex)
        {
            try
            {
                constructorExceptionCallback(serviceScope, ex);
            }
            finally
            {
                serviceScope.Dispose();
            }

            throw;
        }
    }
}
