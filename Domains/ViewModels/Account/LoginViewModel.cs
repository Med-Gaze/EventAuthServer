// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;

namespace EventAuthServer.Domains.ViewModels.Identity
{
    /// <summary>
    /// 
    /// </summary>

    public class LoginViewModel : LoginInputModel
    {
        /// <summary>
        /// 
        /// </summary>
        public bool AllowRememberLogin { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public bool EnableLocalLogin { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !String.IsNullOrWhiteSpace(x.DisplayName));
        /// <summary>
        /// 
        /// </summary>

        /// <summary>
        /// 
        /// </summary>
        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
        /// <summary>
        /// 
        /// </summary>
        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;
    }
}