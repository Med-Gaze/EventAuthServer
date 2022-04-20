// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using EventAuthServer;
using EventAuthServer.Datum.Enum;
using EventAuthServer.Datum.Static;
using EventAuthServer.Domains.ViewModels.Identity;
using EventAuthServer.Entity;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using med.common.api.library.fileupload;
using med.common.library.configuration.service;
using med.common.library.constant;
using med.common.library.Enum;
using med.common.library.model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventAuthServer.Controllers
{
    /// <summary>
    /// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
    /// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
    /// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
    /// </summary>
    /// 
    [SecurityHeaders]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUserModel> userManager;
        private readonly SignInManager<AppUserModel> signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IConfiguration configuration;
        private readonly IEmailService _emailService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="interaction"></param>
        /// <param name="clientStore"></param>
        /// <param name="schemeProvider"></param>
        /// <param name="events"></param>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="configuration"></param>
        /// <param name="emailService"></param>
        /// 
       
        public AccountController(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            UserManager<AppUserModel> userManager,
            SignInManager<AppUserModel> signInManager,
            IConfiguration configuration, IEmailService emailService)
        {
            // if the TestUserStore is not in DI, then we'll just use the global users collection
            // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _emailService = emailService;

        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

            // the user clicked the "cancel" button
            if (button != "login")
            {
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return View("Redirect", new RedirectViewModel { RedirectUrl = model.ReturnUrl });
                    }
                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null

                    return Redirect(model.ReturnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                var user = await this.userManager.FindByEmailAsync(model.Username);
                if (user == null)
                {
                    var userByUserName = await this.userManager.FindByNameAsync(model.Username);
                    if (userByUserName == null)
                    {
                        ModelState.AddModelError(model.Username, $"No Accounts Registered with {model.Username}.");
                    }
                    else
                    {
                        user = userByUserName;
                    }
                    
                }

                if (user != null)
                {
                    var result = await this.signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberLogin, lockoutOnFailure: false);
                    int maxfailed = this.configuration.GetSection("JWTToken:MaxFailedAccess").Get<int>();
                    if (result.IsLockedOut)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "Account has been locked.", clientId: context?.Client.ClientId));
                        ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                    }
                    else if (result.IsNotAllowed)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, $"Not allowed to login for {model.Username}", clientId: context?.Client.ClientId));
                        ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                    }
                    else if (result.RequiresTwoFactor)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, $"Required two factor login process for { model.Username}", clientId: context?.Client.ClientId));
                        ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                    }
                    else if (!result.Succeeded)
                    {
                        await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, $"Invalid Credentials for '{model.Username}'. Account will have locked after { maxfailed - user.AccessFailedCount} wrong attempt", clientId: context?.Client.ClientId));
                        ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                    }
                    else
                    {
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));
                        // only set explicit expiration here if user chooses "remember me". 
                        // otherwise we rely upon expiration configured in cookie middleware.
                        AuthenticationProperties props = null;
                        if (AccountOptions.AllowRememberLogin)
                        {
                            props = new AuthenticationProperties
                            {
                                IsPersistent = model.RememberLogin,
                                ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                            };
                        };

                        // issue authentication cookie with subject ID and username
                        var isuser = new IdentityServerUser(user.Id)
                        {
                            DisplayName = user.UserName
                        };

                        var hasClientCall = HttpContext.Request.QueryString.HasValue && HttpContext.Request.QueryString.Value.Contains("client_id");

                        if (!hasClientCall)
                        {
                            var role = await this.userManager.GetRolesAsync(user);
                            var claims = new List<Claim>
                            {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(JwtClaimTypes.Subject, user.Id),
                            new Claim("FullName", user.FullName),
                            new Claim(ClaimTypes.Role, role.FirstOrDefault()),
                            };

                            var claimsIdentity = new ClaimsIdentity(
                                claims, IdentityServerConstants.LocalApi.AuthenticationScheme);

                            await HttpContext.SignInAsync(IdentityServerConstants.LocalApi.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), props);
                        }
                        else
                        {
                            await HttpContext.SignInAsync(isuser, props);
                        }
                        if (context != null)
                        {
                            if (context.IsNativeClient())
                            {
                                // The client is native, so this change in how to
                                // return the response is for better UX for the end user.
                                return this.LoadingPage("Redirect", model.ReturnUrl);
                            }

                            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                            return Redirect(model.ReturnUrl);
                        }
                        // request for a local page
                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }
                        else if (string.IsNullOrEmpty(model.ReturnUrl))
                        {
                            return Redirect("~/");
                        }
                        else
                        {
                            // user might have clicked on a malicious link - should be logged
                            throw new Exception("invalid return URL");
                        }
                    }
                }
                else
                {
                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client.ClientId));
                    ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);

                }

            }

            // something went wrong, show form with error
            var vm = await BuildLoginViewModelAsync(model);
            return View(vm);
        }


        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            // build a model so the logout page knows what to display
            var vm = await BuildLogoutViewModelAsync(logoutId);

            if (vm.ShowLogoutPrompt == false)
            {
                // if the request for logout was properly authenticated from IdentityServer, then
                // we don't need to show the prompt and can just log the user out directly.
                return await Logout(vm);
            }

            return View(vm);
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();
                await signInManager.SignOutAsync();

                await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = Url.Action("Logout", new { logoutId = vm.LogoutId });

                // this triggers a redirect to the external provider for sign-out
                return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme);
            }

            return vm.PostLogoutRedirectUri != null ? Redirect(vm.PostLogoutRedirectUri) : View("LoggedOut", vm);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildRegisterViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await this.userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "User already registered with same email.");
                var vm = await BuildRegisterViewModelAsync(model.ReturnUrl);
                return View(vm);
            }

            var user = new AppUserModel
            {
                UserName = model.Email,
                FullName = string.Join(" ", model.FirstName, model.MiddleName, model.LastName),
                NickName = model.CalledName,
                Email = model.Email,
                LockoutEnabled = true,
                NormalizedEmail = model.Email.ToUpper(),
                NormalizedUserName = model.Email.ToUpper(),
                PhoneNumber = model.PhoneNumber,
                Status = (int)AccountStatusEnum.Pending
            };

            var userResult = await this.userManager.CreateAsync(user, model.Password);

            if (!userResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, userResult.Errors.FirstOrDefault().Description);
                var vm = await BuildRegisterViewModelAsync(model.ReturnUrl);
                return View(vm);
            }

            var roleResult = await this.userManager.AddToRoleAsync(user, IdentityRoleConstant.Default.ToString());
            if (!roleResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, userResult.Errors.FirstOrDefault().Description);
                var vm = await BuildRegisterViewModelAsync(model.ReturnUrl);
                return View(vm);
            }
            var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);
            var callBack = Url.Action(nameof(ConfirmEmail), "Account", new { token, email = user.Email, returnUrl = model.ReturnUrl }, Request.Scheme);
            var confirmationLink = $"This is a link to confirm your account." +
                       $"<br/> Click Below! <br/>" +
                       $"<a style='padding:12px 28px; margin-top:30px; border-radius: 4px; background-color: #4CAF50;" +
                       $" color:#ffff;text-decoration: none;text-align: center;border: none;" +
                       $"display: inline-block;cursor: pointer;' href = '{callBack}'>Click here to confirm</a>";

            var emailConfig = this.configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();

            var emailModel = new EmailViewModel(emailTo: user.Email, subject: "Confirm Account Link",
                  content: confirmationLink, contentType: (int)EmailContentTypeEnum.Html);

            try
            {
                await _emailService.SendMail(emailModel, emailConfig);

            }
            catch (Exception)
            {
                ModelState.AddModelError("Email", $"Server issue.");
                return View();
            }
            TempData["success"] = $"{model.Email} has created.";
            return Redirect(model.ReturnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword(string returnUrl)
        {
            var vm = new ForgotPasswordViewModel
            {
                ReturnUrl = returnUrl,
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPassword)
        {
            if (!ModelState.IsValid) return View(forgotPassword);

            var user = await this.userManager.FindByEmailAsync(forgotPassword.Email);

            if (user == null)
            {
                ModelState.AddModelError("Email", $"{forgotPassword.Email} not found.");
                return View(forgotPassword);
            }


            var token = await this.userManager.GeneratePasswordResetTokenAsync(user);
            var callBack = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email, returnUrl = forgotPassword.ReturnUrl }, Request.Scheme);
            var confirmationLink = $"This is a link to reset your password." +
                       $"<br/> Click Below! <br/>" +
                       $"<a style='padding:12px 28px; margin-top:30px; border-radius: 4px; background-color: #4CAF50;" +
                       $" color:#ffff;text-decoration: none;text-align: center;border: none;" +
                       $"display: inline-block;cursor: pointer;' href = '{callBack}'>Click here to reset password</a>";

            var emailConfig = this.configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();

            var emailModel = new EmailViewModel(emailTo: user.Email, subject: "Reset Password Link",
                  content: confirmationLink, contentType: (int)EmailContentTypeEnum.Html);

            try
            {
                await _emailService.SendMail(emailModel, emailConfig);

            }
            catch (Exception)
            {
                ModelState.AddModelError("Email", $"Server issue.");
                return View();
            }

            TempData["success"] = $"{forgotPassword.Email} password reset link has send to email. please check it.";
            return Redirect(forgotPassword.ReturnUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email, string returnUrl)
        {
            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email,
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await this.userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound($"{model.Email} not found.");

            var resetPasswordResult = await this.userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (!resetPasswordResult.Succeeded)
            {
                foreach (var error in resetPasswordResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View();
            }

            TempData["success"] = $"{model.Email} password has reset successfully.";

            return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var token = model.Token;
                var emailId = model.Email;
                var user = await this.userManager.FindByEmailAsync(emailId);
                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        TempData["success"] = $"{model.Email} already confirmed.";
                        return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
                    }
                    var result = await this.userManager.ConfirmEmailAsync(user, token);
                    if (result.Succeeded)
                    {
                        user.Status = (int)AccountStatusEnum.Requested;
                        await this.userManager.UpdateAsync(user);
                        TempData["success"] = $"{model.Email} confirmed.";
                        return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
                    }
                }
            }
            return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
        }


        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword(string returnUrl)
        {
            return View(new ChangePasswordViewModel
            {
                ReturnUrl = returnUrl
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var userEmail = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var user = await this.userManager.FindByEmailAsync(userEmail);

            var resetPasswordResult = await this.userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);

            if (!resetPasswordResult.Succeeded)
            {
                foreach (var error in resetPasswordResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View();
            }

            TempData["success"] = $"{userEmail} password has changed.";

            return Redirect(model.ReturnUrl);
        }


        private async Task<RegisterViewModel> BuildRegisterViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new RegisterViewModel
                {
                    EnableLocalLogin = local,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes.Where(x => x.DisplayName != null || (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new RegisterViewModel
            {
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ExternalProviders = providers.ToArray(),
                ReturnUrl = returnUrl
            };
        }

        /*****************************************/
        /* helper APIs for the AccountController */
        /*****************************************/
        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

            if (User?.Identity.IsAuthenticated != true)
            {
                // if the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (User?.Identity.IsAuthenticated == true)
            {
                var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
                {
                    var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need to create one
                            // this captures necessary info from the current logged in user
                            // before we signout and redirect away to the external IdP for signout
                            vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }

    }
}
