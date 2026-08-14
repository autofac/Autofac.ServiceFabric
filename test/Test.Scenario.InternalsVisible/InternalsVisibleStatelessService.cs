// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Fabric;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Test.Scenario.InternalsVisible;

// ReSharper disable once UnusedMember.Global
internal class InternalsVisibleStatelessService : StatelessService
{
    public InternalsVisibleStatelessService(StatelessServiceContext serviceContext)
        : base(serviceContext)
    {
    }
}
