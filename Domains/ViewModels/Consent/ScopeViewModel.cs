// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace EventAuthServer.Domains.ViewModels.Identity
{
    /// <summary>
    /// 
    /// </summary>

    public class ScopeViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Emphasize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Checked { get; set; }
    }
}
