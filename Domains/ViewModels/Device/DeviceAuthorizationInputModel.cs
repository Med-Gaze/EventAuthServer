// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using EventAuthServer.Domains.ViewModels.Identity;

namespace EventAuthServer.Domains.ViewModels.Identity
{
    /// <summary>
    /// 
    /// </summary>
    public class DeviceAuthorizationInputModel : ConsentInputModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string UserCode { get; set; }
    }
}