using System.Fabric;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Test.Scenario.InternalsVisible
{
    // ReSharper disable once UnusedMember.Global
    internal class InternalsVisibleStatelessService : StatelessService
    {
        public InternalsVisibleStatelessService(StatelessServiceContext serviceContext) : base(serviceContext)
        {
        }
    }
}
