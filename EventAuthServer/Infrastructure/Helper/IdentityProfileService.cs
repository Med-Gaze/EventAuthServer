using Dapper;
using EventAuthServer.Entity;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using med.common.library.constant;
using med.common.library.database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventAuthServer.Helper
{
    public class IdentityProfileService : IProfileService
    {
        private readonly UserManager<AppUserModel> _userManager;
        public IdentityProfileService(UserManager<AppUserModel> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var userClaims = new List<Claim>();
            var userId = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(userId);
            var today = DateTime.Today;

            // Calculate the age.
            var age = today.Year - user.DOB.Year; ;
            var role = await _userManager.GetRolesAsync(user);
            var fileId = user.FileId.HasValue ? user.FileId.ToString() : string.Empty;

            userClaims.AddRange(new List<Claim>
            {
                new Claim(ClaimTypes.Role, role.FirstOrDefault()),
                new Claim("Role", role.FirstOrDefault()),
                new Claim("FullName", user.FullName),
                new Claim("NickName", user.NickName??string.Empty),
                new Claim("ProfileFileId", fileId),
                new Claim("Email", user.Email),
                new Claim("Age", age.ToString()),
                new Claim("PhoneNumber", user.PhoneNumber??string.Empty),
            });

            context.IssuedClaims.AddRange(userClaims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }

    }
}
