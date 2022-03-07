// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace EventAuthServer.Domains.ViewModels.Identity
{
    /// <summary>
    /// 
    /// </summary>
    public class ConsentViewModel : ConsentInputModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ClientName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClientUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ClientLogoUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool AllowRememberConsent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ScopeViewModel> ApiScopes { get; set; }
    }
}
