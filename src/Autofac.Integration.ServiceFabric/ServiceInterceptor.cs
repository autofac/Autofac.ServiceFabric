// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Castle.DynamicProxy;

namespace Autofac.Integration.ServiceFabric;

/// <summary>
/// Interceptor that proxies stateful and stateless services.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812", Justification = "Instantiated at runtime via dependency injection")]
internal sealed class ServiceInterceptor : IInterceptor
{
    private readonly ILifetimeScope _lifetimeScope;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceInterceptor"/> class.
    /// </summary>
    /// <param name="lifetimeScope">
    /// The owning lifetime scope to dispose on service close or abort.
    /// </param>
    public ServiceInterceptor(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    /// <inheritdoc/>
    [SuppressMessage("Microsoft.Design", "CA1062", Justification = "The method is only called by Dynamic Proxy and always with a valid parameter.")]
    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();

        var methodName = invocation.Method.Name;

        if (methodName == "OnCloseAsync" || methodName == "OnAbort")
        {
            _lifetimeScope.Dispose();
        }
    }
}
