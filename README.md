# Autofac.ServiceFabric

Service Fabric support for [Autofac](https://autofac.org).

[![Build status](https://ci.appveyor.com/api/projects/status/pwrw1chyf0c2hlj1?svg=true)](https://ci.appveyor.com/project/Autofac/autofac-servicefabric)

Please file issues and pull requests for this package in this repository rather than in the Autofac core repo.

- [Documentation](https://autofac.readthedocs.io/en/latest/integration/servicefabric.html)
- [NuGet](https://www.nuget.org/packages/Autofac.ServiceFabric/)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)
- [Open in Visual Studio Code](https://open.vscode.dev/autofac/Autofac.ServiceFabric)

## Quick Start

In your `Main` program method, build up your container and register services using the Autofac extensions. This will attach service registrations from the container and the `ServiceRuntime`. Dispose of the container at app shutdown.

```csharp
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Autofac;
using Autofac.Integration.ServiceFabric;

namespace DemoService
{
  public static class Program
  {
      private static void Main()
      {
        try
        {
          // The ServiceManifest.xml file defines one or more service type names.
          // Registering a service maps a service type name to a .NET type.
          // When Service Fabric creates an instance of this service type,
          // an instance of the class is created in this host process.

          // Start with the trusty old container builder.
          var builder = new ContainerBuilder();

          // Register any regular dependencies.
          builder.RegisterModule(new LoggerModule(ServiceEventSource.Current.Message));

          // Register the Autofac magic for Service Fabric support.
          builder.RegisterServiceFabricSupport();

          // Register a stateless service...
          builder.RegisterStatelessService<DemoStatelessService>("DemoStatelessServiceType");

          // ...and/or register a stateful service.
          // builder.RegisterStatefulService<DemoStatefulService>("DemoStatefulServiceType");

          using (builder.Build())
          {
            ServiceEventSource.Current.ServiceTypeRegistered(
              Process.GetCurrentProcess().Id,
              typeof(DemoStatelessService).Name);

            // Prevents this host process from terminating so services keep running.
            Thread.Sleep(Timeout.Infinite);
          }
      }
      catch (Exception e)
      {
        ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
        throw;
      }
    }
  }
}
```

[Check out the documentation](https://autofac.readthedocs.io/en/latest/integration/servicefabric.html) for more usage details.

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).
