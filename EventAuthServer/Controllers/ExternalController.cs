using EventAuthServer.Datum.Enum;
using EventAuthServer.Domains.ViewModels.Identity;
using EventAuthServer.Entity;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;
using med.common.library.constant;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventAuthServer.Controllers
{
    /// <summary>
    /// 
    /// </summary>

    [SecurityHeaders]
    [AllowAnonymous]
    public class ExternalController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly SignInManager<AppUserModel> _signInManager;
        private readonly UserManager<AppUserModel> _userManager;
        /// <summary>
        /// 
        /// </summary>

        public ExternalController(
            IIdentityServerInteractionService interaction,
            SignInManager<AppUserModel> signInManager,
            UserManager<AppUserModel> userManager
           )
        {
            _interaction = interaction;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public IActionResult Challenge(string scheme, string returnUrl)
        {
            var redirectUrl = Url.Action("Callback", "External",
                                   new { ReturnUrl = returnUrl });
            var props =
                _signInManager.ConfigureExternalAuthenticationProperties(scheme, redirectUrl);

            return Challenge(props, scheme);

        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Callback(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            LoginViewModel loginViewModel = new()
            {
                ReturnUrl = returnUrl
            };
            if (remoteError != null)
            {
                ModelState
                    .AddModelError(string.Empty, $"Error from external provider: {remoteError}");

                return RedirectToAction("Login", "Account", loginViewModel);
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "Error loading external login information.");

                return RedirectToAction("Login", "Account", loginViewModel);
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (!signInResult.Succeeded)
            {
                // Get the email claim value

                if (email != null)
                {
                    // Create a new user without password if we do not have a user already

                    if (user == null)
                    {
                        user = new AppUserModel
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            FullName = info.Principal.FindFirstValue(ClaimTypes.Name),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            EmailConfirmed = true,
                            Status = (int)AccountStatusEnum.Pending

                        };

                        await _userManager.CreateAsync(user);
                        await _userManager.AddToRoleAsync(user, IdentityRoleConstant.Default.ToString());
                    }

                    // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                    await _userManager.AddLoginAsync(user, info);

                }
                else
                {
                    throw new Exception("External authentication error");
                }
            }
            var role = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
                            {
                            new Claim("FullName", string.IsNullOrEmpty(user.FullName)? user.Email: user.FullName),
                            new Claim("Email", user.Email),
                            new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty),
                            new Claim("Role", role.FirstOrDefault()),
                            };

            await _signInManager.SignInWithClaimsAsync(user, isPersistent: true, claims);

            return LocalRedirect(returnUrl);
        }

    }
}