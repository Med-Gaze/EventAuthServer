
using System;
using System.Collections.Generic;

namespace EventAuthServer.Domains.ViewModels.Identity
{
    /// <summary>
    /// 
    /// </summary>
    public class GrantsViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<GrantViewModel> Grants { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class GrantViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string ClientId { get; set; }
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
        public string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? Expires { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> IdentityGrantNames { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> ApiGrantNames { get; set; }
    }
}