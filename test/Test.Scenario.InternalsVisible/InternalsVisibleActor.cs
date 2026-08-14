// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Test.Scenario.InternalsVisible;

// ReSharper disable once UnusedMember.Global
internal class InternalsVisibleActor : Actor
{
    public InternalsVisibleActor(ActorService actorService, ActorId actorId)
        : base(actorService, actorId)
    {
    }
}
