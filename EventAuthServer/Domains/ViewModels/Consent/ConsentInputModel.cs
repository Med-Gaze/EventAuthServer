// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace EventAuthServer.Domains.ViewModels.Identity
{
    /// <summary>
        /// 
        /// </summary>
    public class ConsentInputModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Button { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> ScopesConsented { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool RememberConsent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReturnUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
    }
}