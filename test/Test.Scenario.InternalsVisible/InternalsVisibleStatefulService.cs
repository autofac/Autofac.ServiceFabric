using System.Fabric;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Test.Scenario.InternalsVisible
{
    // ReSharper disable once UnusedMember.Global
    internal class InternalsVisibleStatefulService : StatefulServiceBase
    {
        public InternalsVisibleStatefulService(StatefulServiceContext serviceContext, IStateProviderReplica2 stateProviderReplica) : base(serviceContext, stateProviderReplica)
        {
        }
    }
}
