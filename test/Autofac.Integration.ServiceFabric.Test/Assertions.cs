using Autofac.Core;
using Xunit;

namespace Autofac.Integration.ServiceFabric.Test
{
    internal static class Assertions
    {
        internal static void AssertRegistered<TService>(this IComponentContext context)
        {
            Assert.True(context.IsRegistered<TService>());
        }

        internal static void AssertSharing<TComponent>(this IComponentContext context, InstanceSharing sharing)
        {
            var registration = context.RegistrationFor<TComponent>();
            Assert.Equal(sharing, registration.Sharing);
        }

        internal static void AssertLifetime<TComponent, TLifetime>(this IComponentContext context)
        {
            var registration = context.RegistrationFor<TComponent>();
            Assert.IsType<TLifetime>(registration.Lifetime);
        }

        internal static void AssertOwnership<TComponent>(this IComponentContext context, InstanceOwnership ownership)
        {
            var registration = context.RegistrationFor<TComponent>();
            Assert.Equal(ownership, registration.Ownership);
        }

        internal static IComponentRegistration RegistrationFor<TComponent>(this IComponentContext context)
        {
            Assert.True(context.ComponentRegistry.TryGetRegistration(new TypedService(typeof(TComponent)), out var registration));
            return registration;
        }
    }
}
