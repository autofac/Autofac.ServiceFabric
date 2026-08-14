// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.ServiceFabric.Test;

public sealed class ServiceScopeResolverTests
{
    [Fact]
    public void ReturnsResolvedInstanceWithoutDisposingScope()
    {
        using var container = BuildContainer();
        var serviceScope = container.BeginLifetimeScope();
        var invoked = false;

        var resolved = ServiceScopeResolver.ResolveOrDispose<Resolvable>(serviceScope, (_, _) => invoked = true);

        Assert.NotNull(resolved);
        Assert.False(invoked);
        Assert.NotNull(serviceScope.Resolve<Resolvable>());
    }

    [Fact]
    public void PassesFailingScopeAndExceptionToCallback()
    {
        using var container = BuildContainer();
        var serviceScope = container.BeginLifetimeScope();
        ILifetimeScope? capturedScope = null;
        Exception? capturedException = null;

        var thrown = Assert.Throws<Autofac.Core.Registration.ComponentNotRegisteredException>(
            () => ServiceScopeResolver.ResolveOrDispose<Unresolvable>(serviceScope, (scope, ex) =>
            {
                capturedScope = scope;
                capturedException = ex;
            }));

        Assert.Same(serviceScope, capturedScope);
        Assert.Same(thrown, capturedException);
    }

    [Fact]
    public void CallbackCanResolveFromScopeBeforeItIsDisposed()
    {
        using var container = BuildContainer();
        var serviceScope = container.BeginLifetimeScope();
        Resolvable? resolvedInCallback = null;

        Assert.Throws<Autofac.Core.Registration.ComponentNotRegisteredException>(
            () => ServiceScopeResolver.ResolveOrDispose<Unresolvable>(
                serviceScope,
                (scope, _) => resolvedInCallback = scope.Resolve<Resolvable>()));

        Assert.NotNull(resolvedInCallback);
    }

    [Fact]
    public void DisposesScopeAfterCallbackCompletes()
    {
        using var container = BuildContainer();
        var serviceScope = container.BeginLifetimeScope();

        Assert.Throws<Autofac.Core.Registration.ComponentNotRegisteredException>(
            () => ServiceScopeResolver.ResolveOrDispose<Unresolvable>(serviceScope, (_, _) => { }));

        Assert.Throws<ObjectDisposedException>(() => serviceScope.Resolve<Resolvable>());
    }

    [Fact]
    public void DisposesScopeWhenCallbackThrows()
    {
        using var container = BuildContainer();
        var serviceScope = container.BeginLifetimeScope();
        var callbackException = new InvalidOperationException("Callback failed");

        var thrown = Assert.Throws<InvalidOperationException>(
            () => ServiceScopeResolver.ResolveOrDispose<Unresolvable>(serviceScope, (_, _) => throw callbackException));

        Assert.Same(callbackException, thrown);
        Assert.Throws<ObjectDisposedException>(() => serviceScope.Resolve<Resolvable>());
    }

    private static IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Resolvable>();
        return builder.Build();
    }

    public sealed class Resolvable
    {
    }

    public sealed class Unresolvable
    {
        public Unresolvable(Resolvable dependency)
        {
            Dependency = dependency;
        }

        public Resolvable Dependency
        {
            get;
        }
    }
}
