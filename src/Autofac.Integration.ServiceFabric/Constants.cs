// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.ServiceFabric
{
    /// <summary>
    /// Constants used in working with Autofac and Service Fabric.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// The name of the lifetime scope serving as the "root" for services.
        /// </summary>
        internal const string DefaultLifetimeScopeTag = "ServiceFabric";
    }
}
