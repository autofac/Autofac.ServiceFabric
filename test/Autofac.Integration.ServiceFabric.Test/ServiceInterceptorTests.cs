// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using IInvocation = Castle.DynamicProxy.IInvocation;

namespace Autofac.Integration.ServiceFabric.Test;

public sealed class ServiceInterceptorTests
{
    [Theory]
    [InlineData("OnCloseAsync")]
    [InlineData("OnAbort")]
    public void DisposesLifetimeScopeWhenTriggerMethodInvoked(string methodName)
    {
        var lifetimeScope = Substitute.For<ILifetimeScope>();

        var method = Substitute.For<MethodInfo>();
        method.Name.Returns(methodName);

        var invocation = Substitute.For<IInvocation>();
        invocation.Method.Returns(method);

        var interceptor = new ServiceInterceptor(lifetimeScope);

        interceptor.Intercept(invocation);

        lifetimeScope.Received(1).Dispose();
        invocation.Received(1).Proceed();
    }
}
