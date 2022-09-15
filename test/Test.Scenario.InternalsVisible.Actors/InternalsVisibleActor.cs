using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Test.Scenario.InternalsVisible.Actors
{
    // ReSharper disable once UnusedMember.Global
    internal class InternalsVisibleActor : Actor
    {
        public InternalsVisibleActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }
    }
}
