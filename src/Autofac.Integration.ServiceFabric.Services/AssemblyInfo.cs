using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]
[assembly: SuppressMessage("Microsoft.Design", "CA1020", Scope = "namespace", Target = "Autofac.Integration.ServiceFabric.Services")]
[assembly: InternalsVisibleTo("Autofac.Integration.ServiceFabric.Services.Test")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // Use this if we have a strongly named assembly ourselves [assembly: InternalsVisibleTo(InternalsVisible.ToDynamicProxyGenAssembly2)]
