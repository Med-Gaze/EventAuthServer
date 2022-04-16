
using IdentityServer4.Models;

namespace EventAuthServer.Domains.ViewModels.Identity
{
    /// <summary>
    /// 
    /// </summary>

    public class ErrorViewModel
    {
        /// <summary>
        /// 
        /// </summary>

        public ErrorViewModel()
        {
        }
        /// <summary>
        /// 
        /// </summary>

        public ErrorViewModel(string error)
        {
            Error = new ErrorMessage { Error = error };
        }
        /// <summary>
        /// 
        /// </summary>

        public ErrorMessage Error { get; set; }
    }
}