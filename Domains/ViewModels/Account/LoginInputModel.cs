// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;

namespace EventAuthServer.Domains.ViewModels.Identity
{
    /// <summary>
    /// 
    /// </summary>

    public class LoginInputModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Username { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Password { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool RememberLogin { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}