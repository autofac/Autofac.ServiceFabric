// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using IInvocation = Castle.DynamicProxy.IInvocation;

namespace Autofac.Integration.ServiceFabric.Test;

public sealed class ActorInterceptorTests
{
    [Fact]
    public void DisposesLifetimeScopeWhenTriggerMethodInvoked()
    {
        var lifetimeScope = new Mock<ILifetimeScope>(MockBehavior.Strict);
        lifetimeScope.Setup(x => x.Dispose()).Verifiable();

        var invocation = new Mock<IInvocation>(MockBehavior.Strict);
        invocation.Setup(x => x.Proceed()).Verifiable();
        invocation.Setup(x => x.Method.Name).Returns("OnDeactivateAsync").Verifiable();

        var interceptor = new ActorInterceptor(lifetimeScope.Object);

        interceptor.Intercept(invocation.Object);

        lifetimeScope.Verify();
        invocation.Verify();
    }
}
